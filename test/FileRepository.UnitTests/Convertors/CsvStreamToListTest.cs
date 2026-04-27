using FileRepository.Convertors;
using FileRepository.UnitTests.Convertors.Dtos;
using FluentAssertions;
using System.IO;
using System.Net.Security;
using System.Text;

namespace FileRepository.UnitTests.Convertors
{


    [TestClass]
    public class CsvStreamToListTest
    {

        [TestMethod]
        public void IsValid_OnSimpleCsv()
        {
            var fullPath = Path.Combine(AppContext.BaseDirectory, "Convertors", "csv", "test.csv");
            if (!File.Exists(fullPath)) throw new FileNotFoundException(fullPath);
            var testContent = File.ReadAllText(fullPath);
            var mem = new MemoryStream(Encoding.UTF8.GetBytes(testContent));
            try
            {
                CsvStreamToList.Validate<TestDto>(mem, true, ",", true);
            }
            catch(Exception ex) {
                throw ex;
            }
        }

        [TestMethod]
        public void IsValid_OnComplexCsv()
        {
            var fullPath = Path.Combine(AppContext.BaseDirectory, "Convertors", "csv", "AddOnly_Employee_DCDD.csv");
            if (!File.Exists(fullPath)) throw new FileNotFoundException(fullPath);
            var testContent = File.ReadAllText(fullPath);
            var mem = new MemoryStream(Encoding.UTF8.GetBytes(testContent));
            try
            {
                CsvStreamToList.Validate<AddOnly_Employee_DCDD>(mem, true, ",", true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [TestCleanup]
        public async Task TestCleanupAsync()
        {
        }

    }
}