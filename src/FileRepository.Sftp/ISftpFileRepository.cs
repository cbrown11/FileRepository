
using Renci.SshNet.Common;

namespace FileRepository.Sftp
{
    public interface ISftpFileRepository
    {
        public IFileRepository FileRepository { get; }
        public Type FileRepositoryType { get;}
        public HostKeyEventArgs HostKeyArgs { get;  }
        public event EventHandler<ExceptionEventArgs> ErrorOccurred;
        public event EventHandler<HostKeyEventArgs> HostKeyReceived;
        Task UploadAsync(Stream inputStream, string sftpFullPath, bool encrypt = false, CancellationToken cancellationToken = default);
        Task UploadFileAsync(string sourceFullPath, string sftpFullPath, bool encrypt = false, CancellationToken cancellationToken = default);
        Task CreateDirectoriesAsync(string sftpFullPath, CancellationToken cancellationToken = default);
        void CreateDirectories(string sftpFullPath);
    }
}