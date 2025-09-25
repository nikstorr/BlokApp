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

            var builder = new BlockBuilder();
            
            foreach (var row in filteredRows)
            {
                builder.ProcessRow(row);
            }
            return builder.GetBlocks();
        }

        // Helper class for Block building
        private class BlockBuilder
        {
            private readonly List<Block> _blocks = [];
            private Block _currentBlock = null;
            private (string kla, string blok) _lastKey = (null, null);

            public void ProcessRow(DataRow row)
            {
                var key = GetBlockKey(row);
                if (IsHeaderRow(key))
                    return;

                if (IsEmptyKla(row))
                {
                    AddRowToCurrentOrNewBlock(key, row);
                    return;
                }

                if (IsNewBlock())
                {
                    StartNewBlock(key, row);
                }
                else if (key == _lastKey)
                {
                    _currentBlock.Rows.Add(row);
                }
                else
                {
                    AddBlockIfNotEmpty();
                    StartNewBlock(key, row);
                }
            }

            public List<Block> GetBlocks()
            {
                AddBlockIfNotEmpty();
                return _blocks;
            }

            private (string kla, string blok) GetBlockKey(DataRow row)
            {
                var kla = row[Constants.BLOKKE_KLA_idx]?.ToString();
                var blok = row[Constants.BLOKKE_BLOK_idx]?.ToString();
                return (kla, blok);
            }

            private bool IsHeaderRow((string kla, string blok) key) => key == ("KLA", "BLOK");
            private bool IsEmptyKla(DataRow row) => string.IsNullOrWhiteSpace(row[Constants.BLOKKE_KLA_idx]?.ToString());
            private bool IsNewBlock() => _lastKey.kla == null && _lastKey.blok == null;

            private void StartNewBlock((string kla, string blok) key, DataRow row)
            {
                _currentBlock = new Block { Key = key, Rows = new List<DataRow> { row } };
                _lastKey = key;
            }

            private void AddRowToCurrentOrNewBlock((string kla, string blok) key, DataRow row)
            {
                if (_currentBlock == null)
                {
                    _currentBlock = new Block { Key = key, Rows = new List<DataRow>() };
                    _lastKey = key;
                }
                _currentBlock.Rows.Add(row);
            }

            private void AddBlockIfNotEmpty()
            {
                if (_currentBlock != null && _currentBlock.Rows.Count > 0)
                    _blocks.Add(_currentBlock);
            }
        }
    }
}
