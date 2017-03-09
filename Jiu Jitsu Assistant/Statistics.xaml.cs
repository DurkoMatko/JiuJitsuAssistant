using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System.Windows.Media;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Data;
using System.Collections.Generic;

namespace Jiu_Jitsu_Assistant
{
   /// <summary>
   /// Interaction logic for Statistics.xaml
   /// </summary>
   public partial class Statistics : Window
   {
      MySqlConnection conn;
      public DataTable myTechniquesTable { get; set; }
      public DataTable techniqueGroupsTable { get; set; }

      public SeriesCollection BarChart { get; set; }
      public string[] Labels_BarChart { get; set; }

      Dictionary<string, List<int>> groupsPerMonthCounts = new Dictionary<string, List<int>>();

      public SeriesCollection myChronological { get; set; }
      public Func<double, string> myChronological_XFormatter { get; set; }

      ChartValues<DateTimePoint> chokesChartValues_Stacked = new ChartValues<DateTimePoint>();
      ChartValues<DateTimePoint> guardsChartValues_Stacked = new ChartValues<DateTimePoint>();
      ChartValues<DateTimePoint> jointLocksChartValues_Stacked = new ChartValues<DateTimePoint>();
      ChartValues<DateTimePoint> escapesChartValues_Stacked = new ChartValues<DateTimePoint>();
      ChartValues<DateTimePoint> sweepsChartValues_Stacked = new ChartValues<DateTimePoint>();

      public Statistics()
      {
         InitializeComponent();
         if (ConnectToDatabase())
         {
            LoadTechniques();
            LoadedTechniqueGroups();
         }
      }

      public Statistics(double left, double top, double height, double width) : this()
      {
         this.Left = left;
         this.Top = top;
         this.Height = height;
         this.Width = width;

         InitializeComponent();


         Dictionary<int, string> monthsNamesDict = new Dictionary<int, string>()
         {
            {1,"January" },
            {2,"February" },
            {3,"March" },
            {4,"April" },
            {5,"Mai" },
            {6,"June" },
            {7,"July" },
            {8,"August" },
            {9,"September" },
            {10,"October" },
            {11,"November" },
            {12,"December" },
         };


         int maxMonth = this.setBarChartValuesAndReturnMaxMonth();
         BarChart = new SeriesCollection { };
         List<string> monthsUsed = new List<string>();
         //loop dictionary of months and technique counts
         foreach (KeyValuePair<string, List<int>> groupCountsPair in groupsPerMonthCounts)
         {
            //for each month create new chartValue of counts per each month
            ChartValues<int> tempChartValues = new ChartValues<int>();
            int i = 0;
            while (i != maxMonth)
            {
               tempChartValues.Add(groupCountsPair.Value[i]);
               monthsUsed.Add(monthsNamesDict[i + 1]);
               i++;
            }

            //add month name and chartValues to actuall seriesCollection
            BarChart.Add(new ColumnSeries
            {
               Title = groupCountsPair.Key,
               Values = tempChartValues
            });
         }
         Labels_BarChart = monthsUsed.ToArray();
         DataContext = this;
         //adding empty values technique type so it will show itself as empty bar -> causing an illusion of separated months
         BarChart.Add(new ColumnSeries{Title ="", Values = new ChartValues<int> { }});


         //************       Stacked Graph part       *********//
         this.setStackedChartValues();
         myChronological = new SeriesCollection
            {
                new StackedAreaSeries
                {
                    Title = "Chokes",
                    Values = chokesChartValues_Stacked,
                    LineSmoothness = 0
                },
                new StackedAreaSeries
                {
                    Title = "Guards",
                    Values = guardsChartValues_Stacked,
                    LineSmoothness = 0
                },
                new StackedAreaSeries
                {
                    Title = "Joint locks",
                    Values = jointLocksChartValues_Stacked,
                    LineSmoothness = 0
                },
                new StackedAreaSeries
                {
                    Title = "Sweeps",
                    Values = sweepsChartValues_Stacked,
                    LineSmoothness = 0
                },
                new StackedAreaSeries
                {
                    Title = "Escapes",
                    Values = escapesChartValues_Stacked,
                    LineSmoothness = 0
                }
            };

         myChronological_XFormatter = val => new DateTime((long)val).ToString("dd/MM/yyyy");
         DataContext = this;

      }

      private Boolean ConnectToDatabase()
      {
         try
         {
            string MyConString =
            "Server=localhost;" +
            "Database=jiujitsuassistant;" +
            "Uid=root;" +
            "Pwd=root;";

            this.conn = new MySqlConnection(MyConString);
            this.conn.Open();
            return true;
         }
         catch (MySql.Data.MySqlClient.MySqlException e)
         {
            return false;
         }
      }

      private void LoadTechniques()
      {
         try
         {
            MySqlCommand cmd;
            cmd = this.conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM techniques ORDER BY date_learned ASC";
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            this.myTechniquesTable = ds.Tables[0];
         }
         catch (MySql.Data.MySqlClient.MySqlException e)
         {
         }
      }


