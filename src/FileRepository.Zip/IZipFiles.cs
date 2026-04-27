
namespace FileRepository.Zip
{
    public interface IZipFiles
    {
        Task AddToZipAsync(IEnumerable<string> filePaths, string zipFullPath, int compressionLevel = 0, CancellationToken cancellationToken = default);
        Task<Stream> AddToZipStreamAsync(IEnumerable<string> filePaths, int compressionLevel = 0, CancellationToken cancellationToken = default);
    }
}