using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ChoETL;
using Common.Models.Paging;
using FileRepository.AzureStorage.Exceptions;
using FileRepository.Convertors;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace FileRepository.AzureStorage
{
    public class AzureStorageFileRepository : IFileRepository
    {
        private static readonly ConcurrentDictionary<string, BlobContainerClient> BlobContainerClients = new();
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public AzureStorageFileRepository(
            BlobServiceClient blobServiceClient,
            string containerName)
        {
            _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
            if (string.IsNullOrEmpty(containerName)) throw new ArgumentException($"{nameof(containerName)} can not be empty");
            _containerName = containerName;
        }

        public async Task<Stream> GetAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = await GetBlobClientAsync(_containerName, filePath);
                return await client.OpenReadAsync(cancellationToken: cancellationToken);
            }
            catch (Exception e)
            {
                throw new FieldAccessException($"Error getting stream from location:{filePath}", e);
            }
        }

        public async Task<FileProperties> GetPropertiesAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = await GetBlobClientAsync(_containerName, filePath);
                var properties = await client.GetPropertiesAsync(cancellationToken: cancellationToken);
                var name = Path.GetFileName(filePath);
                return new FileProperties
                {
                    Name = name,
                    Size = properties.Value.ContentLength,
                    Created = properties.Value.CreatedOn,
                    Modified = properties.Value.LastModified
                };
            }
            catch (Exception e)
            {
                throw new FieldAccessException($"Error getting stream from location:{filePath}", e);
            }
        }

        public async Task<Connection<string>> GetFilesAsPagesAsync(string? prefixFolders = null, string? continuationToken = default,
            int? pageSizeHint = default,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var files = new List<string>();
                var blobContainerClient = await GetBlobContainerClient(_containerName);
                var blobPages = blobContainerClient.GetBlobs(prefix: prefixFolders).AsPages(continuationToken, pageSizeHint);

                var blobPage = blobPages.FirstOrDefault();
                if (blobPage != null)
                {              
                        var pageFiles = blobPage.Values.Select(x => x.Name);
                        files.AddRange(pageFiles);
                }
                var pageInfo = new AzureStoragePageInfo(continuationToken, blobPage?.ContinuationToken);
                var connection = new Connection<string>(files, pageInfo);

                return connection;

            }
            catch (Exception ex)
            {
                throw new BlobClientRetrievalException($"Failed to get files from blob container {_containerName} with paging", ex);
            }
        }

        public async Task<IEnumerable<string>> GetFilesAsync(string? prefixFolders = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var files = new List<string>();
                var blobContainerClient = await GetBlobContainerClient(_containerName);
                    var resultSegment = blobContainerClient.GetBlobsByHierarchyAsync(prefix: prefixFolders, delimiter: "/")
                        .AsPages(default, default);

                    await foreach (Page<BlobHierarchyItem> blobPage in resultSegment)
                    {
                        foreach (BlobHierarchyItem blobhierarchyItem in blobPage.Values)
                        {
                            if (blobhierarchyItem.IsPrefix)
                            {
                                var subFiles = await GetFilesAsync(blobhierarchyItem.Prefix, cancellationToken);
                                files.AddRange(subFiles);
                            }                
                            else
                            {
                                files.Add(blobhierarchyItem.Blob.Name);
                            }
                        }
                    }
                    return files;

            }
            catch (Exception ex)
            {
                throw new BlobClientRetrievalException($"Failed to get files from blob container {_containerName}", ex);
            }
        }

        public async Task<bool> DeleteIfExistsAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                var blobClient = await GetBlobClientAsync(_containerName, filePath);
                return await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            }
            catch (Exception e)
            {
                throw new FieldAccessException($"Error Deleting file:{filePath}", e);
            }
        }

        public async Task CreateAndUploadFileAsync(string filePath, JToken jObject, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    ObjectToCsvStream.Convert(jObject, memoryStream, false);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    await UploadFileAsync(filePath, memoryStream, overwrite: overwrite, cancellationToken: cancellationToken);
                }
            }
            catch (Exception e)
            {
                throw new FieldAccessException($"Error creating jObject to file:{filePath}", e);
            }
        }

        public async Task UploadFileAsync(string filePath, Stream content, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            try
            {
                var blobClient = await GetBlobClientAsync(_containerName, filePath);
                await blobClient.UploadAsync(content, overwrite: overwrite, cancellationToken: cancellationToken);

            }
            catch (Exception e)
            {
                throw new FieldAccessException($"Error creating jObject to file:{filePath}", e);
            }
        }

        public async Task<bool> FileExitsAsync(string filePath, CancellationToken cancellationToken = default)
        {
            var blobClient = await GetBlobClientAsync(_containerName, filePath);
            bool fileExists = await blobClient.ExistsAsync(cancellationToken: cancellationToken);
            return fileExists;
        }

        protected async Task<BlobClient> GetBlobClientAsync(string blobContainerName, string blobName)
        {
            try
            {
                var blobContainerClient = await GetBlobContainerClient(blobContainerName);
                var blobClient = blobContainerClient.GetBlobClient(blobName);
                return blobClient;
            }
            catch (Exception ex)
            {
                throw new BlobClientRetrievalException($"Failed to get blob {blobName} from blob container {blobContainerName}", ex);
            }
        }

        protected async Task<BlobContainerClient> GetBlobContainerClient(string blobContainerName)
        {
            if (!BlobContainerClients.ContainsKey(blobContainerName))
            {
                // Check container exists
                var containerClient = _blobServiceClient.GetBlobContainerClient(blobContainerName);
                if (!containerClient.Exists())
                {
                    await containerClient.CreateAsync();
                }
                BlobContainerClients.AddOrUpdate(blobContainerName, containerClient, (_, _) => containerClient);
            }
            return BlobContainerClients[blobContainerName];
        }
    }
}
