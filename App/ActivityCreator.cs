using App.Domain;

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
        HoldHandler _hold;
        ActivityHelper _helper;

        public ActivityCreator(HoldHandler holdHandler)
        {
            _hold = holdHandler;
            _helper = new ActivityHelper();
        }

        public List<Activity> CreateActivitiesFromBlock(Block block, int firstPOSIdx)
        {
            if (block == null || block.Rows.Count == 0)
                return [];

            List<Activity> activities = [];

            var posIndices = _helper.GetPosIndices(block, firstPOSIdx);
            var posValues = _helper.ExtractPosValues(block, firstPOSIdx);

            string perCiphers = _helper.GetPerCiphers(block);
            string kla = _helper.GetKlaValue(block);
            string blok = _helper.GetBlokValue(block);

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
                int blockLen = _helper.FindLargestMatchingBlock(posValues, col);
                if (_helper.IsValidMultiColumnActivity(posValues, col, blockLen))
                {
                    string per = _helper.TakePerCiphers(ref perCipherIdx, blockLen, perCiphers);
                    string aktNavn = useSimpleAktNavn ? blok : $"{blok} {col + 1}";
                    Activity activity = BuildActivity(kla, aktNavn, blockLen, per);
                    activities.Add(activity);

                    UpdateHold(kla, GetStartPosValue(block, posIndices, col));
                    useSimpleAktNavn = _hold.IsHoldPosZero(kla, GetStartPosValue(block, posIndices, col));
                    
                    // Advance col by the length of the block just processed.
                    // Thus, skipping over the columns that are part of this activity.
                    col += blockLen;
                }
                else if (_helper.IsSingleColumnActivity(posValues, col) && !_helper.IsColumnAllEmpty(posValues, col))
                {
                    string per = _helper.TakePerCiphers(ref perCipherIdx, 1, perCiphers);
                    string aktNavn = useSimpleAktNavn ? blok : $"{blok} {col + 1}";
                    Activity activity = BuildActivity(kla, aktNavn, 1, per);
                    activities.Add(activity);

                    UpdateHold(kla, GetStartPosValue(block, posIndices, col));
                    useSimpleAktNavn = _hold.IsHoldPosZero(kla, GetStartPosValue(block, posIndices, col));
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

        // value of the first POS column in the block (representing HOLD.AKT)
        // used to update POS in the HOLD table
        private string GetStartPosValue(Block group, List<int> posIndices, int col)
        {
            object val = group.Rows[0][posIndices[col]];
            return val == null ? "" : val.ToString();
        }

        /// <summary>
        /// each time an activity is created, the matching row in HOLD table must be updated:
        /// kla: BLOKKE.KLA, must match the HOLD's KLA.
        /// aktValue: the value in the first POS column of the activity/BLOK, must match HOLD.AKT.
        /// </summary>
        private void UpdateHold(string kla, string posValue)
        {
            _hold.UpdateHold(kla, posValue);
        }
    }
}