using App.Domain;
using App.IO;
using System.Data;

namespace App;
public class Runner
{
    private readonly IExcelReader _excelReader;
    private readonly IExcelWriter _excelWriter;
    private DataSet _tableSet = new();

    private DataTable _result = new();
    public DataTable Result => _result;

    private DataTable _holdTable = new();
    private DataTable _blokkeTable = new();

    private readonly List<Activity> _activities = [];

    private BlockHandler _blockHandler;
    private readonly ActivityCreator _activityCreator;

    public Runner(IExcelReader excelReader, IExcelWriter excelWriter)
    {
        _excelReader = excelReader;
        _excelWriter = excelWriter;

        PopulateTables();

        var _holdHandler = new HoldHandler(_holdTable, _blokkeTable);
        _activityCreator = new ActivityCreator(_holdHandler);
        _blockHandler = new BlockHandler();
    }

    /// <summary>
    /// Entry point.
    /// </summary>
    public void Run()
    {
        var blokke = _blockHandler.GetBlocksToProcess(_blokkeTable);
        var firstPOSIndex = Util.GetFirstPOSIdx(_blokkeTable);

        foreach (var block in blokke)
        {
            var activities = _activityCreator.CreateActivitiesFromBlock(block, firstPOSIndex);
            foreach (var act in activities)
                _activities.Add(act);
        }
        // convert from domain objects to DataTable for easy saving to Excel
        _result = Util.ConvertToDataTable(_activities);
    }

    public void SaveFile()
    {
        _excelWriter.WriteExcelFile(_result);
    }

    private void PopulateTables()
    {
        // Read the Excel file to get the DataSet
        _tableSet = _excelReader.ReadExcelFile();
        // Fetch the HOLD table (5th table, index 4) from the DataSet.
        // NOTE Don't fetch it in the ctor. Somehow that breaks the ExcelReader.
        _holdTable = _tableSet.Tables[Constants.HOLD_idx];
        // Fetch the BLOKKE table (6th table, index 5) from the _tableSet
        // NOTE Don't fetch it in the ctor. That messes with the Excel data reader, somehow.
        _blokkeTable = _tableSet.Tables[Constants.BLOKKE_idx];
    }
}

