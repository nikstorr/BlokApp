using App;
using App.Domain;
using System.Data;
using Xunit;

namespace UnitTests
{
    public class HoldHandlerShould
    {
        private static DataTable CreateHoldTable(params (string kla, string akt, int pos)[] rows)
        {
            var table = new DataTable();
            table.Columns.Add("KLA");
            table.Columns.Add("AKT");
            table.Columns.Add("POS", typeof(int));
            // Add a header row (to be skipped)
            table.Rows.Add("HEADER", "HEADER", 0);
            foreach (var (kla, akt, pos) in rows)
            {
                table.Rows.Add(kla, akt, pos);
            }
            return table;
        }

        private static DataTable CreateBlokkeTable() => new DataTable();

        [Fact]
        public void CreateHold_SkipsHeaderAndEmptyRows()
        {
            var holdTable = CreateHoldTable(("A", "X", 2), ("", "", 0));
            var blokkeTable = CreateBlokkeTable();

            var handler = new HoldHandler(holdTable, blokkeTable, new List<Block>());

            Assert.Single(handler._hold);
            Assert.Equal("A", handler._hold[0].KLA);
            Assert.Equal("X", handler._hold[0].AKT);
            Assert.Equal(2, handler._hold[0].POS);
        }

        [Fact]
        public void UpdateHold_DecrementsPosButNotBelowZero()
        {
            var holdTable = CreateHoldTable(("A", "X", 1));
            var blokkeTable = CreateBlokkeTable();
            var handler = new HoldHandler(holdTable, blokkeTable, new List<Block>());

            handler.UpdateHold("A", "X");
            Assert.Equal(0, handler._hold[0].POS);

            handler.UpdateHold("A", "X");
            Assert.Equal(0, handler._hold[0].POS); // Should not go below zero
        }

        [Fact]
        public void UpdateHold_DoesNothingIfNoMatch()
        {
            var holdTable = CreateHoldTable(("A", "X", 2));
            var blokkeTable = CreateBlokkeTable();
            var handler = new HoldHandler(holdTable, blokkeTable, new List<Block>());

            handler.UpdateHold("B", "Y");
            Assert.Equal(2, handler._hold[0].POS);
        }

        [Fact]
        public void IsHoldPosZero_ReturnsTrueIfPosIsZero()
        {
            var holdTable = CreateHoldTable(("A", "X", 0));
            var blokkeTable = CreateBlokkeTable();
            var handler = new HoldHandler(holdTable, blokkeTable, new List<Block>());

            Assert.True(handler.IsHoldPosZero("A", "X"));
        }

        [Fact]
        public void IsHoldPosZero_ReturnsFalseIfPosIsNotZero()
        {
            var holdTable = CreateHoldTable(("A", "X", 2));
            var blokkeTable = CreateBlokkeTable();
            var handler = new HoldHandler(holdTable, blokkeTable, new List<Block>());

            Assert.False(handler.IsHoldPosZero("A", "X"));
        }

        [Fact]
        public void IsHoldPosZero_ReturnsFalseIfNoMatch()
        {
            var holdTable = CreateHoldTable(("A", "X", 0));
            var blokkeTable = CreateBlokkeTable();
            var handler = new HoldHandler(holdTable, blokkeTable, new List<Block>());

            Assert.False(handler.IsHoldPosZero("B", "Y"));
        }
    }
}