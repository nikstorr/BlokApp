using App.Domain;
using App.IO;
using System.Data;

namespace App
{
    public class BlockHandler
    {
        List<string> columnHeaders = [];
        private readonly List<Activity> _activities = [];

        List<Block> _blocks = new();
        private readonly ActivityCreator _activityCreator;

        // index of first POS column
        private int _firstPosIdx = -1;

        public BlockHandler(ActivityCreator activityCreator)
        {
            _activityCreator = activityCreator;
        }

        public List<Block> GetBlocksToProcess(DataTable table)
        {
            GetFirstPOSIdx(table);

            var filteredRows = Util.RemoveEmptyBlokkeRows(table);
            return CreateActivityBlocks(filteredRows);
        }

        public int GetFirstPOSIdx(DataTable table)
        {
            columnHeaders = table.Rows[0].ItemArray
                .Where(item => item != null && item.ToString() != string.Empty)
                .Select(x => x.ToString())
                .ToList();

            _firstPosIdx = columnHeaders.FindIndex(h => h.Contains("POS", StringComparison.OrdinalIgnoreCase));
            return _firstPosIdx;
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
                // skip header rows
                if (key == ("KLA", "BLOK"))
                    continue;

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
    }
}
