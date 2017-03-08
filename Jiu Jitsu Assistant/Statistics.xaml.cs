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

      public SeriesCollection techniquesChronological { get; set; }
      public Func<double, string> techniquesChronological_XFormatter { get; set; }
      public Func<double, string> techniquesChronological_YFormatter { get; set; }

      public SeriesCollection myChronological { get; set; }
      public Func<double, string> myChronological_XFormatter { get; set; }
      public Func<double, string> myChronological_YFormatter { get; set; }

      ChartValues<DateTimePoint> chokesChartValues = new ChartValues<DateTimePoint>();
      ChartValues<DateTimePoint> guardsChartValues = new ChartValues<DateTimePoint>();
      ChartValues<DateTimePoint> jointLocksChartValues = new ChartValues<DateTimePoint>();
      ChartValues<DateTimePoint> escapesChartValues = new ChartValues<DateTimePoint>();
      ChartValues<DateTimePoint> sweepsChartValues = new ChartValues<DateTimePoint>();

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


         List<DateTimePoint> parts = new List<DateTimePoint>();

         techniquesChronological = new SeriesCollection
            {
                new StackedAreaSeries
                {
                    Title = "Africa",
                    Values = new ChartValues<DateTimePoint>
                    {
                        new DateTimePoint(new DateTime(1950, 1, 1), 1),
                        new DateTimePoint(new DateTime(1960, 1, 1), 1),
                        new DateTimePoint(new DateTime(1970, 1, 1), 1),
                        new DateTimePoint(new DateTime(1980, 1, 1), 1),
                        new DateTimePoint(new DateTime(1990, 1, 1), 1),
                        new DateTimePoint(new DateTime(2000, 1, 1), 1),
                        new DateTimePoint(new DateTime(2010, 1, 1), 1),
                        new DateTimePoint(new DateTime(2013, 1, 1), 1)
                    },
                    LineSmoothness = 0
                },
                new StackedAreaSeries
                {
                    Title = "N & S America",
                    Values = new ChartValues<DateTimePoint>
                    {
                        new DateTimePoint(new DateTime(1950, 1, 1), 1),
                        new DateTimePoint(new DateTime(1960, 1, 1), 1),
                        new DateTimePoint(new DateTime(1970, 1, 1), 1),
                        new DateTimePoint(new DateTime(1980, 1, 1), 1),
                        new DateTimePoint(new DateTime(1990, 1, 1), 1),
                        new DateTimePoint(new DateTime(2000, 1, 1), 1),
                        new DateTimePoint(new DateTime(2010, 1, 1), 1),
                        new DateTimePoint(new DateTime(2013, 1, 1), 1)
                    },
                    LineSmoothness = 0
                },
                new StackedAreaSeries
                {
                    Title = "Asia",
                    Values = new ChartValues<DateTimePoint>
                    {
                        new DateTimePoint(new DateTime(1950, 1, 1), 1),
                        new DateTimePoint(new DateTime(1960, 1, 1), 1),
                        new DateTimePoint(new DateTime(1970, 1, 1), 1),
                        new DateTimePoint(new DateTime(1980, 1, 1), 1),
                        new DateTimePoint(new DateTime(1990, 1, 1), 1),
                        new DateTimePoint(new DateTime(2000, 1, 1), 1),
                        new DateTimePoint(new DateTime(2010, 1, 1), 1),
                        new DateTimePoint(new DateTime(2013, 1, 1), 1)
                    },
                    LineSmoothness = 0
                },
                new StackedAreaSeries
                {
                    Title = "Europe",
                    Values = new ChartValues<DateTimePoint>
                    {
                        new DateTimePoint(new DateTime(1950, 1, 1), 1),
                        new DateTimePoint(new DateTime(1960, 1, 1), 1),
                        new DateTimePoint(new DateTime(1970, 1, 1), 1),
                        new DateTimePoint(new DateTime(1980, 1, 1), 1),
                        new DateTimePoint(new DateTime(1990, 1, 1), 1),
                        new DateTimePoint(new DateTime(2000, 1, 1), 1),
                        new DateTimePoint(new DateTime(2010, 1, 1), 1),
                        new DateTimePoint(new DateTime(2013, 1, 1), 3)
                    },
                    LineSmoothness = 0
                }
            };

         techniquesChronological_XFormatter = val => new DateTime((long)val).ToString("yyyy");
         techniquesChronological_YFormatter = val => val.ToString("N") + " M";

         DataContext = this;

         this.setChartValues();

         myChronological = new SeriesCollection
            {
                new StackedAreaSeries
                {
                    Title = "Chokes",
                    Values = chokesChartValues,
                    LineSmoothness = 0
                },
                new StackedAreaSeries
                {
                    Title = "Guards",
                    Values = guardsChartValues,
                    LineSmoothness = 0
                },
                new StackedAreaSeries
                {
                    Title = "Joint locks",
                    Values = jointLocksChartValues,
                    LineSmoothness = 0
                },
                new StackedAreaSeries
                {
                    Title = "Escapes",
                    Values = escapesChartValues,
                    LineSmoothness = 0
                },
                new StackedAreaSeries
                {
                    Title = "Sweeps",
                    Values = sweepsChartValues,
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

      private void setChartValues()
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
            chokesChartValues.Add(new DateTimePoint(pair.Key, pair.Value[0]));
            guardsChartValues.Add(new DateTimePoint(pair.Key, pair.Value[1]));
            jointLocksChartValues.Add(new DateTimePoint(pair.Key, pair.Value[2]));
            escapesChartValues.Add(new DateTimePoint(pair.Key, pair.Value[3]));
            sweepsChartValues.Add(new DateTimePoint(pair.Key, pair.Value[4]));
         }
      }
   }
}