      private void LoadedTechniqueGroups()
      {
         try
         {
            MySqlCommand cmd;
            cmd = this.conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM techniquegroups";
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            this.techniqueGroupsTable = ds.Tables[0];
         }
         catch (MySql.Data.MySqlClient.MySqlException e)
         {
         }
      }

      private void setStackedChartValues()
      {
         //count of values must be cumulative (can't just say "add one" but have to keep count of how many I already have)
         int chokeCumulative = 0;
         int guardeCumulative = 0;
         int lockCumulative = 0;
         int escapeCumulative = 0;
         int sweepCumulative = 0;

         Dictionary<DateTime, List<int>> dateCounts = new Dictionary<DateTime, List<int>>();
         int sameDate_indexToChange = 0;
         foreach (DataRow techniqueRow in myTechniquesTable.Rows)
         {
            switch (techniqueRow.Field<int>("group_id"))
            {
               case 1:
                  ++chokeCumulative;
                  sameDate_indexToChange = 1;
                  goto default;
               case 2:
                  ++guardeCumulative;
                  sameDate_indexToChange = 2;
                  goto default;
               case 3:
                  ++lockCumulative;
                  sameDate_indexToChange = 3;
                  goto default;
               case 4:
                  ++escapeCumulative;
                  sameDate_indexToChange = 4;
                  goto default;
               case 5:
                  ++sweepCumulative;
                  sameDate_indexToChange = 5;
                  goto default;
               //not adding to charts here because if same date in two dataRows, data duplicate themselves
               //I just create dictionary with dates as keys
               default:
                  //if date already there, I just increase count for particular technique group
                  if (dateCounts.ContainsKey(techniqueRow.Field<DateTime>("date_learned")))
                  {
                     List<int> temp = dateCounts[techniqueRow.Field<DateTime>("date_learned")];
                     temp[sameDate_indexToChange - 1] = temp[sameDate_indexToChange - 1] + 1;
                     dateCounts[techniqueRow.Field<DateTime>("date_learned")] = temp;
                  }
                  else
                  {
                     dateCounts.Add(techniqueRow.Field<DateTime>("date_learned"), new List<int>(new int[] { chokeCumulative, guardeCumulative, lockCumulative, escapeCumulative, sweepCumulative }));
                  }
                  break;
            }
         }

         //here fill the chartvalues for each date
         foreach (KeyValuePair<DateTime, List<int>> pair in dateCounts)
         {
            chokesChartValues_Stacked.Add(new DateTimePoint(pair.Key, pair.Value[0]));
            guardsChartValues_Stacked.Add(new DateTimePoint(pair.Key, pair.Value[1]));
            jointLocksChartValues_Stacked.Add(new DateTimePoint(pair.Key, pair.Value[2]));
            escapesChartValues_Stacked.Add(new DateTimePoint(pair.Key, pair.Value[3]));
            sweepsChartValues_Stacked.Add(new DateTimePoint(pair.Key, pair.Value[4]));
         }
      }

      private int setBarChartValuesAndReturnMaxMonth()
      {
         Dictionary<int, string> groupsNamesDict = this.techniqueGroupsToDictionary();
         int maxMonth = 0;

         foreach (DataRow techniqueRow in myTechniquesTable.Rows)
         {
            //if directory does not have key of techniqueGroup of current techniqueRow, create new entry with groupName and list of 12 zeros as count of the group for each month
            if (!this.groupsPerMonthCounts.ContainsKey(groupsNamesDict[techniqueRow.Field<int>("group_id")]))
            {
               this.groupsPerMonthCounts.Add(groupsNamesDict[techniqueRow.Field<int>("group_id")], new List<int>(new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));
            }

            //get list of 12 values (each month one value)
            List<int> countsInMonth = this.groupsPerMonthCounts[groupsNamesDict[techniqueRow.Field<int>("group_id")]];
            //update value of current month (indexed from zero, that's why -1 is needed)
            countsInMonth[techniqueRow.Field<DateTime>("date_learned").Month - 1] = countsInMonth[techniqueRow.Field<DateTime>("date_learned").Month - 1] + 1;
            this.groupsPerMonthCounts[groupsNamesDict[techniqueRow.Field<int>("group_id")]] = countsInMonth;
            
            //maxmonth part
            if (maxMonth < techniqueRow.Field<DateTime>("date_learned").Month)
               maxMonth = techniqueRow.Field<DateTime>("date_learned").Month;
         }
         return maxMonth;
      }


      private Dictionary<int, string> techniqueGroupsToDictionary() {
         Dictionary<int, string> dict = new Dictionary<int, string>();
         foreach (DataRow techniqueRow in techniqueGroupsTable.Rows)
         {
            dict.Add(techniqueRow.Field<int>("group_id"), techniqueRow.Field<string>("name"));
         }
         return dict;
      }
   }
}