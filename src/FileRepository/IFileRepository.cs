using Common.Models.Paging;
using Newtonsoft.Json.Linq;

namespace FileRepository
{
    public interface IFileRepository
    {
        Task CreateAndUploadFileAsync(string filePath, JToken jObject, bool overwrite = false, CancellationToken cancellationToken = default);
        Task UploadFileAsync(string filePath, Stream content, bool overwrite = false, CancellationToken cancellationToken = default);
        Task<bool> DeleteIfExistsAsync(string filePath, CancellationToken cancellationToken = default);
        Task<bool> FileExitsAsync(string filePath, CancellationToken cancellationToken = default);
        Task<Stream> GetAsync(string filePath, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> GetFilesAsync(string? prefixFolders = null, CancellationToken cancellationToken = default);
        Task<FileProperties> GetPropertiesAsync(string filePath, CancellationToken cancellationToken = default);
        Task<Connection<string>> GetFilesAsPagesAsync(string? prefixFolders = null, string? continuationToken = default,int? pageSizeHint = default, CancellationToken cancellationToken = default);
    }
}