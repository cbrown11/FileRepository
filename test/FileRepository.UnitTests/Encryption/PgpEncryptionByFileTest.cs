namespace FileRepository.UnitTests.Encryption
{
    using FileRepository.Encrption.PGP;
    using FileRepository.Encryption.Config;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using System.IO;
    using System.Text;

    [TestClass]
    public class PgpEncryptionByFileTest
    {
        private PgpEncryptionByFile SUT;
        private IConfiguration _config;
        private Mock<IEncryptionByFileConfig> encryptionConfig = new();


        private MemoryStream mockStream;

        private string testContent = "Test Content!!!";
        private Stream encryptStream;
        public PgpEncryptionByFileTest()
        {
            // Encryption.GenerateKey(@"C:\keys\public.asc", @"C:\keys\public.asc","email@email.com", "password");
            encryptionConfig = new Mock<IEncryptionByFileConfig>();
            mockStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));

           // PgpEncryptionUtils.GenerateKey("")
            encryptionConfig.Setup(x => x.PublicKeyFullPath).Returns(Path.Combine(AppContext.BaseDirectory, "Keys", "0xE12E7127-public.asc"));
            encryptionConfig.Setup(x => x.PrivateKeyFullPath).Returns(Path.Combine(AppContext.BaseDirectory, "Keys", "0xE12E7127-private.asc"));
            encryptionConfig.Setup(x => x.Password).Returns(@"knightfrank");
            SUT = new PgpEncryptionByFile(encryptionConfig.Object);
        }


        [TestMethod]
        public async Task RunEncryptAsync()
        {
            var encryptStream = await SUT.EncryptAsync(mockStream);
            string encryptStreamText = ReadStream(encryptStream);
            encryptStreamText.Should().NotBe(testContent);
        }

        [TestMethod]
        public async Task RunDecryptOnEncryptStreamAsync()
        {
            var encryptStream = await SUT.EncryptAsync(mockStream);
            encryptStream.Position = 0;
            var decryptStream = await SUT.DecryptAsync(encryptStream);
            string streamText = ReadStream(decryptStream);
            streamText.Should().Be(testContent);
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