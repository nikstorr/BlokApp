using App;
using NSubstitute;
using System.Data;
using Xunit;

namespace UnitTests
{
    public class BlockHandlerShould
    {
        private readonly ActivityCreator _activityCreatorSubstitute;
        private readonly BlockHandler _blockHandler;

        public BlockHandlerShould()
        {
            _activityCreatorSubstitute = Substitute.For<ActivityCreator>((HoldHandler)null);
            _blockHandler = new BlockHandler(_activityCreatorSubstitute);
        }

        [Fact]
        public void GetBlocksToProcess_ReturnsEmptyList_WhenTableIsEmpty()
        {
            // Arrange
            var table = new DataTable();
            table.Columns.Add("KLA");
            table.Columns.Add("BLOK");
            table.Columns.Add("POS1");

            var row1 = table.NewRow();
            row1["KLA"] = "";
            row1["BLOK"] = "";
            row1["POS1"] = "";
            table.Rows.Add(row1);

            // Act
            var result = _blockHandler.GetBlocksToProcess(table);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void CreateActivityBlocks_GroupsRowsByKlaAndBlok()
        {
            // Arrange
            var table = new DataTable();
            table.Columns.Add("KLA");
            table.Columns.Add("BLOK");
            table.Columns.Add("POS1");

            var row1 = table.NewRow();
            row1["KLA"] = "";
            row1["BLOK"] = "";
            row1["POS1"] = "";
            table.Rows.Add(row1);

            var row2 = table.NewRow();
            row2["KLA"] = "A";
            row2["BLOK"] = "1";
            row2["POS1"] = "da";
            table.Rows.Add(row2);

            var row3 = table.NewRow();
            row3["KLA"] = "";
            row3["BLOK"] = "";
            row3["POS1"] = "da";
            table.Rows.Add(row3);

            var row4 = table.NewRow();
            row4["KLA"] = "B";
            row4["BLOK"] = "2";
            row4["POS1"] = "ty";
            table.Rows.Add(row4);

            var row5 = table.NewRow();
            row5["KLA"] = "";
            row5["BLOK"] = "";
            row5["POS1"] = "eø";
            table.Rows.Add(row5);

            // Act
            var result = _blockHandler.GetBlocksToProcess(table);

            // Assert
            Assert.Equal(2, result.Count);

            Assert.Equal(("A", "1"), result[0].Key);
            Assert.Equal(2, result[0].Rows.Count);
            Assert.Equal("da", result[0].Rows[0]["POS1"]);
            Assert.Equal("da", result[0].Rows[1]["POS1"]);

            Assert.Equal(("B", "2"), result[1].Key);
            Assert.Equal("ty", result[1].Rows[0]["POS1"]);
            Assert.Equal("eø", result[1].Rows[1]["POS1"]);


        }

        [Fact]
        public void CreateActivityBlocks_HandlesRowsWithEmptyKla()
        {
            // Arrange
            var table = new DataTable();
            table.Columns.Add("KLA");
            table.Columns.Add("BLOK");
            table.Columns.Add("POS1");

            var row1 = table.NewRow();
            row1["KLA"] = "1a";
            row1["BLOK"] = "1";
            row1["POS1"] = "da";
            table.Rows.Add(row1);

            var row2 = table.NewRow();
            row2["KLA"] = "";
            row2["BLOK"] = "";
            row2["POS1"] = "da";
            table.Rows.Add(row2);

            var row3 = table.NewRow();
            row3["KLA"] = "";
            row3["BLOK"] = "";
            row3["POS1"] = "da";
            table.Rows.Add(row3);


            // Act
            var result = _blockHandler.GetBlocksToProcess(table);

            // Assert
            Assert.Single(result);
            Assert.Equal(3, result[0].Rows.Count);
        }

        [Fact]
        public void CreateActivityBlocks_StartsNewBlockOnKeyChange()
        {
            // Arrange
            var table = new DataTable();
            table.Columns.Add("KLA");
            table.Columns.Add("BLOK");

            var row1 = table.NewRow();
            row1["KLA"] = "A";
            row1["BLOK"] = "1";
            table.Rows.Add(row1);

            var row2 = table.NewRow();
            row2["KLA"] = "B";
            row2["BLOK"] = "2";
            table.Rows.Add(row2);

            var row3 = table.NewRow();
            row3["KLA"] = "A";
            row3["BLOK"] = "1";
            table.Rows.Add(row3);

            // Act
            var result = _blockHandler.GetBlocksToProcess(table);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(("A", "1"), result[0].Key);
            Assert.Equal(("B", "2"), result[1].Key);
            Assert.Equal(("A", "1"), result[2].Key);
        }
    }
}