using Spire.Xls;
using System.Data;

namespace App.IO
{
    public interface IExcelWriter
    {
        void WriteExcelFile(DataTable table);
        void WriteExcelFile(string filePath, DataTable dataTable);
    }
    public class ExcelWriter : IExcelWriter
    {
        private readonly string path;

        public ExcelWriter(string path)
        {
            this.path = path;
        }
        public void WriteExcelFile(DataTable table)
        {
            WriteExcelFile(path, table);
        }
        public void WriteExcelFile(string filePath, DataTable dataTable)
        {
            // Implementation for writing to an Excel file would go here.
            // Create a Workbook object
            Workbook workbook = new Workbook();

            // Add a new worksheet
            Worksheet sheet = workbook.Worksheets.Add("Activities");

            // Import DataTable data into the worksheet
            sheet.InsertDataTable(dataTable, true, 1, 1);

            // Autofit column width
            for (int i = 1; i <= sheet.AllocatedRange.ColumnCount; i++)
            {
                sheet.AutoFitColumn(i);
            }

            // Save the file
            workbook.SaveToFile(filePath, FileFormat.Version2016);
            workbook.Dispose();
        }
    }
}
