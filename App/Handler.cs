using App.Domain;
using App.IO;
using System.Data;

namespace App
{
    public class Handler
    {
        readonly string _path = "../../../../Files/Nikolai-Arbejdsopgave_2.xlsx";
        // all tables in the excel input file
        readonly DataSet _tableSet = new();
        // The resulting activities as a DataTable ready to write as a new excel spreadsheet
        DataTable _result = new();
        public DataTable Result => _result;
        // HOLD and BLOKKE tables
        DataTable _holdTable = new();
        DataTable _blokkeTable = new();
        // resulting Activities as domain objects, before converting into DataTable
        readonly List<Activity> _activities = [];

        readonly ActivityCreator _activityCreator;

        public Handler()
        {
            _tableSet = new ExcelReader().ReadExcelFile(_path);
            // TODO check if it is an anti-pattern (to use the HOLD table here in the ctor, before it is assigned anything.
            _activityCreator =  new ActivityCreator(_holdTable);
        }

        /// <summary>
        /// Entry point.
        /// </summary>
        public void HandleBlocks()
        {
            PopulateTables();

            foreach (var block in GetBlocksToProcess(_blokkeTable))
            {
                var activities = _activityCreator.CreateActivitiesFromBlock(block);
                foreach(var act in activities)
                    _activities.Add(act);
            }

            // convert from domain objects to DataTable for easy saving to Excel
            ConvertToDataTable();
        }

        private void PopulateTables()
        {
            // Fetch the HOLD table (5th table, index 4) from the DataSet, here!
            // Don't fetch it in the ctor. Somehow that breaks the ExcelReader.
            _holdTable = _tableSet.Tables[Constants.HOLD_idx];
            // Fetch the BLOKKE table (6th table, index 5) from the _tableSet
            // Don't fetch it in the ctor. That messes with the Excel data reader, somehow.
            _blokkeTable = _tableSet.Tables[Constants.BLOKKE_idx];
        }

        private List<Block> GetBlocksToProcess(DataTable table)
        {
            var filteredRows = RemoveEmptyRows(table);
            return CreateActivityBlocks(filteredRows);
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

        /// <summary>
        ///  Group a sequence of DataRow objects (from the BLOKKE table) into logical blocks, each represented by a Block object. 
        ///  Each block corresponds to a set of rows that share the same (KLA, BLOK) key
        /// </summary>
        private List<Block> CreateActivityBlocks(IEnumerable<DataRow> rows)
        {
            // TODO : This can probably be done more elegantly with LINQ GroupBy, or some such.
            // either way, this method is too long and should be refactored.

            var blocks = new List<Block>();
            Block currentBlock = null;
            (string kla, string blok) lastKey = (null, null);

            foreach (var row in rows)
            {
                var kla = row[Constants.BLOKKE_KLA_idx]?.ToString();
                var blok = row[Constants.BLOKKE_BLOK_idx]?.ToString();
                var key = (kla, blok);

                // If KLA is empty, add row to current group (if any), or create a new group if none exists
                if (string.IsNullOrWhiteSpace(kla))
                {
                    if (currentBlock == null)
                    {
                        currentBlock = new Block { Key = key, Rows = new List<DataRow>() };
                        lastKey = key;
                    }
                    currentBlock.Rows.Add(row);
                    continue;
                }

                if (lastKey.kla == null && lastKey.blok == null)
                {
                    currentBlock = new Block { Key = key, Rows = new List<DataRow>() };
                    currentBlock.Rows.Add(row);
                    lastKey = key;
                }
                else if (key == lastKey)
                {
                    currentBlock.Rows.Add(row);
                }
                else
                {
                    if (currentBlock != null && currentBlock.Rows.Count > 0)
                        blocks.Add(currentBlock);
                    currentBlock = new Block { Key = key, Rows = new List<DataRow>() };
                    currentBlock.Rows.Add(row);
                    lastKey = key;
                }
            }
            if (currentBlock != null && currentBlock.Rows.Count > 0)
                blocks.Add(currentBlock);

            return blocks;
        }

        /// <summary>
        /// Convert from domain object to DataTable for easy saving to Excel.
        /// </summary>
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
