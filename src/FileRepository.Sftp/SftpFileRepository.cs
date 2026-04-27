using FileRepository.Encryption;
using FileRepository.Sftp.Exceptions;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace FileRepository.Sftp
{
    public class SftpFileRepository : ISftpFileRepository, IAsyncDisposable, IDisposable 
    {
        private readonly ISftpClient _sftpClient;
        private readonly IEncryption _encryption;
        public IFileRepository FileRepository { get; private set; }
        public event EventHandler<ExceptionEventArgs> ErrorOccurred;
        public event EventHandler<HostKeyEventArgs> HostKeyReceived;
        public HostKeyEventArgs HostKeyArgs { get; private set; }
        public Type FileRepositoryType { get; private set; }

        public SftpFileRepository(ISftpClient sftpClient, IFileRepository fileRepository,
            IEncryption encryption) : this(sftpClient, fileRepository)
        {
            _encryption = encryption ?? throw new ArgumentNullException(nameof(encryption));
        }
        public SftpFileRepository(ISftpClient sftpClient, IFileRepository fileRepository)
        {
            _sftpClient = sftpClient ?? throw new ArgumentNullException(nameof(sftpClient));
            FileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
            FileRepositoryType = fileRepository.GetType();
            _sftpClient.HostKeyReceived += (sender, e) => HostKeyArgs = e;

            _sftpClient.HostKeyReceived += (sender, e) => HostKeyReceived?.Invoke(this,e);
            _sftpClient.ErrorOccurred += (sender, e) => ErrorOccurred?.Invoke(this, e);
        }

        public async Task UploadFileAsync(string sourceFullPath, string sftpFullPath, bool encrypt = false, CancellationToken cancellationToken = default)
        {
            try
            {
                using var inputStream = await FileRepository.GetAsync(sourceFullPath, cancellationToken);
                await this.UploadAsync(inputStream, sftpFullPath, encrypt, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new SftpException($"Error uploadling file {sourceFullPath}.", ex);
            }
        }

        public async Task CreateDirectoriesAsync(string sftpFullPath,  CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_sftpClient.IsConnected)
                {
                    await _sftpClient.ConnectAsync(cancellationToken);
                }
                string[] directories = sftpFullPath.Split('/');
                for (int i = 0; i < directories.Length; i++)
                {
                    string dirName = string.Join("/", directories, 0, i + 1);
                    if (!_sftpClient.Exists(dirName))
                        _sftpClient.CreateDirectory(dirName);
                }
                
            }
            catch (Exception e)
            {
                throw new SftpException($"Error Create Directories. host={_sftpClient.ConnectionInfo.Host}, sftpFullPath={sftpFullPath}", e);
            }
        }

        public void CreateDirectories(string sftpFullPath)
        {
            try
            {
                if (!_sftpClient.IsConnected)
                {
                    _sftpClient.Connect();
                }
                string[] directories = sftpFullPath.Split('/');
                for (int i = 0; i < directories.Length; i++)
                {
                    string dirName = string.Join("/", directories, 0, i + 1);
                    if (!_sftpClient.Exists(dirName))
                        _sftpClient.CreateDirectory(dirName);
                }
            }
            catch (Exception e)
            {
                throw new SftpException($"Error Create Directories. host={_sftpClient.ConnectionInfo.Host}, sftpFullPath={sftpFullPath}", e);
            }
        }


        public async Task Upload(Stream inputStream, string sftpFullPath, bool encrypt = false, CancellationToken cancellationToken = default)
        {
            try
            {
                var stream = new MemoryStream();
                if (encrypt)
                {
                    MemoryStream streamEncrypt = await EncryptStream(inputStream);
                    streamEncrypt.CopyTo(stream);
                }
                else
                {
                    inputStream.CopyTo(stream);
                }
                if (!_sftpClient.IsConnected)
                {
                     await _sftpClient.ConnectAsync(cancellationToken);
                }
                stream.Position = 0;
                _sftpClient.UploadFile(stream, sftpFullPath);
            }
            catch (Exception e)
            {
                throw new SftpException($"Error uploadling stream. host={_sftpClient.ConnectionInfo.Host}, sftpFullPath={sftpFullPath}, encrypt={encrypt}", e);
            }
        }

        public async Task UploadAsync(Stream inputStream, string sftpFullPath, bool encrypt = false, CancellationToken cancellationToken = default)
        {
            try
            {
                var stream = new MemoryStream();
                if (encrypt)
                {
                    MemoryStream streamEncrypt = await EncryptStream(inputStream);
                    streamEncrypt.CopyTo(stream);
                }
                else
                {
                    inputStream.CopyTo(stream);
                }
                if (!_sftpClient.IsConnected)
                {
                    await _sftpClient.ConnectAsync(cancellationToken);
                }
                stream.Position = 0;
                _sftpClient.UploadFile(stream, sftpFullPath);
            }
            catch (Exception e)
            {
                throw new SftpException($"Error uploadling stream. host={_sftpClient.ConnectionInfo.Host}, sftpFullPath={sftpFullPath}, encrypt={encrypt}", e);
            }
        }

        protected async Task<MemoryStream> EncryptStream(Stream inputStream)
        {
            if (_encryption == null) throw new ArgumentException("An IEncryption has not been provided");
            var streamEncrypt = await _encryption.EncryptAsync(inputStream);
            streamEncrypt.Position = 0;
            return streamEncrypt;
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        public virtual async ValueTask DisposeAsyncCore()
        {
            if (_sftpClient != null)
            {
                if (_sftpClient.IsConnected)
                {
                    _sftpClient.Disconnect();
                }
                _sftpClient.Dispose();
            }
        }

        public virtual void Dispose()
        {
            DisposeAsyncCore().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }
    }
}
