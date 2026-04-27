# Introduction

This package helps easily convert any object to CSV file, along with manipulate files on to an Azure Storage.


There are two packages:

- FileRepository
- FileRepository.AzureStorage

# FileRepository

## Introduction
This package helps easily convert any object to CSV file.


## Installing from NuGet

To install via NuGet, run this command in NuGet package manager console:

PM> Install-Package FileRepository


# FileRepository.AzureStorage

## Package Dependencies

- FileRepository


## Introduction

This has AzureStorageFileRepository class that can easily Get, Create/Upload and Delete from a define Blob Storage.

You can use this directory or inherit the class and act like a base class.


An example of this is Datalake.Adapters.ADLS project, which defines explict adapters for containers on a define container port


```

    public interface IContainerFileRepository: IFileRepository
    {
        public string ContainerName { get; }
        public string Delimiter{ get; }
        Task<IEnumerable<TDto>> GetAsync<TDto>(string filename, CancellationToken cancellationToken = default) where TDto : class;
    }

    public interface ICuratedFileRepository : IContainerFileRepository
    {
    }

    public class CuratedFileRepository : AzureStorageFileRepository, ICuratedFileRepository
    {

        protected const string _containerName = "curated";
        protected const string _delimiter = ",";

        public string ContainerName => _containerName;
        public string Delimiter => _delimiter;

        public CuratedFileRepository(BlobServiceClient blobServiceClient)
            : base(blobServiceClient, _containerName)
        { }

        public async Task<IEnumerable<TDto>> GetAsync<TDto>(string filename, CancellationToken cancellationToken = default) where TDto : class
        {
            var stream = await GetAsync(filename, cancellationToken);
            // ignoresHeader can be false as should have heders
            return CsvStreamToList.Convert<TDto>(stream, ignoresHeader: false, delimiter: _delimiter, throwAndStopOnMissingField: false);
        }
    }
```

Another example of this is Snowflake.Saga project

```
    public class SagaRepository<T> : AzureStorageFileRepository, ISagaRepository<T> where T : ISnowflakeTable
    {

        protected const string _containerName = "saga";
        protected readonly string _fileName;

        public string ContainerName { get; private set; }

        public SagaRepository(BlobServiceClient blobServiceClient,
            ILogger<SagaRepository<T>> logger) : base(blobServiceClient, _containerName, logger)
        {
            _fileName = $"{typeof(T).Name}.csv";
            ContainerName = _containerName;
        }

        public async Task<IList<TDto>> GetAsync<TDto>(string filename, CancellationToken cancellationToken = default)
            where TDto : class
        {
            var stream = await base.GetAsync(filename, cancellationToken);
            return  CsvStreamToList.Convert<TDto>(stream);
        }

        protected virtual async Task<string> SaveAsync(T value, string filename, CancellationToken cancellationToken = default)
        {
            var jToken = JToken.FromObject(value);
            await base.CreateAndUploadFileAsync(filename, jToken, cancellationToken);
            return filename;
        }

        public virtual async Task<string> SaveAsync(T value, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(value.ID)) throw new ArgumentException("ID can not be null or empty");
            if (value.VERSION_NUMBER == 0) throw new ArgumentException("VERSION_NUMBER has not greater than 0");
            var filename = $"{value.ID}_V{value.VERSION_NUMBER}_{_fileName}";
            return await this.SaveAsync(value, filename, cancellationToken);
        }

        public virtual async Task<string> SaveAsync(IEnumerable<T> values, CancellationToken cancellationToken = default)
        {
            var first = values.First();
            var last = values.Last();
            var filename = $"{first.ID}_V{first.VERSION_NUMBER}_To_V{last.VERSION_NUMBER}_{_fileName}";
            return await this.SaveAsync(values, filename, cancellationToken);
        }

        protected virtual async Task<string> SaveAsync(IEnumerable<T> values, string filename, CancellationToken cancellationToken = default)
        {
            if (values == null || values.Count() == 0) return string.Empty;
            await this.SaveAsync(filename, values, cancellationToken);
            return filename;
        }

        protected async Task SaveAsync(string targetFileName, IEnumerable<T> values, CancellationToken cancellationToken = default)
        {
            if (values == null || values.Count() == 0) return;
            var jToken = JToken.FromObject(values);
            await base.CreateAndUploadFileAsync(targetFileName, jToken, cancellationToken);
        }

        public async Task<bool> DeleteStagingIfExistsAsync(string fileName, CancellationToken cancellationToken = default)
        {
             var result = await base.DeleteIfExistsAsync(fileName, cancellationToken);
            return result;
        }
    }

```


## Installing from NuGet

To install via NuGet, run this command in NuGet package manager console:

PM> Install-Package FileRepository.AzureStorage

## Config

This will require IFileAzureStorageConfig.

```
public interface IFileAzureStorageConfig
{
    string FileAzureConnectionString { get; }
}
```

## Dependency Injection

Just add the following on startup

```
    public static IServiceCollection AddAdapterBindings(this IServiceCollection services)
    {
        // Graphql Function App DI
        services.AddFileRepositoryAzureStorageBindings();
    } 
```

# FileRepository.Encryption.Pgp

## Package Dependencies

- FileRepository.Encryption


## Introduction

This will encrypt and decrypt a memory stream.

```
SUT.EncryptAsync(mockStream);

SUT.DecryptAsync(mockStream);

```

You can create a public and private key using  https://pgpkeygen.com/. Download the files and state in the config either as either as a file or as the key string.

## Config

This will require IEncryptionByStringConfig or IEncryptionByFileConfig.

```
    public interface IEncryptionByFileConfig: IEncryptionConfig
    {
        // Encrypt
        string PublicKeyFullPath { get; }
        // Decrypt
        string PrivateKeyFullPath { get; }
        string Password { get; }
    }
```


# FileRepository.Zip

## Package Dependencies

- FileRepository


## Introduction

This add files to a define zip file and upload to the FileRepository. The is all done in a Memory Stream.

```
            var sourceFullPath = "/test/test.csv";
            var sftpFullPath = "test.csv";

            var zipFileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}.{Guid.NewGuid().ToString().Substring(0, 4)}.zip";
            var filePaths = new List<string> { "test1.txt", "test2.txt" };
            await SUT.AddToZipAsync(filePaths, zipFileName, default);

```


# FileRepository.Zip.AzureStorage

## Package Dependencies

- FileRepository.AzureStorage
- FileRepository.Zip


## Introduction

This explicit For Azure and also will reduce the use of memory as placed on the Azure Storage. See  
https://josef.codes/azure-storage-zip-multiple-files-using-azure-functions/ for more details

```
            var sourceFullPaths = new List<string> { "/Test.csv", "/TestFolder/Test1.csv", "/TestFolder/Test2.csv" };
            var zipPath = "azureStorageResults/testBlobStorageFile.zip";

            await TestFileRepository.DeleteIfExistsAsync(zipPath);

            await SUT.AddToZipAsync(sourceFullPaths, zipPath);

```