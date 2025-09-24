using ExcelDataReader;
using System.Data;
using System.Reflection.PortableExecutable;

namespace App.IO;
public interface IExcelReader
{
    DataSet ReadExcelFile();
    DataSet ReadExcelFile(string path);
}
public class ExcelReader: IExcelReader
{
    DataSet result = new DataSet();
    private readonly string filePath;
  
    public ExcelReader(string filePath)
    {
        this.filePath = filePath;
    }

    public DataSet ReadExcelFile()
    {
        return ReadExcelFile(filePath);
    }
    public DataSet ReadExcelFile(string filePath)
    {
        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                result = reader.AsDataSet();
            }
        }
        return result;
    }

    static ExcelReader()
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    }
}

