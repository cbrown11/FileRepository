using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using ICSharpCode.SharpZipLib.Zip;

namespace FileRepository.Zip.AzureStorage
{
    internal class BlobStorageZipper : IDisposable
    {
        private bool _disposed = false;
        private readonly string _storageConnectionString;
        private readonly string _containerName;
        private Stream? _blobZipFileStream;
        private ZipOutputStream? _zipFileOutputStream;

        internal BlobStorageZipper(string storageConnectioString, string containerName)
        {
            _storageConnectionString = storageConnectioString;
            _containerName = containerName;
        }

        internal async Task CreateZipAsync(string archiveName, int compressionLevel = 0, CancellationToken cancellationToken = default)
        {
            _blobZipFileStream = await OpenBlobZipFileStream(archiveName, cancellationToken);

            _zipFileOutputStream = CreateZipOutputStream(_blobZipFileStream);

            _zipFileOutputStream.SetLevel(compressionLevel);
        }

        internal async Task AddToZipAsync(BlockBlobClient blob, CancellationToken cancellationToken = default)
        {
            var properties = await blob.GetPropertiesAsync(cancellationToken: cancellationToken);

            var zipEntry = new ZipEntry(blob.Name)
            {
                Size = properties.Value.ContentLength
            };

            _zipFileOutputStream?.PutNextEntry(zipEntry);

            await blob.DownloadToAsync(_zipFileOutputStream, cancellationToken);

            _zipFileOutputStream?.CloseEntry();
        }

        internal async Task<long> GetTotalEntriesAsync(string archiveName, CancellationToken cancellationToken)
        {
            using Stream stream = await GetBlockBlobClient(archiveName).OpenReadAsync(cancellationToken: cancellationToken);

            using ZipFile zip = new ZipFile(stream);

            return zip.Count;
        }

        private async Task<Stream> OpenBlobZipFileStream(string zipFilename, CancellationToken cancellationToken)
        {
            var zipBlobClient = new BlockBlobClient(_storageConnectionString, _containerName, zipFilename);

            var blockBlobOpenWriteOptions = new BlockBlobOpenWriteOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = "application/zip"
                }
            };

            return await zipBlobClient.OpenWriteAsync(true, blockBlobOpenWriteOptions, cancellationToken);
        }

        private static ZipOutputStream CreateZipOutputStream(Stream zipFileStream) =>
            new ZipOutputStream(zipFileStream)
            {
                IsStreamOwner = false
            };

        private BlockBlobClient GetBlockBlobClient(string filePath) =>
            new BlockBlobClient(_storageConnectionString, _containerName, filePath);


        internal void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _zipFileOutputStream?.Dispose();
                    _blobZipFileStream?.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~BlobStorageZipper() => Dispose(false);
    }
}
