namespace FileRepository.AzureStorage.DependencyInjection
{
    using Microsoft.Extensions.DependencyInjection;
    using Azure.Storage.Blobs;
    using FileRepository.AzureStorage.Config;
    using System.Runtime.Serialization;
    using Azure.Identity;

    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddFileRepositoryAzureStorageBindings(this IServiceCollection services, string? container ="")
        {
            services.AddScoped(sp =>
            {
                var config = sp.GetService<IFileAzureStorageConfig>() ?? throw new MissingMemberException(nameof(IFileAzureStorageConfig));
                if (config.FileAzureConnectionString.Contains("AccountKey=") 
                || config.FileAzureConnectionString.Contains("UseDevelopmentStorage=true"))
                {
                    return new BlobServiceClient(config.FileAzureConnectionString);
                }
                return new BlobServiceClient(new Uri(config.FileAzureConnectionString), new DefaultAzureCredential());
     
            });
            CreateAzureStorageFileRepository(services, container);
            return services;
        }

        private static void CreateAzureStorageFileRepository(IServiceCollection services, string? container)
        {
            if (!string.IsNullOrEmpty(container))
            {
                services.AddScoped<IFileRepository>(sp =>
                {
                    var blobServiceClient = sp.GetService<BlobServiceClient>() ?? throw new MissingMemberException(nameof(BlobServiceClient));
                    return new AzureStorageFileRepository(blobServiceClient, container);
                });
            }
        }
    }
}