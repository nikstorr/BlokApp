using App.Domain;

namespace App
{
    public class ActivityHelper
    {
        /// <summary>
        /// A valid multi-column activity requires at least two columns in the block (blockLen >= 2)
        /// and not all values in the block are empty.
        /// </summary>
        public static bool IsValidMultiColumnActivity(List<List<string>> posValues, int col, int blockLen)
        {
            // TODO remove unnecessary check for IsBlockAllEmpty. This will never be the case since input data have been sanitized
            return blockLen >= 2 && !IsBlockAllEmpty(posValues, col, blockLen);
        }

        /// <summary>
        /// Check if all values in the specified block of columns are empty or whitespace.
        /// </summary>
        private static bool IsBlockAllEmpty(List<List<string>> posValues, int startCol, int blockLen)
        {
            // TODO  remove calls to this method. It will never be true since input data have been sanitized!
            // TODO  remove this method. It will never be true since input data have been sanitized!
            for (int c = startCol; c < startCol + blockLen; c++)
            {
                for (int r = 0; r < posValues.Count; r++)
                {
                    if (!string.IsNullOrWhiteSpace(posValues[r][c]))
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Find the largest block possible.
        /// Start by trying the largest possible block at startCol and work backwards, returning immediately when a valid block is found.
        /// It is more efficient in cases where the largest block is likely, as it avoids unnecessary smaller block checks.
        /// 
        /// posValues are POS field values for all rows in a block.
        /// startCol is the index of the first column to investigate.
        /// </summary>
        public static int FindLargestMatchingBlock(List<List<string>> posValues, int startCol)
        {
            int maxBlockLen = 0;
            int totalCols = posValues[0].Count;
            for (int blockLen = totalCols - startCol; blockLen >= 2; blockLen--)
            {
                if (ColumnsMatch(posValues, startCol, blockLen))
                    return blockLen;
            }
            return maxBlockLen;
        }

        /// <summary>
        /// Determine whether, for a given block of POS columns (starting at startCol and spanning blockLen columns), 
        /// each row in the Block has matching values/non-values across those columns.
        /// </summary>
        private static bool ColumnsMatch(List<List<string>> posValues, int startCol, int blockLen)
        {
            for (int r = 0; r < posValues.Count; r++)
            {
                string firstVal = posValues[r][startCol];
                bool allMatch = true;
                for (int c = startCol; c < startCol + blockLen; c++)
                {
                    string val = posValues[r][c];
                    if (val != firstVal && !(string.IsNullOrWhiteSpace(val) && string.IsNullOrWhiteSpace(firstVal)))
                    {
                        allMatch = false;
                        break;
                    }
                }
                if (!allMatch)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Check if there is at least one non-empty value in the specified column across all rows.
        /// Used to identify single column activities.
        /// </summary>
        public static bool IsSingleColumnActivity(List<List<string>> posValues, int col)
        {
            for (int r = 0; r < posValues.Count; r++)
            {
                if (!string.IsNullOrWhiteSpace(posValues[r][col]))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// check that not all rows have empty column in the specified column.
        /// Used for single column activities
        /// <summary>
        public static bool IsColumnAllEmpty(List<List<string>> posValues, int col)
        {
            // TODO check if the method above can be used instead of this one. It seems to do the same thing. 
            for (int r = 0; r < posValues.Count; r++)
            {
                if (!string.IsNullOrWhiteSpace(posValues[r][col]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// rettrieve the indices for POS columns in a block
        /// </summary>
        public static List<int> GetPosIndices(Block block, int firstPOSIdx)
        {
            if (block is null || block.Rows == null || block.Rows.Count == 0)
                return [];

            var table = block.Rows[0].Table;
            if (table == null)
                return [];

            // Take up to 8 columns (the amount of POS columns in the example file) starting at firstPOSIdx,
            // but do not exceed the column count
            int maxCount = Math.Min(8, table.Columns.Count - firstPOSIdx);
            if (maxCount <= 0)
                return [];

            return Enumerable.Range(firstPOSIdx, maxCount).ToList();

        }

        /// <summary>
        /// extract the values of the POS columns from the group of rows (the block).
        /// <summary>
        public static List<List<string>> GetPosValues(Block block, int firstPosIdx)
        {
            if (block.Rows.Count == 0)
                return [];

            List<List<string>> posValues = [];
            for (int i = 0; i < block.Rows.Count; i++)
            {
                List<string> rowValues = [];
                var indices = GetPosIndices(block, firstPosIdx);
                foreach (var idx in indices)
                {
                    object val = block.Rows[i][idx];
                    rowValues.Add(val == null ? "" : val.ToString());
                }
                posValues.Add(rowValues);
            }
            return posValues;
        }

        public static string GetPerCiphers(Block group)
        {
            object val = group.Rows[0][Constants.BLOKKE_PER_idx];
            return val == null ? "" : val.ToString();
        }

        public static string GetKlaValue(Block group)
        {
            object val = group.Rows[0][Constants.BLOKKE_KLA_idx];
            return val == null ? "" : val.ToString();
        }

        public static string GetBlokValue(Block group)
        {
            object val = group.Rows[0][Constants.BLOKKE_BLOK_idx];
            return val == null ? "" : val.ToString();
        }

        /// <summary>
        /// Extract a substring of digits from the PER field whose sum equals the required POS count 
        /// (the amount of POS columns in the activity).
        /// - perCipherIdx is a reference index that tracks how many digits have been used so far.
        /// - posCount is the number of POS columns in the activity.
        /// - perCiphers is the full string of digits from the PER field.
        /// </summary>
        public static string TakePerCiphers(ref int perCipherIdx, int posCount, string perCiphers)
        {
            int sum = 0, endIdx = perCipherIdx;
            while (endIdx < perCiphers.Length && sum < posCount && char.IsDigit(perCiphers[endIdx]))
            {
                sum += perCiphers[endIdx] - '0';
                endIdx++;
            }
            if (sum == posCount)
            {
                string per = perCiphers.Substring(perCipherIdx, endIdx - perCipherIdx);
                perCipherIdx = endIdx;
                return per;
            }
            // Not enough or too many ciphers to match posCount, return empty and do not advance index
            return "";
        }
    }
}
