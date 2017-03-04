using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data;
using MySql.Data.MySqlClient;

namespace Jiu_Jitsu_Assistant
{
   /// <summary>
   /// Interaction logic for MyTechniques.xaml
   /// </summary>
   public partial class MyTechniques : Window
   {
      MySqlConnection conn;
      public MyTechniques()
      {
         InitializeComponent();
         if (ConnectToDatabase())
         {
            LoadTechniques();
            LoadedTechniqueGroups();
         }
      }

      public MyTechniques(double left,double top, double height, double width):this() {
         this.Left = left;
         this.Top = top;
         this.Height = height;
         this.Width = width;

      }

      public Boolean ConnectToDatabase()
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

      public void LoadTechniques()
      {
         try
         {
            MySqlCommand cmd;
            cmd = this.conn.CreateCommand();
            cmd.CommandText = "SELECT name,date_learned,belt_level FROM techniques";
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            techniquesGrid.ItemsSource = ds.Tables[0].DefaultView;
         }
         catch (MySql.Data.MySqlClient.MySqlException e)
         {
         }
      }

      public void LoadTechniques(List<string> groups)
      {
         try
         {
            MySqlCommand cmd;
            cmd = this.conn.CreateCommand();
            //list to comma separated string
            string inValues = string.Format("'{0}'", string.Join("','", groups.Select(i => i.Replace("'", "''")).ToArray())); 
            cmd.CommandText = "SELECT name,date_learned,belt_level FROM techniques WHERE group_id IN " +
                              "(SELECT group_id " + 
                              "FROM techniquegroups WHERE name IN (" + inValues + "))";
            //cmd.Parameters.AddWithValue("@Name","inValues" );  //doesnt work...
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            techniquesGrid.ItemsSource = ds.Tables[0].DefaultView;
         }
         catch (MySql.Data.MySqlClient.MySqlException e)
         {
         }
      }

      public void LoadedTechniqueGroups() {
         try
         {
            MySqlCommand cmd;
            cmd = this.conn.CreateCommand();
            cmd.CommandText = "SELECT name FROM techniquegroups";
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            techniqueGroupsGrid.ItemsSource = ds.Tables[0].DefaultView;
         }
         catch (MySql.Data.MySqlClient.MySqlException e)
         {
         }
      }
      

      private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
      {
         if (this.conn.State == ConnectionState.Open)
            this.conn.Close();
      }

      //get chosen cell value and update techniquesGrid with filtered values
      private void techniqueGroupsGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
      {
         try
         {
            List<string> groups = new List<string>();
            foreach (DataGridCellInfo item in techniqueGroupsGrid.SelectedCells)
            {
               var col = item.Column as DataGridColumn;
               var fc = col.GetCellContent(item.Item);
               var chosenGroup = (fc as TextBlock).Text;
               groups.Add(chosenGroup);
            }
            LoadTechniques(groups);
         }
         catch (Exception){ }
      }
   }
}
