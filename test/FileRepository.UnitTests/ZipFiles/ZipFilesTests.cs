namespace FileRepository.UnitTests.ZipFiles
{
    using FileRepository.Encryption.Config;
    using FileRepository.Zip;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using System.IO;
    using System.Text;

    [TestClass]
    public class ZipFilesTests
    {
        private ZipFiles SUT;
        private IConfiguration _config;
        private Mock<IFileRepository> mockFileRepository = new();


        private MemoryStream mockStream;

        private string testContent = "Test Content!!!";
        private Stream uploadStream;

        public ZipFilesTests()
        {
            mockFileRepository = new Mock<IFileRepository>();



            mockStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));
            mockFileRepository
                 .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(mockStream);

            mockFileRepository
                 .Setup(x => x.GetPropertiesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new FileProperties { Size = testContent.Length});

            mockFileRepository
                .Setup(x => x.UploadFileAsync(It.IsAny<string>(), It.IsAny<Stream>(), true, default))
                .Callback(new InvocationAction(i =>
                {
                    uploadStream = (Stream)i.Arguments[1];
                }));

            SUT = new ZipFiles(mockFileRepository.Object);
        }


        [TestMethod]
        public async Task ZipWithNoEncryptionAsync()
        {
            var sourceFullPath = "/test/test.csv";
            var sftpFullPath = "test.csv";

            var zipFileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}.{Guid.NewGuid().ToString().Substring(0, 4)}.zip";
            var filePaths = new List<string> { "test1.txt", "test2.txt" };
            await SUT.AddToZipAsync(filePaths, zipFileName, default);


            string uploadStreamText = ReadStream(uploadStream);
            uploadStreamText.Should().NotBeNull();
        }



        private string ReadStream(Stream stream)
        {
            byte[] readBuffer = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(readBuffer, 0, readBuffer.Length);
            return Encoding.UTF8.GetString(readBuffer);
        }
    }
}