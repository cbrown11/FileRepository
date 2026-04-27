
using FileRepository.Zip;
using Microsoft.Extensions.DependencyInjection;

namespace FileRepository.Sftp.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddZipBindings(this IServiceCollection services)
        {

            services.AddScoped<IZipFiles, ZipFiles>();
            return services;
        }


    }
}