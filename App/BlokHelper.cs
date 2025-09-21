using App.Domain;

namespace App;

public class BlokHelper
{
    /*
     The BlokCreator class is responsible for analyzing a Block (a group of related rows) to determine if there are 
    at least two POS columns in any row that have matching values (or are both empty), and then checking if this matching condition 
    holds across all rows in the block.
     */
    public bool BlockHasActivity(Block block)
    {
        var posColumns = GetPosColumns(block);

        // Find first row with at least one matching pair
        int rowIdxWithMatch = -1;
        List<(int, int)> matchingPairs = null;
        for (int i = 0; i < posColumns.Count; i++)
        {
            var pairs = GetMatchingColumnPairs(posColumns[i]);
            if (pairs.Count > 0)
            {
                rowIdxWithMatch = i;
                matchingPairs = pairs;
                break;
            }
        }

        if (rowIdxWithMatch == -1)
            return false; // No row has at least two matching columns

        // For each pair, check all other rows
        foreach (var (colA, colB) in matchingPairs)
        {
            bool allRowsMatch = posColumns.All(cols =>
            {
                bool bothEmpty = string.IsNullOrWhiteSpace(cols[colA]) && string.IsNullOrWhiteSpace(cols[colB]);
                bool bothEqual = string.Equals(cols[colA], cols[colB], StringComparison.OrdinalIgnoreCase);
                return bothEmpty || bothEqual;
            });

            if (allRowsMatch)
            {
                // All rows have matching values (or all empty) for columns colA and colB
                // You can process or record this as needed
                // Example: Console.WriteLine($"All rows match for columns {colA} and {colB}");
                return true;
            }
        }
        return false; // No pair found that matches across all rows
    }

    private static List<(int, int)> GetMatchingColumnPairs(string[] posValues)
    {
        var pairs = new List<(int, int)>();
        for (int i = 0; i < posValues.Length; i++)
        {
            for (int j = i + 1; j < posValues.Length; j++)
            {
                bool bothEmpty = string.IsNullOrWhiteSpace(posValues[i]) && string.IsNullOrWhiteSpace(posValues[j]);
                bool bothEqual = string.Equals(posValues[i], posValues[j], StringComparison.OrdinalIgnoreCase);
                if (bothEmpty || bothEqual)
                    pairs.Add((i, j));
            }
        }
        return pairs;
    }
    private List<string[]> GetPosColumns(Block group)
    {
        var posIndices = new[] { Constants.BLOKKE_POS1_idx, Constants.BLOKKE_POS2_idx, Constants.BLOKKE_POS3_idx, Constants.BLOKKE_POS4_idx, Constants.BLOKKE_POS5_idx };
        var posValues = new List<string[]>();

        foreach (var row in group.Rows)
        {
            var values = posIndices
                .Select(idx => row[idx]?.ToString())
                .ToArray();
            posValues.Add(values);
        }

        return posValues;
    }
}

