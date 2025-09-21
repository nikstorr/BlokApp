using App;
using System.Data;
using Xunit;

namespace UnitTests
{
    public class ActivityHandlerShould
    {

        [Fact]
        public void Test1()
        {
            var sut = new Handler();

            sut.HandleActivities();
            var activities = sut.Result;
            var data = Data();

            foreach (DataRow row in activities.Rows)
            {
                var kla = row["KLA"].ToString();
                var akt_navn = row["AKT_NAVN"].ToString();
                var pos = (int)row["POS"];
                var per = row["PER"].ToString();

                var match = activities
                    .AsEnumerable()
                    .FirstOrDefault(a =>
                        a.Field<string>("KLA") == kla &&
                        a.Field<string>("AKT_NAVN") == akt_navn &&
                        a.Field<int>("POS") == pos &&
                        a.Field<string>("PER") == per);

                Assert.NotNull(match);
            }
        }


        private DataTable Data()
        {
            DataTable _activities = new();
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
            _activities.Rows.Add("3a", "V2 4 ", 1, "1");

            _activities.Rows.Add("3a", "V3 1", 3, "21");
            _activities.Rows.Add("3a", "V3 4", 1, "1");

            _activities.Rows.Add("3a", "V1 1", 2, "2" );
            _activities.Rows.Add("3a", "V1 3", 1, "1");
            _activities.Rows.Add("3a", "V1 4", 1, "1");

            _activities.Rows.Add("3c", "3cSM 1", 3, "111");
            return _activities;
        }

        private void WrongData()
        {/*
            1a; "1abS 1"; 3; 111
            1b; "1bK 1"; 1; 1
            1b; 1bK; 1; 1
            1d; "1deS 1"; 3; 111
            2a; "2gS1 1"; 1; 1
            2a; "2gS1 2"; 1; 1
            2a; 2gS1; 1; 1
            2b; "2bcMa 1"; 4; 1111
            2d; "BLOK1 1"; 1; 999
            2d; "BLOK1 2"; 1; 999
            2d; "BLOK1 3"; 1; 999
            2d; BLOK1; 1; 999
            3a; "V4 1"; 1; 999
            3a; "V4 2"; 1; 999
            3a; V4; 1; 999
            3a; "V4 4"; 1; 999
            3a; V2; 1; 999
            3a; V2; 1; 999
            3a; V2; 1; 999
            3a; V2; 1; 999
            3a; "V3 1"; 1; 999
            3a; "V3 2"; 1; 999
            3a; "V3 3"; 1; 999
            3a; V3; 1; 999
            3a; V1; 1; 999
            3a; V1; 1; 999
            3a; V1; 1; 999
            3a; V1; 1; 999
            3c; "3cSM 1"; 1; 1
            3c; "3cSM 2"; 1; 1
            3c; 3cSM; 1; 1
            */
        }
    }
}
