namespace FileRepository.Encryption.Config
{
    public interface IEncryptionByStringConfig : IEncryptionConfig
    {
        string PublicKey { get; }
        string PrivateKey { get; }
    }
}