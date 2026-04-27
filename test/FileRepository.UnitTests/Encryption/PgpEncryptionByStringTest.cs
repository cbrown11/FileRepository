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
    public class PgpEncryptionByStringTest
    {
        private PgpEncryptionByString SUT;
        private IConfiguration _config;
        private Mock<IEncryptionByStringConfig> encryptionConfig = new();


        private MemoryStream mockStream;

        private string testContent = "Test Content!!!";
        private Stream encryptStream;
        public PgpEncryptionByStringTest()
        {
            encryptionConfig = new Mock<IEncryptionByStringConfig>();
            mockStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));

  
            var pub = Base64Encode(File.ReadAllText(@"Keys\0xE12E7127-public.asc"));
            var priv = Base64Encode(File.ReadAllText(@"Keys\0xE12E7127-private.asc"));
            encryptionConfig.Setup(x => x.PublicKey).Returns(pub);
            encryptionConfig.Setup(x => x.PrivateKey).Returns(priv);
            encryptionConfig.Setup(x => x.Password).Returns(@"knightfrank");
            SUT = new PgpEncryptionByString(encryptionConfig.Object);
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

        public static string Base64Encode(string plainText) 
        {
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}