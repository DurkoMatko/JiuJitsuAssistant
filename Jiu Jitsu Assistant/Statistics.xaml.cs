using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System.Windows.Media;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Data;
using System.Collections.Generic;
using System.Text;

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

      Dictionary<string, List<int>> groupsPerMonthCounts = new Dictionary<string, List<int>>();
      Dictionary<string, List<int>> groupsPerMonthCounts_ordered = new Dictionary<string, List<int>>();
      public SeriesCollection BarChart { get; set; }
      public string[] Labels_BarChart { get; set; }

      Dictionary<string, ChartValues<DateTimePoint>> stackedChartDictionary = new Dictionary<string, ChartValues<DateTimePoint>>();
      Dictionary<string, ChartValues<DateTimePoint>> stackedChartDictionary_ordered = new Dictionary<string, ChartValues<DateTimePoint>>();
      public SeriesCollection myChronological { get; set; }
      public Func<double, string> myChronological_XFormatter { get; set; }

      //workaround to show alert message on proper place
      double mouse_x { get; set; }
      double mouse_y { get; set; }

      public Statistics()
      {
         InitializeComponent();
         if (ConnectToDatabase())
         {
            LoadTechniques();
            LoadedTechniqueGroups();
         }

         populateBarChart();
         populateStackedChart();

         barchart_container.ChartLegend.Foreground = new SolidColorBrush(Colors.White);
         stacked_container.ChartLegend.Foreground = new SolidColorBrush(Colors.White);
         window.Background = new SolidColorBrush(Colors.Black);
         DataContext = this;
      }

      public Statistics(double left, double top, double height, double width) : this()
      {
         this.Left = left;
         this.Top = top;
         this.Height = height;
         this.Width = width;
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
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT * FROM techniques ");
            sb.Append("WHERE group_id != (Select group_id from techniquegroups where name='Natural Human Movements') ");
            sb.Append("ORDER BY date_learned ASC ");
            MySqlCommand cmd;
            cmd = this.conn.CreateCommand();
            cmd.CommandText = sb.ToString();
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
            cmd.CommandText = "SELECT * FROM techniquegroups where name != 'Natural Human Movements'";
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            this.techniqueGroupsTable = ds.Tables[0];
         }
         catch (MySql.Data.MySqlClient.MySqlException e)
         {
         }
      }

      private int setBarChartValuesAndReturnMaxMonth()
      {
         Dictionary<int, string> groupsNamesDict = this.techniqueGroupsToDictionary();
         int maxMonth = 0;

         //insert all groups to dictionary (even if user can't do any of that group)...like this, colors are the same for both charts
         foreach (KeyValuePair<int, string> groupIdNamePair in groupsNamesDict)
         {
            this.groupsPerMonthCounts.Add(groupIdNamePair.Value, new List<int>(new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));
         }

         foreach (DataRow techniqueRow in myTechniquesTable.Rows)
         {
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

      //used to set mouse_x,mouse_y to proper position AddTechniqueDialog
      private void addTechnique_button_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
      {
         var relativePosition = e.GetPosition(this);
         var point = PointToScreen(relativePosition);
         this.mouse_x = point.X;
         this.mouse_y = point.Y;
      }

      private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
      {
         if (this.conn.State == ConnectionState.Open)
            this.conn.Close();
      }

      private void populateBarChart() {
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
         this.BarChart = new SeriesCollection { };
         List<string> monthsUsed = new List<string>();

         //just reordering the dictionary so the colors match colors in second graph
         var groupsPerMonthCounts_ordered = from pair in this.groupsPerMonthCounts orderby pair.Key ascending select pair;

         //loop dictionary of months and technique counts
         foreach (KeyValuePair<string, List<int>> groupCountsPair in groupsPerMonthCounts_ordered)
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
            this.BarChart.Add(new ColumnSeries
            {
               Title = groupCountsPair.Key,
               Values = tempChartValues
            });
         }
         this.Labels_BarChart = monthsUsed.ToArray();
         
         //adding empty values technique type so it will show itself as empty bar -> causing an illusion of separated months
         this.BarChart.Add(new ColumnSeries { Title = "", Values = new ChartValues<int> { } });
      }


      private void setStackedChartValues()
      {
         Dictionary<int, string> groupsNamesDict = this.techniqueGroupsToDictionary();

         //count of values must be cumulative (can't just say "add one" but have to keep count of how many I already have) - that's how lcharts work
         //so I create an array of -1
         List<int> zeros = Enumerable.Repeat(0, groupsNamesDict.Count+1).ToList();  //list of so many zeros as there are groups
         int[] cumulatives = zeros.ToArray();

         Dictionary<DateTime, List<int>> dateCounts = new Dictionary<DateTime, List<int>>();
         
         foreach (DataRow techniqueRow in myTechniquesTable.Rows)
         {
            //not adding to charts here because if same date in two dataRows, data duplicate themselves
            //I just create dictionary with dates as keys

            //if date is not there just add it with current cumulative values
            if (!dateCounts.ContainsKey(techniqueRow.Field<DateTime>("date_learned")))
               dateCounts.Add(techniqueRow.Field<DateTime>("date_learned"), new List<int>(cumulatives));

            //increase count using group_id as index
            List<int> temp = dateCounts[techniqueRow.Field<DateTime>("date_learned")];
            temp[techniqueRow.Field<int>("group_id")]++;
            cumulatives[techniqueRow.Field<int>("group_id")]++;
            dateCounts[techniqueRow.Field<DateTime>("date_learned")] = temp;
         }


         //here loop all technique groups...keyvaluepair<groupTechnique id, groupTechnique name>
         foreach (KeyValuePair<int, string> group in groupsNamesDict)
         {
            if (!this.stackedChartDictionary.ContainsKey(group.Value))
            {
               this.stackedChartDictionary.Add(group.Value,new ChartValues<DateTimePoint>());
            }
            //here fill the chartvalue for each date....keyvaluepair<date, cumulatives for all techniqueGroups>
            foreach (KeyValuePair<DateTime, List<int>> pair in dateCounts)
            {
               //add to one grouptechnique only
               this.stackedChartDictionary[group.Value].Add(new DateTimePoint(pair.Key, pair.Value[group.Key]));
            }
         }
      }

      private void populateStackedChart()
      {
         this.setStackedChartValues();
         this.myChronological = new SeriesCollection { };

         //just reordering the dictionary so the colors match colors in second graph
         var stackedChartDictionary_ordered = from pair in this.stackedChartDictionary orderby pair.Key ascending select pair;

         //keyvaluepair<groupTechnique name, values>
         foreach (KeyValuePair<string, ChartValues<DateTimePoint>> groupCounts in stackedChartDictionary_ordered)
         {
            myChronological.Add(new StackedAreaSeries
            {
               Title = groupCounts.Key,
               Values = groupCounts.Value,
               LineSmoothness = 0
            });
         }
         this.myChronological_XFormatter = val => new DateTime((long)val).ToString("dd/MM/yyyy");
      }
   }
}