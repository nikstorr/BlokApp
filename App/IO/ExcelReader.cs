using ExcelDataReader;
using System.Data;

namespace App.IO;

public class ExcelReader
{
    DataSet result = new DataSet();
    public DataSet ReadExcelFile(string filePath)
    {
        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {

                result = reader.AsDataSet();
                // The result of each spreadsheet is in result.Tables
            }
        }
        return result;
    }

    static ExcelReader()
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    }
}

