namespace FileRepository.Sftp.Config;

public interface IFileSftpConfig
{
    string SftpHost { get; }
    int SftpPort { get; }
    string SftpUsername { get; }
    string SftpPassPhrase { get; }
    string SftpKeyFile { get; }
    string SftpKey { get; }
}