using FileRepository.Sftp;

namespace FileRepository.UnitTests.Sftp
{
    using FileRepository.Encrption.PGP;
    using FileRepository.Encryption.Config;
    using FileRepository.Sftp.Config;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Renci.SshNet;
    using System.IO;
    using System.Text;
    using static System.Net.Mime.MediaTypeNames;

    [TestClass]
    public class SendToSftpTest
    {
        private SftpFileRepository SUT;
        private IConfiguration _config;
        private Mock<IFileRepository> mockFileRepository = new();
        private Mock<ISftpClient> mockSftpClient = new();
        private Mock<IEncryptionByFileConfig> encryptionConfig = new();


        private MemoryStream mockStream;

        private string testContent = "Test Content!!!";
        private Stream uploadStream;
        public SendToSftpTest()
        {
            mockFileRepository = new Mock<IFileRepository>();
            mockSftpClient = new Mock<ISftpClient>();
            encryptionConfig = new Mock<IEncryptionByFileConfig>();

            mockStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));
            mockFileRepository
                 .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(mockStream);

            mockSftpClient
                .Setup(x => x.UploadFile(It.IsAny<Stream>(), It.IsAny<string>(), null))
                .Callback(new InvocationAction(i =>
                {
                    uploadStream = (Stream)i.Arguments[0];
                }));
            encryptionConfig.Setup(x => x.PublicKeyFullPath).Returns(Path.Combine(AppContext.BaseDirectory, "Keys", "0xE12E7127-public.asc"));
            SUT = new SftpFileRepository(mockSftpClient.Object, mockFileRepository.Object, new PgpEncryptionByFile(encryptionConfig.Object));
        }


        [TestMethod]
        public async Task UploadWithNoEncryptionAsync()
        {
            var sourceFullPath = "/test/test.csv";
            var sftpFullPath = "test.csv";
            await SUT.UploadFileAsync(sourceFullPath, sftpFullPath, false, default);
            mockSftpClient.Verify(x => x.ConnectAsync(default), Times.Once);

            string uploadStreamText = ReadStream(uploadStream);
            uploadStreamText.Should().Be(testContent);
        }


        [TestMethod]
        public async Task UploadWithEncryptionAsync()
        {
            var sourceFullPath = "/test/test.csv";
            var sftpFullPath = "test.csv";
            await SUT.UploadFileAsync(sourceFullPath, sftpFullPath, true, default);
            mockSftpClient.Verify(x => x.ConnectAsync(default), Times.Once);

            string uploadStreamText = ReadStream(uploadStream);
            uploadStreamText.Should().NotBe(testContent);
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