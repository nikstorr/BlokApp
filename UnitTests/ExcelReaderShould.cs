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

            var path = "../../../../Files/Nikolai-Arbejdsopgave.xlsx";
            var data = _reader.ReadExcelFile(path);


        }
    }
}
