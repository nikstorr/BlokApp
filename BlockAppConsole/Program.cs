// See https://aka.ms/new-console-template for more information


using App;
using System.Data;

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

var handler = new BlockHandler();
handler.HandleBlocks();

foreach (DataRow row in handler.Result.Rows)
{
    Console.WriteLine(
        $"|{row["KLA"],-klaWidth}|{row["AKT_NAVN"],-aktNavnWidth}|{row["POS"],-posWidth}|{row["PER"],-perWidth}|");
    Console.WriteLine(border);
}