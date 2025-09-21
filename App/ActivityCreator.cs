using App.Domain;
using System.Data;

namespace App
{
    /*
     This class is responsible for creating activities from a group of DataRow objects (rows in an excel dataTable).
        Each group is represented by a Block object containing multiple DataRow objects.
      The 'CreateActivitiesFromBlock' method analyzes the POS columns in the rows to 
        identify activities based on matching values across the rows.
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
        /// 
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

        /*
         check that not all rows have empty column in the specified column.
        Used for single column activities
         */
        private bool IsColumnAllEmpty(List<List<string>> posValues, int col)
        {
            for (int r = 0; r < posValues.Count; r++)
            {
                if (!string.IsNullOrWhiteSpace(posValues[r][col]))
                    return false;
            }
            return true;
        }

        /* 
         extract the values of the POS columns from the group of rows (the block).
         */
        private List<List<string>> ExtractPosValues(Block group, List<int> posIndices)
        {
            List<List<string>> posValues = [];
            for (int i = 0; i < group.Rows.Count; i++)
            {
                List<string> rowValues = [];
                for (int j = 0; j < posIndices.Count; j++)
                {
                    object val = group.Rows[i][posIndices[j]];
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
        /// ( perCipherIdx: a reference to the current index in the PER string, tracking how many ciphers have already been used.
        ///   posCount: The number of POS columns in the activity (the required sum of digits
        ///   perCiphers: The full PER string from which to extract digits.)
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

            // Alternative implementation.
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

            // Alternative way without LINQ
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

/*
    Examples of group and activity:

group 1:

     | KLA | BLOK |  PER | POS1 | POS2 | POS3 |
     | 1a  | 1abS |  111 | ty   | ty   | ty   |
     |     |      |      | da   | da   | da   |

activities:

     | KLA | AKT_NAVN | POS | PER  |
     |  1a |  BLOK1 1 |  3  |  111 |

    group 1 results in 1 activity because there is at least two identical POS columns in all rows (ty and da)
        ·	The value can be different for each row, but must be the same across all columns for the block.  
     where 
        KLA is KLA, 
        AKT_NAVN is the BLOK value from the group concatenated with the index of the first POS field in the activity.
        POS is the number of identical POS fields in the activity (in this example 3 because POS1, POS2 and POS3 matches each other in all rows).
        PER is the number of identical POS fields in the activity represented as the PER ciphers from the group 
            (in this example 111 because the activity spans 3 POS columns).

 -------
 group 2:

     | KLA | BLOK  |  PER | POS1 | POS2 | POS3 |
     |1b   | BLAK3 |  111 |      | ty   | ty   |
     |     |       |      | da   | da   | eø   |
group 2 does not result in any activity because there is no way to make an activity that spans at least two POS columns in all rows.
        
        in this example the group does not become an activity because even if both rows have at least two identical column values (ty and da)
            the forst row has values in columns POS2, and POS3 while row 2 has idnetical values in columns POS1 and POS2.
            There is no way to make an activity that spans at least two POS columns in all rows.
 -------
 group 3:

     | KLA | BLOK  |  PER | REF  | POS1 | POS2 | POS3 | POS4 |
     | 3a  | V4    |  211 |	3a   | eø   | eø   | eø	  |      |
     |     |       |      | 3b   | ke   | ke   | ke	  | ke   |
     |     |       |      |	     | Idv  | Idv  | Idv  | Idv  | 
     |     |       |      | 3a   | ps2  | ps2  | ps2  |	     |

     in this example the group becomes two activities. The first activity is because there is at least two identical columns in all rows (eø, ke, idv,ps2). 
        In his example the first activity spans POS1 to POS3 because all these values in each of the rows have similar values.
        The second activity is because POS4 contains data outside the first activity. In this single POS column activity no matching values acrross all rows is needed. 
     The resulting activities becomes: 

activities:
     
     | KLA | AKT_NAVN | POS | PER  |
     |  3a |  V4 1    |  3  |  21  |
     |  3a |  V4 4    |  1  |  1   |

     In the first activity AKT_NAVN is the BLOK value concatenated with the index of the first POS field in the activity 
            (in this example the first activity starts in POS1)
     In the second activity AKT_NAVN is the BLOK value concatenated with the index of the first POS field in the activity 
           (in this example the second activity starts in POS4)

 -------
 group 4:

     | KLA | BLOK  |  PER | REF  | POS1 | POS2 | POS3 | POS4 |
     | 2d  | BLOK1 | 211  |      | ke   | ke   | ke   | ke   | 
     |     |       |      |  3a  | re   | re   |      |      |

     in this example the group becomes two activities. 
        The first activity spans POS columns 1 and 2. 
        The second activity spans POS columns 3 and 4. Notice how the second row does not have values in POS3 and POS4. Tha is still considered matching value. 
     The resulting activities becomes: 

     | KLA | AKT_NAVN | POS | PER  |
     |  2d |  BLOK1 1 |  2  |  2   |    
     |  2d |  BLOK1 3 |  2  |  11  |

     in the first activity AKT_NAVN is the BLOK value concatenated with the index of the first POS field in the activity 
            (in this example the first activity starts in POS1)
        the POS value is 2 because the activity spans two POS columns.
        the PER value is taken from the blok PER fields ciphers (from left to right). We take the number of ciphers which added up equals the number of POS columns in the activity.
            (every time we use ciphers from the group PER field we remove them from that field. They can only be used once.)
    in the second activity AKT_NAVN is the BLOK value concatenated with the index of the first POS field in the activity 
           (in this example the second activity starts in POS3)
        the POS value is 2 because the activity spans two POS columns.
        the PER value is taken from the group PER field's ciphers (from left to right). 
            We take the number of ciphers which added up equals the number of POS columns in the activity.
                In this case we take the ciphers 11 because the first activity already used the first cipher 2.
 --------
 group 5:

     | KLA | BLOK     |  PER | REF  | POS1 | POS2 | POS3 | POS4 | POS5 |
     |  3s |	VB5   |	11111|	    | enA  | enA  | enA  | enA  |      |			
     |     |          |	     | 3u   | enA  | enA  | enA  | enA  |      |			
     |     |	      |	     | 3u   | maA  | maA  | maA  | maA  | maA  |		
     |     |	      |	     |	    | frbA2| frbA2| frbA2|frbA2 | frbA2|			
     |     |	      |	     | 3y   | keA  | keA  | keA  | keA  | keA  |			
     |     |	      |	     | 3v   | maA2 | maA2 | maA2 | maA2 | maA2 |			
     |     |	      |	     |	    | tyfA | tyfA | tyfA | tyfA | tyfA |			
        
           in this example the group becomes two activities. 
            The first activity spans POS columns 1 to 4. 
            The second activity spans POS column 5. Notice how the first activity spans four POS columns because all these columns have matching values across all rows (enA, maA, frbA2, keA, maA2, tyfA).
            The second activity is a single POS column activity because there are no matching values across all rows in POS1 to POS4.

The resulting activities becomes: 

     | KLA | AKT_NAVN | POS | PER  |
     |  3s |  VB5 1   |  4  | 1111 |    
     |  3u |  VB5 5   |  1  |  1   |

---------------
 
For each extracted activity the _hold DataTable must be updated:

fetch the single row from _hold where: 
    KLA matches the group's KLA and 
    AKT matches the value of the POS column that indicates the start of an activity.

Deduct 1 from that row's POS field. 
            
If the _hold POS field is now 0, then AKT_NAVN for the activity becomes simply the BLOK value from group 
    (without the index of the first POS column in the activity).
. Any subsequent activities created for this group will also have AKT_NAVN without the index.


*/



    }
}