using App.IO;
using System.Data;
using Xunit;

namespace IntegrationTests
{
    public class ExcelReaderShould
    {
        private ExcelReader _reader = new ExcelReader("../../../../Files/Nikolai-Arbejdsopgave_2.xlsx");

        [Fact]
        public void ReadExcelFile()
        {
            
            DataSet data = _reader.ReadExcelFile();

            Assert.NotNull(data);
            Assert.Equal(10, data.Tables.Count);
        }
    }
}
