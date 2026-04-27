namespace FileRepository.Encryption.Config
{
    public interface IEncryptionByFileConfig : IEncryptionConfig
    {
        // Encrypt
        string PublicKeyFullPath { get; }
        // Decrypt
        string PrivateKeyFullPath { get; }
    }
}