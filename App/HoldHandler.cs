using App.Domain;
using System.Data;

namespace App;

public class HoldHandler
{
    public List<Hold> _hold = [];
    private readonly DataTable _holdTable;
    private readonly DataTable _blokkeTable;

    public HoldHandler(DataTable hold, DataTable blokke)
    {
        _holdTable = hold;
        _blokkeTable = blokke;

        CreateHold();
    }

    private void CreateHold()
    {
        var filteredHOLD = Util.RemoveEmptyRows(_holdTable);

        foreach (DataRow row in filteredHOLD.Skip(1)) // skip Header row
        {
            string kla = row[Constants.HOLD_KLA_idx]?.ToString() ?? "invalid";
            string akt = row[Constants.HOLD_AKT_idx]?.ToString() ?? "invalid";

            if (string.IsNullOrWhiteSpace(kla) || string.IsNullOrWhiteSpace(akt))
                continue;

            var pos = row[Constants.HOLD_POS_idx];

            _hold.Add(new Hold
            {
                KLA = kla.Trim(),
                AKT = akt.Trim(),
                POS = !(pos is DBNull) ? Convert.ToInt32(pos) : 0
            });
        }
    }

    /// <summary>
    /// each time an activity is created, the HOLD table must be updated:
    /// the POS field must be decremented by 1 for the specific (KLA, AKT) row that matchs the Blok where,
    /// action.KLA = HOLD.KLA && the actions's first pos index = HOLD.AKT
    /// POS must never go below zero.
    public void UpdateHold(string actionKLA, string actionPOSValue)
    {
        // TODO capture exceptions. Get better data or rewrite requirements

        var hold = _hold.FirstOrDefault(h => 
            h.KLA == actionKLA.Trim() &&
            h.AKT == actionPOSValue.Trim()
        );

        if (hold != null)       
        {
            int newPos = Math.Max(0, hold.POS - 1);
            hold.POS = newPos;
        }
    }

    /// <summary>
    /// check whether the POS field in the _hold DataTable is zero for a specific row identified by KLA and AKT values
    /// </summary>
    public virtual bool IsHoldPosZero(string actionKlA, string aktionPOSValue)
    {
        var hold = _hold.FirstOrDefault(h => h.KLA == actionKlA.Trim() && h.AKT == aktionPOSValue.Trim());
        return hold != null && hold.POS == 0;
    }
}

