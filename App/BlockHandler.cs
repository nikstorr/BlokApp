using App.Domain;
using System.Data;

namespace App
{
    /*
     this class is responsible for creating Blocks from the BLOKKE table
     */
    public class BlockHandler
    {
        public List<Block> GetBlocksToProcess(DataTable table)
        {
            var filteredRows = Util.RemoveEmptyBlokkeRows(table);
            return CreateActivityBlocks(filteredRows);
        }

        /// <summary>
        ///  Group a sequence of DataRow objects (from the BLOKKE table) into logical blocks, each represented by 
        ///  a Block object. Each block corresponds to a set of rows that share the same (KLA, BLOK) key
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
