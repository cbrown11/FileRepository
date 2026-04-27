using FileRepository.AzureStorage.Config;
using FileRepository.Zip;
using Microsoft.Extensions.DependencyInjection;

namespace FileRepository.Zip.AzureStorage.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddStandardZipBindings(this IServiceCollection services, string containerName)
        {

            services.AddScoped<IZipFiles, BlobStorageZipFilesService>(sp =>
            {
                var fileRepository = sp.GetService<IFileRepository>() ?? throw new MissingMemberException(nameof(IFileRepository));
                var config = sp.GetService<IFileAzureStorageConfig>() ?? throw new MissingMemberException(nameof(IFileAzureStorageConfig));
                var connectionString = config.FileAzureConnectionString;

                ArgumentNullException.ThrowIfNull(fileRepository, nameof(fileRepository));
                ArgumentNullException.ThrowIfNull(connectionString, nameof(connectionString));
                return new BlobStorageZipFilesService(fileRepository, connectionString, containerName);
            });
            return services;
        }
    }
}
