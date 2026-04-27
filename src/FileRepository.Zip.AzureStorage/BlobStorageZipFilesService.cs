using Azure.Storage.Blobs.Specialized;
using FileRepository.AzureStorage.Config;
using FileRepository.Zip;
using System.Configuration;
using System.Diagnostics;

namespace FileRepository.Zip.AzureStorage
{
    public class BlobStorageZipFilesService : IZipFiles
    {
        private readonly IFileRepository _fileRepository;
        private string _storageConnectionString;
        private string _containerName;

        public BlobStorageZipFilesService(
            IFileRepository fileRepository,
            string storageConnectionString,
            string storageContainerName)
        {
            _fileRepository = fileRepository;
            _storageConnectionString = storageConnectionString;
            _containerName = storageContainerName;
        }

        public async Task AddToZipAsync(IEnumerable<string> filePaths, string zipFullPath, int compressionLevel = 0, CancellationToken cancellationToken = default)
        {
            try
            {
                using BlobStorageZipper zipper = new BlobStorageZipper(_storageConnectionString, _containerName);

                await zipper.CreateZipAsync(zipFullPath, compressionLevel, cancellationToken);

                foreach (var filePath in filePaths)
                {
                    var blockBlobClient = GetBlockBlobClient(filePath);
                    await zipper.AddToZipAsync(blockBlobClient);
                }
            }
            catch (TaskCanceledException)
            {
                await GetBlockBlobClient(zipFullPath).DeleteIfExistsAsync();
                throw;
            }
        }

        public async Task<Stream> AddToZipStreamAsync(IEnumerable<string> filePaths, int compressionLevel = 0, CancellationToken cancellationToken = default)
        {
            var zipFilePath = $"{DateTime.UtcNow:yyyyMMddHHmmss}.{Guid.NewGuid().ToString().Substring(0, 4)}.zip";

            await AddToZipAsync(filePaths, zipFilePath, compressionLevel, cancellationToken);

            return await _fileRepository.GetAsync(zipFilePath, cancellationToken);
        }

        public async Task<long> GetTotalEntriesAsync(string zipFullPath, CancellationToken cancellationToken = default)
        {
            using BlobStorageZipper zipper = new BlobStorageZipper(_storageConnectionString, _containerName);

            return await zipper.GetTotalEntriesAsync(zipFullPath, cancellationToken);

        }

        private BlockBlobClient GetBlockBlobClient(string filePath) =>
            new BlockBlobClient(_storageConnectionString, _containerName, filePath);
    }
}
