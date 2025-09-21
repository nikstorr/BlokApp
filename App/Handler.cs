using App.Domain;
using App.IO;
using System.Data;

namespace App
{
    public class Handler
    {
        readonly string _path = "../../../../Files/Nikolai-Arbejdsopgave_2.xlsx";
        // Helper for block processing
        readonly BlokHelper _blockHelper = new();
        // all tables in the excel input file
        readonly DataSet _tableSet = new();
        // The resulting activities as a DataTable ready to write as a new excel spreadsheet
        DataTable _result = new();
        public DataTable Result => _result;
        // HOLD and BLOKKE tables
        DataTable _holdTable = new();
        DataTable _blokkeTable = new();

        // A dictionary of HOLD as:  (KLA+FAG, POS)
        readonly List< (string, string, int)> _HOLD_POS = [];

        readonly List<Activity> _activities = new();

        readonly ActivityCreator _activityCreator = null;


        public Handler()
        {
            _tableSet = new ExcelReader().ReadExcelFile(_path);

            _activityCreator =  new ActivityCreator(_holdTable);
        }
        public void HandleActivities()
        {
            PopulateTables();

            foreach (var block in GetBlocksToProcess(_blokkeTable))
            {
                if (_blockHelper.BlockHasActivity(block))
                {
                    var activities = _activityCreator.CreateActivitiesFromBlock(block);
                    foreach(var act in activities)
                        _activities.Add(act);
                }
            }
            ConvertToDataTable();
        }

        private void PopulateTables()
        {
            // Fetch the HOLD table (5th table, index 4) from the DataSet, here.
            // Don't fetch it in the ctor. Somehow that breaks the ExcelReader.
            _holdTable = _tableSet.Tables[Constants.HOLD_idx];
            // Fetch the BLOKKE table (6th table, index 5) from the _tableSet
            // Don't fetch it in the ctor. That messes with the Excel data reader somehow.
            _blokkeTable = _tableSet.Tables[Constants.BLOKKE_idx];
        }

        private List<Block> GetBlocksToProcess(DataTable table)
        {
            var filteredRows = RemoveEmptyRows(table);
            return CreateActivityBlocks(filteredRows);
        }


        private void FilterHold()
        {

            // fetch only rows where KLA has a value. 
            var hold = _holdTable.Rows.Cast<DataRow>()
                .Where(row => !string.IsNullOrWhiteSpace(row.Field<string>(0)))
                .Skip(1); // skip the Column-name row, row 1.

            foreach (DataRow row in hold)
            {
                var klasse = row.Field<string>(0);
                var aktivitet = row.Field<string>(1);
                var positioner = (int)row.Field<double>(3);

                _HOLD_POS.Add((klasse, aktivitet, positioner));
            }
        }

        private static IEnumerable<DataRow> RemoveEmptyRows(DataTable blokkeTable)
        {
            return blokkeTable.Rows.Cast<DataRow>().Skip(2)
                .Where(row =>
                  
                    // Exclude rows where all POS columns are empty or null
                    Enumerable.Range(Constants.BLOKKE_POS1_idx, Constants.BLOKKE_POS5_idx - Constants.BLOKKE_POS1_idx + 1)
                        .Any(posIdx => !row.IsNull(posIdx) && !string.IsNullOrWhiteSpace(row[posIdx]?.ToString()))
                );
        }

        private List<Block> CreateActivityBlocks(IEnumerable<DataRow> rows)
        {
            var groups = new List<Block>();
            Block currentGroup = null;
            (string kla, string blok) lastKey = (null, null);

            foreach (var row in rows)
            {
                var kla = row[Constants.BLOKKE_KLA_idx]?.ToString();
                var blok = row[Constants.BLOKKE_BLOK_idx]?.ToString();
                var key = (kla, blok);

                // If KLA is empty, add row to current group (if any), or create a new group if none exists
                if (string.IsNullOrWhiteSpace(kla))
                {
                    if (currentGroup == null)
                    {
                        currentGroup = new Block { Key = key, Rows = new List<DataRow>() };
                        lastKey = key;
                    }
                    currentGroup.Rows.Add(row);
                    continue;
                }

                if (lastKey.kla == null && lastKey.blok == null)
                {
                    currentGroup = new Block { Key = key, Rows = new List<DataRow>() };
                    currentGroup.Rows.Add(row);
                    lastKey = key;
                }
                else if (key == lastKey)
                {
                    currentGroup.Rows.Add(row);
                }
                else
                {
                    if (currentGroup != null && currentGroup.Rows.Count > 0)
                        groups.Add(currentGroup);
                    currentGroup = new Block { Key = key, Rows = new List<DataRow>() };
                    currentGroup.Rows.Add(row);
                    lastKey = key;
                }
            }
            if (currentGroup != null && currentGroup.Rows.Count > 0)
                groups.Add(currentGroup);

            return groups;
        }

        public void ConvertToDataTable()
        {
            DataTable dt = new();
            dt.Columns.Add("KLA", typeof(string));
            dt.Columns.Add("AKT_NAVN", typeof(string));
            dt.Columns.Add("POS", typeof(int));
            dt.Columns.Add("PER", typeof(string));
            foreach (var activity in _activities)
            {
                dt.Rows.Add(activity.KLA, activity.AKT_NAVN, activity.POS, activity.PER);
            }
            // Now _activities is a DataTable that can be saved in Excel format.
            _result = dt;
        }
    }

}
