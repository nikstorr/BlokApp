// See https://aka.ms/new-console-template for more information

using App;
using App.Domain;
using App.IO;
using System.Data;

// ------
string inputPath = "../../../../Files/Nikolai-Arbejdsopgave_2.xlsx";
string outputPath = "../../../../Files/Activities.xlsx";

var excelReader = new ExcelReader(inputPath);
var excelWriter = new ExcelWriter(outputPath);

DataSet tableSet = excelReader.ReadExcelFile(inputPath);

var runner = new Runner(excelReader, excelWriter);
// ------

const int klaWidth = 6;
const int aktNavnWidth = 12;
const int posWidth = 4;
const int perWidth = 6;

string border = $"+{new string('-', klaWidth)}+{new string('-', aktNavnWidth)}+{new string('-', posWidth)}+{new string('-', perWidth)}+";

Console.WriteLine("Activities:");
Console.WriteLine(border);
Console.WriteLine(
    $"|{"KLA",-klaWidth}|{"AKT_NAVN",-aktNavnWidth}|{"POS",-posWidth}|{"PER",-perWidth}|");
Console.WriteLine(border);

// Run main logic
runner.Run(); 

foreach (DataRow row in runner.Result.Rows)
{
    Console.WriteLine(
        $"|{row["KLA"],-klaWidth}|{row["AKT_NAVN"],-aktNavnWidth}|{row["POS"],-posWidth}|{row["PER"],-perWidth}|");
    Console.WriteLine(border);
}