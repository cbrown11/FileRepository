namespace FileRepository.Encrption.PGP
{
    using FileRepository.Encryption;
    using FileRepository.Encryption.Config;
    using FileRepository.Encryption.Exceptions;
    using PgpCore;
    using System.Threading.Tasks;

    public class PgpEncryptionByString : IEncryption
    {
        private readonly IEncryptionByStringConfig _encryptionConfig;
        private readonly string _publicKey;
        private readonly string _privateKey;
        private readonly string _password;

        public PgpEncryptionByString(IEncryptionByStringConfig encryptionConfig)
        {
            _encryptionConfig = encryptionConfig ?? throw new ArgumentNullException(nameof(encryptionConfig));
            _publicKey = encryptionConfig?.PublicKey;
            _privateKey = encryptionConfig?.PrivateKey;
            _password = encryptionConfig?.Password;
        }

        public PgpEncryptionByString(string publicKey, string privateKey, string password)
        {
            _publicKey = publicKey;
            _privateKey = privateKey;
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
 
               if(string.IsNullOrEmpty(_publicKey)) throw new ArgumentNullException(nameof(_publicKey));
                var outputStream = new MemoryStream();
                var ms = this.GetMemoryStream(_publicKey);
                var encryptionKeys = new EncryptionKeys(ms);
                var pgp = new PGP(encryptionKeys);
                await pgp.EncryptAsync(inputStream, outputStream,name:name, headers:headers);
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
                var outputStream = new MemoryStream();
                var ms = this.GetMemoryStream(_privateKey);
                var decryptionKeys = new EncryptionKeys(ms, _password);
                var pgp = new PGP(decryptionKeys);
                await pgp.DecryptAsync(inputStream, outputStream);
                return outputStream;
            }
            catch (Exception ex)
            {
                throw new EncryptException($"Error in decrypting stream", ex);
            }
        }

        protected MemoryStream GetMemoryStream(string key)
        {
            var secretBytes = Convert.FromBase64String(key);
            MemoryStream ms = new MemoryStream();
            ms.Write(secretBytes, 0, secretBytes.Length);
            ms.Position = 0;
            return ms;
        }
    }
}
