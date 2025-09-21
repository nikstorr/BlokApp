using App.Domain;
using System;
using System.Data;

namespace App
{
    /*
     This class is responsible for creating activities from a group of DataRow objects (rows in an excel dataTable).
     Each group is represented by a Block object containing multiple DataRow objects.
     The 'CreateActivitiesFromBlock' method analyzes POS columns to identify activities 
     based on matching values across columns.
     */

    public class ActivityCreator
    {
        // HOLD dataTable, used to update POS values as activities are created.
        private readonly DataTable _hold;

        public ActivityCreator(DataTable hold)
        {
            _hold = hold;
        }

        public List<Activity> CreateActivitiesFromBlock(Block block)
        {
            List<Activity> activities = [];
            var posIndices = Constants.PosIndices;

            var posValues = ExtractPosValues(block, posIndices);
            string perCiphers = GetPerCiphers(block);
            string kla = GetKla(block);
            string blok = GetBlok(block);

            // index to track how many PER ciphers have been used
            int perCipherIdx = 0;

            // flag to indicate if AKT_NAVN should be simple (just BLOK) or with index (BLOK + index)
            bool useSimpleAktNavn = false;

            // column index in the POS columns. 0 represents the first POS column ( POS1 ).
            int col = 0;

            //  traverse all POS columns
            while (col < posIndices.Count)
            {
                // Find the largest contiguous block of matching POS columns starting at col
                int blockLen = FindLargestMatchingBlock(posValues, col);
                if (IsValidMultiColumnActivity(posValues, col, blockLen))
                {
                    string per = TakePerCiphers(ref perCipherIdx, blockLen, perCiphers);
                    string aktNavn = useSimpleAktNavn ? blok : $"{blok} {col + 1}";
                    Activity activity = BuildActivity(kla, aktNavn, blockLen, per);
                    activities.Add(activity);

                    UpdateHold(kla, GetStartPosValue(block, posIndices, col));
                    useSimpleAktNavn = IsHoldPosZero(kla, GetStartPosValue(block, posIndices, col));
                    
                    // Advances col by the length of the block just processed.
                    // Thus, skipping over the columns that are part of this activity.
                    col += blockLen;
                }
                else if (IsSingleColumnActivity(posValues, col) && !IsColumnAllEmpty(posValues, col))
                {
                    string per = TakePerCiphers(ref perCipherIdx, 1, perCiphers);
                    string aktNavn = useSimpleAktNavn ? blok : $"{blok} {col + 1}";
                    Activity activity = BuildActivity(kla, aktNavn, 1, per);
                    activities.Add(activity);

                    UpdateHold(kla, GetStartPosValue(block, posIndices, col));
                    useSimpleAktNavn = IsHoldPosZero(kla, GetStartPosValue(block, posIndices, col));
                    col++;
                }
                else
                {
                    // No valid activity found at this column, move to the next column
                    col++;
                }
            }
            return activities;
        }

        /// <summary>
        /// A valid multi-column activity requires at least two columns in the block (blockLen >= 2)
        /// and not all values in the block are empty.
        /// </summary>
        private bool IsValidMultiColumnActivity(List<List<string>> posValues, int col, int blockLen)
        {
            // TODO remove unnecessary check for IsBlockAllEmpty. This will never be the case since input data have been sanitized
            return blockLen >= 2 && !IsBlockAllEmpty(posValues, col, blockLen);
        }

