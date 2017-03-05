using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using MySql.Data.MySqlClient;
using System.Windows.Input;

namespace Jiu_Jitsu_Assistant
{
   /// <summary>
   /// Interaction logic for MyTechniques.xaml
   /// </summary>
   public partial class MyTechniques : Window
   {

      MySqlConnection conn;
      public DataTable myTechniquesTable { get; set; }
      public DataTable techniqueGroupsTable { get; set; }
      double mouse_x { get; set; }
      double mouse_y { get; set; }

      public MyTechniques()
      {
         InitializeComponent();
         if (ConnectToDatabase())
         {
            LoadTechniques();
            LoadedTechniqueGroups();
         }

         this.myTechniquesTable = new DataTable();
         this.techniqueGroupsTable = new DataTable();
      }

      public MyTechniques(double left,double top, double height, double width):this() {
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
            MySqlCommand cmd;
            cmd = this.conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM techniques";
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            this.myTechniquesTable = ds.Tables[0];
            techniquesGrid.ItemsSource = this.myTechniquesTable.DefaultView;
         }
         catch (MySql.Data.MySqlClient.MySqlException e)
         {
         }
      }


      private void LoadTechniques(List<string> groups)
      {
         try
         {
            MySqlCommand cmd;
            cmd = this.conn.CreateCommand();
            //list to comma separated string
            string inValues = string.Format("'{0}'", string.Join("','", groups.Select(i => i.Replace("'", "''")).ToArray()));
            cmd.CommandText = "SELECT * FROM techniques WHERE group_id IN " +
                              "(SELECT group_id " +
                              "FROM techniquegroups WHERE name IN (" + inValues + "))";
            //cmd.Parameters.AddWithValue("@Name","inValues" );  //doesnt work...
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            this.myTechniquesTable = ds.Tables[0];
            techniquesGrid.ItemsSource = this.myTechniquesTable.DefaultView;
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
            techniqueGroupsGrid.ItemsSource = this.techniqueGroupsTable.DefaultView;
            //fill technique groups combobox in add new techniquq form
            techniqueGroup_comboBox.ItemsSource = this.techniqueGroupsTable.DefaultView;
            techniqueGroup_comboBox.DisplayMemberPath = "name";
            techniqueGroup_comboBox.SelectedValuePath = "group_id";
            techniqueGroup_comboBox.SelectedIndex = 0;
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

      private void cbi_brown_Selected(object sender, RoutedEventArgs e)
      {

      }

      private void AddNewTechnique(object sender, RoutedEventArgs e)
      {
         try{
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("INSERT INTO techniques (group_id,name,date_learned,belt_level) VALUES ({0},'{1}','{2}','{3}')", techniqueGroup_comboBox.SelectedValue.ToString(), techniqueName_textbox.Text, DateTime.Parse(dateLearned_datepicker.SelectedDate.Value.Date.ToShortDateString()).ToString("yyyy-MM-dd"), ((ComboBoxItem)belt_comboBox.SelectedItem).Content.ToString());
            MySqlCommand cmd;
            cmd = this.conn.CreateCommand();
            cmd.CommandText = sb.ToString();
            int effectedRows = cmd.ExecuteNonQuery();

            resetAddTechniqueValues();
            bool success = false;
            if (effectedRows != 0) {
               LoadTechniques();
               success = true;
            }

            //javascript like alert dialog to let user know if adding technique was successful
            AddTechniqueDialog atd = new AddTechniqueDialog(success, mouse_x, mouse_y);
            atd.Show();
         }
         catch (Exception ex){
            resetAddTechniqueValues();
            AddTechniqueDialog atd = new AddTechniqueDialog(false, mouse_x, mouse_y);
            atd.Show();
         }
      }

      //reset new technique form
      private void resetAddTechniqueValues()
      {
         techniqueName_textbox.Text = "";
         dateLearned_datepicker.SelectedDate = DateTime.Today;
         belt_comboBox.SelectedIndex = 0;
         techniqueGroup_comboBox.SelectedIndex = 0;
      }


      //used to set mouse_x,mouse_y to proper position AddTechniqueDialog
      private void addTechnique_button_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
      {
         var relativePosition = e.GetPosition(this);
         var point = PointToScreen(relativePosition);
         this.mouse_x = point.X;
         this.mouse_y = point.Y;
      }

      //enables/disables Add button always when technique name textbox changes value
      private void emptyTechniqueNameCheck(object sender, TextChangedEventArgs e)
      {
         if (string.IsNullOrWhiteSpace(techniqueName_textbox.Text)) {
            addTechnique_button.IsEnabled = false;
            return;
         }
         addTechnique_button.IsEnabled = true;
      }
      private void mainGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
      {
         Keyboard.ClearFocus();
         mainGrid.Focus();
      }

      //propagates changes done in datagrid to the database
      private void updateTechniqueFromDataGrid(object sender, DataGridCellEditEndingEventArgs e)
      {
         if (e.EditAction == DataGridEditAction.Commit)
         {
            int rowIndex = e.Row.GetIndex();
            DataGridRow row = (DataGridRow)techniquesGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex);

            //loop over cells of selected row and get their values
            List<string> gridValues = new List<string>();
            foreach (DataGridColumn column in techniquesGrid.Columns)
            {
               if (column.GetCellContent(row) is TextBlock)
               {
                  TextBlock cellContent = column.GetCellContent(row) as TextBlock;
                  gridValues.Add(cellContent.Text);
               }
               else if (column.GetCellContent(row) is TextBox)
               {
                  TextBox cellContent = column.GetCellContent(row) as TextBox;
                  gridValues.Add(cellContent.Text);
               }
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("update techniques set name='{0}',date_learned='{1}',belt_level='{2}' where technique_id = {3}",gridValues[0], gridValues[1], gridValues[2], gridValues[3]);

            MySqlCommand cmd;
            cmd = this.conn.CreateCommand();
            cmd.CommandText = sb.ToString();
            int effectedRows = cmd.ExecuteNonQuery();

            LoadTechniques();
         }
      }
   }
}
