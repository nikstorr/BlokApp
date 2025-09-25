using App.Domain;
using System.Data;

namespace App;

public class HoldHandler
{
    public List<Hold> _hold = [];
    private readonly DataTable _holdTable;

    public HoldHandler(DataTable hold)
    {
        _holdTable = hold;

        CreateHold();
    }

    private void CreateHold()
    {       
        foreach (DataRow row in FilterOutEmptyRows()) 
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

    private IEnumerable<DataRow> FilterOutEmptyRows()
    {
        // Remove empty rows and skip the Header row.
        // Which means that all unit tests must have a header row (to be skipped here)!
        return Util.RemoveEmptyRows(_holdTable)
            .Skip(1);
    }   

    /// <summary>
    /// Decrement POS by 1 where:
    /// (KLA, AKT) == the Blok where (action.KLA = HOLD.KLA && action's first pos index = HOLD.AKT)
    /// POS must never go below zero.
    public void UpdateHold(string actionKLA, string actionPOSValue)
    {
        // TODO capture exceptions. And get better data or rewrite requirements

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