        /// <summary>
        /// Find the largest block possible.
        /// Start by trying the largest possible block at startCol and works backwards, returning immediately when a valid block is found.
        /// It is more efficient in cases where the largest block is likely, as it avoids unnecessary smaller block checks.
        /// 
        ///  (posValues are POS field values for all rows in a block.
        ///  startCol is the index of the first column to investigate)
        /// </summary>
        private int FindLargestMatchingBlock(List<List<string>> posValues, int startCol)
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
        /// each row in the group has matching values/non-values across those columns.
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
        /// Check if all values in the specified block of columns are empty or whitespace.
        /// TODO remove this method. It will never be the case since input data have been sanitized
        /// </summary>
        private bool IsBlockAllEmpty(List<List<string>> posValues, int startCol, int blockLen)
        {
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
        /// Check if there is at least one non-empty value in the specified column across all rows.
        /// Used to identify single column activities.
        /// </summary>
        private bool IsSingleColumnActivity(List<List<string>> posValues, int col)
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
        private bool IsColumnAllEmpty(List<List<string>> posValues, int col)
        {
            for (int r = 0; r < posValues.Count; r++)
            {
                if (!string.IsNullOrWhiteSpace(posValues[r][col]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// extract the values of the POS columns from the group of rows (the block).
        /// <summary>
        private List<List<string>> ExtractPosValues(Block block, List<int> posIndices)
        {
            List<List<string>> posValues = [];
            for (int i = 0; i < block.Rows.Count; i++)
            {
                List<string> rowValues = [];
                for (int j = 0; j < posIndices.Count; j++)
                {
                    object val = block.Rows[i][posIndices[j]];
                    rowValues.Add(val == null ? "" : val.ToString());
                }
                posValues.Add(rowValues);
            }
            return posValues;
        }

        private string GetPerCiphers(Block group)
        {
            object val = group.Rows[0][Constants.BLOKKE_PER_idx];
            return val == null ? "" : val.ToString();
        }

        private string GetKla(Block group)
        {
            object val = group.Rows[0][Constants.BLOKKE_KLA_idx];
            return val == null ? "" : val.ToString();
        }

        private string GetBlok(Block group)
        {
            object val = group.Rows[0][Constants.BLOKKE_BLOK_idx];
            return val == null ? "" : val.ToString();
        }

        /// <summary>
        /// Extract a substring of digits from the PER field whose sum matches the required POS count
        /// ( perCipherIdx: a reference to the current index in the PER string, tracking how many ciphers have already been used (and can't be used nomore).
        ///   posCount: The number of POS columns in the activity (the required sum of digits
        ///   perCiphers: The full PER string from which to extract digits )
        /// </summary>
        private static string TakePerCiphers(ref int perCipherIdx, int posCount, string perCiphers)
        {
            int sum = 0, endIdx = perCipherIdx;
            while (endIdx < perCiphers.Length && sum < posCount && char.IsDigit(perCiphers[endIdx]))
                sum += perCiphers[endIdx++] - '0';
            if (sum == posCount)
            {
                string per = perCiphers.Substring(perCipherIdx, endIdx - perCipherIdx);
                perCipherIdx = endIdx;
                return per;
            }
            // Not enough or too many ciphers to match posCount, return empty and do not advance index
            return "";

            // ALTERNATIVE IMPLEMENTATION!
            //int sum = 0;
            //int startIdx = perCipherIdx;
            //int endIdx = startIdx;
            //while (endIdx < perCiphers.Length && sum < posCount)
            //{
            //    char c = perCiphers[endIdx];
            //    if (char.IsDigit(c))
            //        sum += c - '0';
            //    else
            //        break;
            //    endIdx++;
            //}
            //// Only return if sum matches posCount exactly
            //if (sum == posCount)
            //{
            //    string per = perCiphers.Substring(startIdx, endIdx - startIdx);
            //    perCipherIdx = endIdx;
            //    return per;
            //}
            //// Not enough or too many ciphers to match posCount, return empty and do not advance index
            //return "";
        }

        private static Activity BuildActivity(string kla, string aktNavn, int blockLen, string per)
        {
            return new()
            {
                KLA = kla,
                AKT_NAVN = aktNavn,
                POS = blockLen,
                PER = per
            };
        }

        private string GetStartPosValue(Block group, List<int> posIndices, int col)
        {
            object val = group.Rows[0][posIndices[col]];
            return val == null ? "" : val.ToString();
        }

        /// <summary>
        /// each time an activity is created, the HOLD table must be updated:
        ///     HOLD.POS must be decremented by 1 for the row where HOLD.KLA matches the group's KLA and HOLD.AKT matches the POS value 
        ///     of the BLOK (that indicates the start of the activity).
        /// </summary>
        private void UpdateHold(string kla, string aktValue)
        {
            var row = _hold.AsEnumerable()
                .FirstOrDefault(row => row.Field<string>("KLA") == kla && row.Field<string>("AKT") == aktValue);

            if (row != null)
            {
                // weird syntax is to handle both named and indexed columns. 
                string posField = row.Table.Columns.Contains("POS") ? "POS" : Constants.BLOKKE_POS1_idx.ToString();
                int posVal = 0;
                int.TryParse(row[posField].ToString(), out posVal);
                posVal = Math.Max(0, posVal - 1);
                row[posField] = posVal;
            }

            // ALTERNATIVE IMPLEMENTATION ( without LINQ )
            //for (int i = 0; i < _hold.Rows.Count; i++)
            //{
            //    DataRow row = _hold.Rows[i];
            //    string rowKla = row.Table.Columns.Contains("KLA") ? row["KLA"].ToString() : row[Constants.BLOKKE_KLA_idx].ToString();
            //    string rowAkt = row.Table.Columns.Contains("AKT") ? row["AKT"].ToString() : row[Constants.BLOKKE_BLOK_idx].ToString();
            //    if (rowKla == kla && rowAkt == aktValue)
            //    {
            //        string posField = row.Table.Columns.Contains("POS") ? "POS" : Constants.BLOKKE_POS1_idx.ToString();
            //        int posVal = 0;
            //        int.TryParse(row[posField].ToString(), out posVal);
            //        posVal = Math.Max(0, posVal - 1);
            //        row[posField] = posVal;
            //        break;
            //    }
            //}
        }

        private bool IsHoldPosZero(string kla, string aktValue)
        {
            for (int i = 0; i < _hold.Rows.Count; i++)
            {
                DataRow row = _hold.Rows[i];
                // This weird syntax is to handle both named and indexed columns.
                string rowKla = row.Table.Columns.Contains("KLA") ? row["KLA"].ToString() : row[Constants.BLOKKE_KLA_idx].ToString();
                string rowAkt = row.Table.Columns.Contains("AKT") ? row["AKT"].ToString() : row[Constants.BLOKKE_BLOK_idx].ToString();
                if (rowKla == kla && rowAkt == aktValue)
                {
                    string posField = row.Table.Columns.Contains("POS") ? "POS" : Constants.BLOKKE_POS1_idx.ToString();
                    int posVal = 0;
                    int.TryParse(row[posField].ToString(), out posVal);
                    return posVal == 0;
                }
            }
            return false;
        }
    }
}