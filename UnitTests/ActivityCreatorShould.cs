using App;
using App.Domain;
using NSubstitute;
using System.Collections.Generic;
using System.Data;
using Xunit;

namespace UnitTests
{
    public class ActivityCreatorShould
    {
        [Fact]
        public void CreateActivitiesFromBlock_ReturnsEmptyList_WhenBlockIsNull()
        {
            var randomFirsPOSIdx = 3;

            var holdHandler = Substitute.For<HoldHandler>((DataTable)null, (DataTable)null);
            var creator = new ActivityCreator(holdHandler);

            var result = creator.CreateActivitiesFromBlock(null, randomFirsPOSIdx);

            Assert.Empty(result);
        }

        [Fact]
        public void CreateActivitiesFromBlock_ReturnsEmptyList_WhenBlockHasNoRows()
        {
            var randomFirsPOSIdx = 3;

            var holdHandler = Substitute.For<HoldHandler>((DataTable)null, (DataTable)null);
            var creator = new ActivityCreator(holdHandler);
            var block = new Block { Key = ("A","B"), Rows = new List<DataRow>() };

            var result = creator.CreateActivitiesFromBlock(block, randomFirsPOSIdx);

            Assert.Empty(result);
        }

        [Fact]
        public void CreateActivitiesFromBlock_CreatesActivities_WithExpectedAktNavn()
        {
            var holdHandler = Substitute.For<HoldHandler>((DataTable)null, (DataTable)null);
            holdHandler.IsHoldPosZero(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
            var creator = new ActivityCreator(holdHandler);

            // Setup block with two rows and POS columns
            var table = new DataTable();
            table.Columns.Add("KLA");
            table.Columns.Add("BLOK");
            table.Columns.Add("POS1");

            var row1 = table.NewRow();
            row1["KLA"] = "A";
            row1["BLOK"] = "BLOK";
            row1["POS1"] = "foo";
            table.Rows.Add(row1);
            
            var row2 = table.NewRow();
            row2["KLA"] = "";
            row2["BLOK"] = "";
            row2["POS1"] = "foo";
            table.Rows.Add(row2);

            var block = new Block { Rows = new List<DataRow> { row1, row2 } };

            var result = creator.CreateActivitiesFromBlock(block, 2);

            Assert.All(result, a => Assert.StartsWith("BLOK", a.AKT_NAVN));
            Assert.Single(result);
        }

        [Fact]
        public void CreateActivitiesFromBlock_CallsIsHoldPosZero()
        {
            var holdHandler = Substitute.For<HoldHandler>(Substitute.For<DataTable>(), Substitute.For<DataTable>());
            holdHandler.IsHoldPosZero(Arg.Any<string>(), Arg.Any<string>()).Returns(false); 
            
            var creator = new ActivityCreator(holdHandler);

            var table = new DataTable();
            table.Columns.Add("KLA");
            table.Columns.Add("BLOK");
            table.Columns.Add("POS1");
            var row = table.NewRow();
            row["KLA"] = "A";
            row["BLOK"] = "1";
            row["POS1"] = "foo";
            table.Rows.Add(row);

            var block = new Block { Key = ("A","1"), Rows = new List<DataRow> { row } };

            creator.CreateActivitiesFromBlock(block, 2);

            holdHandler.Received().IsHoldPosZero(Arg.Any<string>(), Arg.Any<string>());
        }
    }
}