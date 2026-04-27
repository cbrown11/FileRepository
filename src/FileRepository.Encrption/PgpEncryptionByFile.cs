namespace FileRepository.Encrption.PGP
{
    using FileRepository.Encryption;
    using FileRepository.Encryption.Config;
    using FileRepository.Encryption.Exceptions;
    using PgpCore;
    using System.Threading.Tasks;

    public class PgpEncryptionByFile : IEncryption
    {
        private readonly IEncryptionByFileConfig _encryptionConfig;
        private readonly string _publicKeyFullPath;
        private readonly string _privateKeyFullPath;
        private readonly string _password;

        public PgpEncryptionByFile(IEncryptionByFileConfig encryptionConfig)
        {
            _encryptionConfig = encryptionConfig ?? throw new ArgumentNullException(nameof(encryptionConfig));
            _publicKeyFullPath = encryptionConfig?.PublicKeyFullPath;
            _privateKeyFullPath = encryptionConfig?.PrivateKeyFullPath;
            _password = encryptionConfig?.Password;
        }

        public PgpEncryptionByFile(string publicKeyFullPath, string privateKeyFullPath,string password)
        {
            _publicKeyFullPath = publicKeyFullPath;
            _privateKeyFullPath = privateKeyFullPath;
            _password = password;
        }


        /// <summary>
        /// PGP Encrypt the stream.
        /// </summary>
        /// <param name="inputStream">Plain data stream to be encrypted</param>
        /// <param name="name">Name of encrypted file in message, defaults to the input file name</param>
        /// <param name="headers">Optional headers to be added to the output</param>
        public async Task<MemoryStream> EncryptAsync(Stream inputStream, string name = null, IDictionary<string, string> headers = null)
        {
            try 
            {
                if (string.IsNullOrEmpty(_publicKeyFullPath)) throw new ArgumentNullException(nameof(_publicKeyFullPath));
                var outputStream = new MemoryStream();
                using var publicKeyStream = new FileStream(_publicKeyFullPath, FileMode.Open);
                var encryptionKeys = new EncryptionKeys(publicKeyStream);
                var pgp = new PGP(encryptionKeys);
                await pgp.EncryptAsync(inputStream, outputStream, name: name, headers: headers);
                return outputStream;
            }
            catch (Exception ex)
            {
                throw new EncryptException($"Error in encrypting stream", ex);
            }
        }

        /// <summary>
        /// PGP Decrypt the stream.
        /// </summary>
        /// <param name="inputStream">Plain data stream to be encrypted</param>
        public async Task<MemoryStream> DecryptAsync(Stream inputStream)
        {
            try
            {
                if (string.IsNullOrEmpty(_privateKeyFullPath)) throw new ArgumentNullException(nameof(_privateKeyFullPath));
                var outputStream = new MemoryStream();
                using var privateKeyStream = new FileStream(_privateKeyFullPath, FileMode.Open);
                var decryptionKeys = new EncryptionKeys(privateKeyStream, _password);
                var pgp = new PGP(decryptionKeys);
                await pgp.DecryptAsync(inputStream, outputStream);
                return outputStream;
            }
            catch (Exception ex)
            {
                throw new EncryptException($"Error in decrypting stream", ex);
            }
        }
    }
}
