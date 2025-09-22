using App;
using System.Data;
using System.Globalization;
using Xunit;

namespace UnitTests;

public class ActivityHandlerShould
{
    [Fact]
    public void Test1()
    {
        // Arrange
        var sut = new BlockHandler();

        // Act
        sut.HandleBlocks();

        var actual = sut.Result;
        var expected = Expected();
                
        // Assert
        Assert.Equal(expected.Rows.Count, actual.Rows.Count);

        foreach (DataRow expectedRow in expected.Rows)
        {
            var expectedKla = expectedRow["KLA"].ToString();
            var expectedAktNavn = expectedRow["AKT_NAVN"].ToString();
            var expectedPos = (int)expectedRow["POS"];
            var expectedPer = expectedRow["PER"].ToString();

            var match = actual.AsEnumerable()
            .FirstOrDefault(a =>
                a.Field<string>("KLA") == expectedKla &&
                a.Field<string>("AKT_NAVN") == expectedAktNavn &&
                a.Field<int>("POS") == expectedPos &&
                a.Field<string>("PER") == expectedPer);

            Assert.NotNull(match);
        }
    }

    private static DataTable Expected()
    {
        DataTable _activities = new();
        _activities.Locale = CultureInfo.InvariantCulture;

        _activities.Columns.Add("KLA", typeof(string));
        _activities.Columns.Add("AKT_NAVN", typeof(string));
        _activities.Columns.Add("POS", typeof(int));
        _activities.Columns.Add("PER", typeof(string));

        _activities.Rows.Add("1a", "1abS 1", 3, "111");

        _activities.Rows.Add("1b", "1bK 1", 2, "11");

        _activities.Rows.Add("1d", "1deS 1", 3, "111");

        _activities.Rows.Add("2a", "2gS1 1", 3, "111");

        _activities.Rows.Add("2b", "2bcMa 1", 4, "1111");

        _activities.Rows.Add("2d", "BLOK1 1", 2, "2");
        _activities.Rows.Add("2d", "BLOK1 3", 2, "11");

        _activities.Rows.Add("3a", "V4 1", 3, "21");
        _activities.Rows.Add("3a", "V4 4", 1, "1");

        _activities.Rows.Add("3a", "V2 1", 3, "21");
        _activities.Rows.Add("3a", "V2 4", 1, "1");

        _activities.Rows.Add("3a", "V3 1", 3, "21");
        _activities.Rows.Add("3a", "V3 4", 1, "1");

        _activities.Rows.Add("3a", "V1 1", 2, "2");
        _activities.Rows.Add("3a", "V1 3", 1, "1");
        _activities.Rows.Add("3a", "V1 4", 1, "1");

        _activities.Rows.Add("3c", "3cSM 1", 3, "111");
        return _activities;
    }
}

