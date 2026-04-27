namespace FileRepository.Encryption
{
    public interface IEncryption
    {
        Task<MemoryStream> EncryptAsync(Stream inputStream, string name = null, IDictionary<string, string> headers = null);


        Task<MemoryStream> DecryptAsync(Stream inputStream);
    }
}