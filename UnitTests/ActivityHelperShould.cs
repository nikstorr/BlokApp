using App;
using App.Domain;
using FluentAssertions;
using System.Data;
using Xunit;

namespace UnitTests;

public class ActivityHelperShould
{
    private List<List<string>> CreatePosValues(params string[][] rows)
    {
        var result = new List<List<string>>();
        foreach (var row in rows)
            result.Add(new List<string>(row));
        return result;
    }

    private Block CreateBlock(string kla, string blok, string per, params string[][] posRows)
    {
        var table = new DataTable();
        table.Columns.Add("KLA", typeof(string));
        table.Columns.Add("BLOK", typeof(string));
        table.Columns.Add("PER", typeof(string));
        for (int i = 1; i <= posRows[0].Length; i++)
            table.Columns.Add($"POS{i}", typeof(string));

        var dataRows = new List<DataRow>();
        foreach (var pos in posRows)
        {
            var dr = table.NewRow();
            dr["KLA"] = kla;
            dr["BLOK"] = blok;
            dr["PER"] = per;
            for (int i = 0; i < pos.Length; i++)
                dr[$"POS{i + 1}"] = pos[i];
            dataRows.Add(dr);
        }
        return new Block { Key = (kla, blok), Rows = dataRows };
    }

    [Fact]
    public void Should_Validate_MultiColumnActivity()
    {
        var helper = new ActivityHelper();
        var posValues = CreatePosValues(
            new[] { "A", "A", "A" },
            new[] { "A", "A", "A" }
        );
        Assert.True(helper.IsValidMultiColumnActivity(posValues, 0, 3));
        Assert.False(helper.IsValidMultiColumnActivity(posValues, 0, 1));
    }

    [Fact]
    public void Should_Find_Largest_Matching_Block()
    {
        var helper = new ActivityHelper();
        var posValues = CreatePosValues(
            new[] { "X", "X", "Y", "Y" },
            new[] { "X", "X", "Y", "Y" }
        );
        Assert.Equal(2, helper.FindLargestMatchingBlock(posValues, 0));
        Assert.Equal(2, helper.FindLargestMatchingBlock(posValues, 2));
    }

    [Fact]
    public void Should_Identify_Single_Column_Activity()
    {
        var helper = new ActivityHelper();
        var posValues = CreatePosValues(
            new[] { "", "B", "" },
            new[] { "", "", "" }
        );
        Assert.True(helper.IsSingleColumnActivity(posValues, 1));
        Assert.False(helper.IsSingleColumnActivity(posValues, 0));
    }

    [Fact]
    public void Should_Check_If_Column_All_Empty()
    {
        var helper = new ActivityHelper();
        var posValues = CreatePosValues(
            new[] { "", "", "" },
            new[] { "", "", "" }
        );
        Assert.True(helper.IsColumnAllEmpty(posValues, 1));

        posValues = CreatePosValues(
            new[] { "", "C", "" },
            new[] { "", "", "" }
        );
        Assert.False(helper.IsColumnAllEmpty(posValues, 1));
    }

    [Fact]
    public void Should_Extract_Pos_Values()
    {
        var helper = new ActivityHelper();
        var block = CreateBlock("1a", "BLOK1", "123", new[] { "A", "B", "C" }, new[] { "D", "E", "F" });
        var posIndices = new List<int> { 3, 4, 5 }; // POS1, POS2, POS3 columns

        var posValues = helper.ExtractPosValues(block, 3);
        Assert.Equal(2, posValues.Count);
        new List<string> { "A", "B", "C" }.Should().BeEquivalentTo(posValues[0]);
        new List<string> { "D", "E", "F" }.Should().BeEquivalentTo(posValues[1]);
    }

    [Fact]
    public void Should_Get_Per_Ciphers()
    {
        var helper = new ActivityHelper();
        var block = CreateBlock("1a", "BLOK1", "123", new[] { "A", "B", "C" });
        Assert.Equal("123", helper.GetPerCiphers(block));
    }

    [Fact]
    public void Should_Get_Kla_And_Blok_Values()
    {
        var helper = new ActivityHelper();
        var block = CreateBlock("1a", "BLOK1", "123", new[] { "A", "B", "C" });
        Assert.Equal("1a", helper.GetKlaValue(block));
        Assert.Equal("BLOK1", helper.GetBlokValue(block));
    }

    [Fact]
    public void Should_Take_Per_Ciphers()
    {
        var helper = new ActivityHelper();
        int idx = 0;
        string per = helper.TakePerCiphers(ref idx, 3, "1213");
        Assert.Equal("12", per);
        Assert.Equal(2, idx);

        idx = 0;
        per = helper.TakePerCiphers(ref idx, 6, "1234");
        Assert.Equal("123", per);
        Assert.Equal(3, idx);

    }
}