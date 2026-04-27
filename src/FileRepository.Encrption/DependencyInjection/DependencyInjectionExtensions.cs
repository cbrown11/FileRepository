using FileRepository.Encrption.PGP;
using FileRepository.Encryption;
using Microsoft.Extensions.DependencyInjection;

namespace FileRepository.Sftp.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddPgpEncryptionByStringBindings(this IServiceCollection services)
        {
            services.AddScoped<IEncryption, PgpEncryptionByString>();
            return services;
        }

        public static IServiceCollection AddPgpEncryptionByFileBindings(this IServiceCollection services)
        {
            services.AddScoped<IEncryption, PgpEncryptionByFile>();
            return services;
        }
    }
}