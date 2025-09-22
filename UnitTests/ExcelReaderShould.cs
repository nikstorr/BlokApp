using App.IO;
using Xunit;

namespace UnitTests
{
    public class ExcelReaderShould
    {
        private ExcelReader _reader = new ExcelReader();

        [Fact]
        public void ReadExcelFile()
        {
            var path = "../../../../Files/Nikolai-Arbejdsopgave_2.xlsx";
            var data = _reader.ReadExcelFile(path);

            Assert.NotNull(data);
            Assert.Equal(10, data.Tables.Count);
        }
    }
}
