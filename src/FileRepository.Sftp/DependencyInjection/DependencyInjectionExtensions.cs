using FileRepository.Sftp.Config;
using Microsoft.Extensions.DependencyInjection;
using Renci.SshNet;

namespace FileRepository.Sftp.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddFileRepositorySftpBindings(this IServiceCollection services)
        {
            services.AddScoped<ISftpClient>(sp =>
            {
                var config = sp.GetService<IFileSftpConfig>() ?? throw new MissingMemberException(nameof(IFileSftpConfig));
                var privateKeys = new List<PrivateKeyFile>();
                if (string.IsNullOrEmpty(config?.SftpHost)) throw new MissingMemberException(nameof(config.SftpHost));
                if (config?.SftpPort == null) throw new MissingMemberException(nameof(config.SftpPort));
                if (string.IsNullOrEmpty(config?.SftpUsername)) throw new MissingMemberException(nameof(config.SftpUsername));
                if (string.IsNullOrEmpty(config?.SftpKeyFile) && string.IsNullOrEmpty(config?.SftpKey)) throw new MissingMemberException($"{nameof(config.SftpKeyFile)} or {nameof(config.SftpKey)}");

                if (!string.IsNullOrEmpty(config?.SftpKeyFile))
                {
                    privateKeys.Add(new PrivateKeyFile(config.SftpKeyFile,config.SftpPassPhrase));
                }
                if (!string.IsNullOrEmpty(config?.SftpKey))
                {
                    privateKeys.Add(new PrivateKeyFile(GetMemoryStream(config.SftpKey),config.SftpPassPhrase));
                }        
                return new SftpClient(config.SftpHost, config.SftpPort, config.SftpUsername, privateKeys.ToArray());
            });
            services.AddScoped<ISftpFileRepository, SftpFileRepository>();
            return services;
        }

        public static MemoryStream GetMemoryStream(string key)
        {
            var secretBytes = Convert.FromBase64String(key);
            MemoryStream ms = new MemoryStream();
            ms.Write(secretBytes, 0, secretBytes.Length);
            ms.Position = 0;
            return ms;
        }
    }
}