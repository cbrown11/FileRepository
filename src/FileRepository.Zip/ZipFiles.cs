using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace FileRepository.Zip
{
    public class ZipFiles : IZipFiles
    {
        private readonly IFileRepository _fileRepository;
        public ZipFiles(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
        }

        public async Task AddToZipAsync(IEnumerable<string> filePaths, string zipFullPath, int compressionLevel = 0, CancellationToken cancellationToken = default)
        {
            var zipFileOutputStream = await AddToZipStreamAsync(filePaths, compressionLevel, cancellationToken);
            await _fileRepository.UploadFileAsync(zipFullPath, zipFileOutputStream, true, cancellationToken);
        }

        public async Task<Stream> AddToZipStreamAsync(IEnumerable<string> filePaths, int compressionLevel=0, CancellationToken cancellationToken= default)
        {
            try
            {
                var outputMemStream = new MemoryStream();
                using (var zipStream = new ZipOutputStream(outputMemStream))
                {
                    zipStream.SetLevel(compressionLevel);
                    foreach (var filePath in filePaths)
                    {
                        //var properties = await _fileRepository.GetPropertiesAsync(filePath, cancellationToken);
                        var zipEntry = new ZipEntry(filePath);
                        zipEntry.DateTime = DateTime.Now;

                        zipStream.PutNextEntry(zipEntry);
                        var fileStream = await _fileRepository.GetAsync(filePath, cancellationToken);
                        fileStream.Position = 0;
                        await fileStream.CopyToAsync(zipStream);
                        //StreamUtils.Copy(fileStream, zipStream, new byte[4096]);
                        zipStream.CloseEntry();

                        // Stop ZipStream.Dispose() from also Closing the underlying stream.
                        zipStream.IsStreamOwner = false;

                    }

                }
                outputMemStream.Position = 0;
                return outputMemStream;
            }
            catch (Exception ex)
            {

                throw;
            }
        }



    }

}
    

