using App.Domain;
using System.Data;

namespace App
{
    public static class Util
    {
        /// <summary>
        /// Remove rows where either KLA or AKT columns are empty or whitespace.
        /// </summary>
        public static IEnumerable<DataRow> RemoveEmptyRows(DataTable table)
        {
            if(table is not null && table.Rows.Count > 0)
            {
                // A row is empty if either KLA or AKT columns are empty or whitespace
                return table.Rows.Cast<DataRow>()
                    .Where(row =>
                    {
                        var kla = row.Table.Columns.Contains("KLA") ? row["KLA"]?.ToString() : null;
                        var akt = row.Table.Columns.Contains("AKT") ? row["AKT"]?.ToString() : null;
                        return !(string.IsNullOrWhiteSpace(kla) || string.IsNullOrWhiteSpace(akt));
                    });

            }
            return Enumerable.Empty<DataRow>();
        }

        //public static List<int> GetPosIndices(DataTable table)
        //{

        //    var posIndices = new List<int>();
        //    for (int i = 1; i <= 8; i++)
        //    {
        //        var colName = $"POS{i}";
        //        if (table.Columns.Contains(colName))
        //        {
        //            posIndices.Add(table.Columns[colName].Ordinal);
        //        }
        //    }
        //    return posIndices;
        //}

        public static List<int> GetAllIndices(DataTable table)
        {
            var indices = new List<int>();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                indices.Add(i);
            }
            return indices;
        }

        public static IEnumerable<DataRow> RemoveEmptyBlokkeRows(DataTable table)
        {
            var posIndices = GetAllIndices(table);
            var result = table.Rows.Cast<DataRow>()
                .Where(row =>

                    // Exclude rows where all columns are empty or null
                    posIndices.Any(posIdx => !row.IsNull(posIdx) && !string.IsNullOrWhiteSpace(row[posIdx]?.ToString()))
                );
            return result;
        }

        /// <summary>
        /// Convert from domain object to DataTable for easy saving to Excel.
        /// </summary>
        public static DataTable ConvertToDataTable(List<Activity> activities)
        {
            DataTable dt = new();
            dt.Locale = System.Globalization.CultureInfo.InvariantCulture;

            dt.Columns.Add("KLA", typeof(string));
            dt.Columns.Add("AKT_NAVN", typeof(string));
            dt.Columns.Add("POS", typeof(int));
            dt.Columns.Add("PER", typeof(string));

            foreach (var activity in activities)
            {
                dt.Rows.Add(activity.KLA, activity.AKT_NAVN, activity.POS, activity.PER);
            }
            // Now _activities is a DataTable that can be saved in Excel format.
            return dt;
        }
    }
}
