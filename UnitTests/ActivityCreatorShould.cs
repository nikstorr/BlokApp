using Xunit;
using System.Data;
using App;
using App.Domain;

namespace UnitTests;

public class ActivityCreatorShould
{
    // Note there will never be a block with no activities in it,
    // as those blocks are filtered out before calling ActivityCreator

    private DataTable CreateHoldTable()
    {
        var table = new DataTable();
        table.Columns.Add("KLA", typeof(string));
        table.Columns.Add("AKT", typeof(string));
        table.Columns.Add("FAG", typeof(string));
        table.Columns.Add("POS", typeof(int));
        // Add a row for testing
        table.Rows.Add("1a", "ty", "FAG1", 2);
        table.Rows.Add("1a", "da", "FAG2", 1);
        return table;
    }

    private Block CreateBlock(params (string kla, string blok, string per, string[] pos)[] rows)
    {
        var table = new DataTable();
        table.Columns.Add("KLA", typeof(string));
        table.Columns.Add("BLOK", typeof(string));
        table.Columns.Add("PER", typeof(string));
        table.Columns.Add("REF", typeof(string));
        table.Columns.Add("POS1", typeof(string));
        table.Columns.Add("POS2", typeof(string));
        table.Columns.Add("POS3", typeof(string));
        table.Columns.Add("POS4", typeof(string));
        table.Columns.Add("POS5", typeof(string));
        table.Columns.Add("POS6", typeof(string));
        table.Columns.Add("POS7", typeof(string));
        table.Columns.Add("POS8", typeof(string));

        var dataRows = new List<DataRow>();
        foreach (var row in rows)
        {
            var dr = table.NewRow();
            dr["KLA"] = row.kla;
            dr["BLOK"] = row.blok;
            dr["PER"] = row.per;
            dr["POS1"] = row.pos.Length > 0 ? row.pos[0] : "";
            dr["POS2"] = row.pos.Length > 1 ? row.pos[1] : "";
            dr["POS3"] = row.pos.Length > 2 ? row.pos[2] : "";
            dr["POS4"] = row.pos.Length > 3 ? row.pos[3] : "";
            dr["POS5"] = row.pos.Length > 4 ? row.pos[4] : "";
            dr["POS6"] = row.pos.Length > 5 ? row.pos[5] : "";
            dr["POS7"] = row.pos.Length > 6 ? row.pos[6] : "";
            dr["POS8"] = row.pos.Length > 7 ? row.pos[7] : "";
            dataRows.Add(dr);
        }
        return new Block { Key = (rows[0].kla, rows[0].blok), Rows = dataRows };
    }

    [Fact]
    public void Should_Create_Activities_From_Block_With_Matching_POS()
    {
        var holdTable = CreateHoldTable();
        var creator = new ActivityCreator(holdTable);

        // Group with two rows, POS1-POS3 all match per row
        var block = CreateBlock(
            ("1a", "1abS", "111", new[] { "ty", "ty", "ty", "", "", "", "", "" }),
            ("", "", "", new[] { "da", "da", "da", "", "", "", "", "" })
        );

        Assert.Equal(2, holdTable.Rows[0]["POS"]);
        var activities = creator.CreateActivitiesFromBlock(block);

        Assert.Single(activities);
        Assert.Equal("1a", activities[0].KLA);
        Assert.Equal("1abS 1", activities[0].AKT_NAVN);
        Assert.Equal(3, activities[0].POS);
        Assert.Equal("111", activities[0].PER);
        // POS in holdTable should be decremented for "ty"
        Assert.Equal(1, holdTable.Rows[0]["POS"]);
    }

    [Fact]
    public void Should_Create_3_Single_Column_Activities()
    {
        var holdTable = CreateHoldTable();
        var creator = new ActivityCreator(holdTable);

        var block = CreateBlock(
            ("1b", "BLAK3", "111", new[] { ""  , "ty", "ty", "", "", "", "", "" }),
            (""  , ""     , ""   , new[] { "da", "it", "eø", "", "", "", "", "" })
        );

        var activities = creator.CreateActivitiesFromBlock(block);

        Assert.True(activities.Count == 3);
    }

    [Fact]
    public void Should_Create_Single_Column_Activity_If_At_Least_One_Value()
    {
        var holdTable = CreateHoldTable();
        var creator = new ActivityCreator(holdTable);

        // Only POS1 has a value in one row
        var block = CreateBlock(
            ("1a", "BLOK1", "1", new[] { "ty", "", "", "", "" }),
            ("", "", "", new[] { "", "", "", "", "" })
        );

        var activities = creator.CreateActivitiesFromBlock(block);

        Assert.Single(activities);
        Assert.Equal("1a", activities[0].KLA);
        Assert.Equal("BLOK1 1", activities[0].AKT_NAVN);
        Assert.Equal(1, activities[0].POS);
        Assert.Equal("1", activities[0].PER);
        // POS in holdTable should be decremented for "ty"
        Assert.Equal(1, holdTable.Rows[0]["POS"]);
    }
}