using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using MySql.Data.MySqlClient;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;

namespace Jiu_Jitsu_Assistant
{
   /// <summary>
   /// Interaction logic for MyTechniques.xaml
   /// </summary>
   public partial class MyTechniques : Window
   {

      MySqlConnection conn;
      private DataTable myTechniquesTable { get; set; }
      private DataTable techniqueGroupsTable { get; set; }
      private DataTable positionsTable { get; set; }
      private Dictionary<string,int> positionsDict = new Dictionary<string, int>();

      private double mouse_x { get; set; }
      private double mouse_y { get; set; }

      public MyTechniques()
      {
         InitializeComponent();
         if (ConnectToDatabase())
         {
            LoadTechniques();
            LoadedTechniqueGroups();
            LoadPositions();

            window1.Background = new SolidColorBrush(Colors.Black);
         }
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
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT t.technique_id, t.name, t.date_learned, t.belt_level, pFrom.name as position_from, pTo.name as position_to FROM techniques as t ");
            sb.Append("LEFT JOIN positions as pFrom ");
            sb.Append("ON pFrom.position_id = t.position_from ");
            sb.Append("LEFT JOIN positions as pTo ");
            sb.Append("ON pTo.position_id = t.position_to ");
            sb.Append("WHERE t.group_id != (Select group_id from techniquegroups where name='Natural Human Movements')");
            MySqlCommand cmd;
            cmd = this.conn.CreateCommand();
            cmd.CommandText = sb.ToString();
                        
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
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT t.technique_id, t.name, t.date_learned, t.belt_level, pFrom.name as position_from, pTo.name as position_to FROM techniques as t ");
            sb.Append("LEFT JOIN positions as pFrom ");
            sb.Append("ON pFrom.position_id = t.position_from ");
            sb.Append("LEFT JOIN positions as pTo ");
            sb.Append("ON pTo.position_id = t.position_to ");
            sb.AppendFormat("WHERE t.group_id IN (SELECT group_id FROM techniquegroups WHERE name IN  ({0}))", inValues);
            cmd.CommandText = sb.ToString();
               
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
            cmd.CommandText = "SELECT * FROM techniquegroups where name !='Natural Human Movements'";
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            this.techniqueGroupsTable = ds.Tables[0];
            techniqueGroupsGrid.ItemsSource = this.techniqueGroupsTable.DefaultView;
            //fill technique groups combobox in add new techniquq form
            techniqueGroup_comboBox.ItemsSource = this.techniqueGroupsTable.DefaultView;
            techniqueGroup_comboBox.DisplayMemberPath = "name";
            techniqueGroup_comboBox.SelectedValuePath = "group_id";
            //techniqueGroup_comboBox.SelectedIndex = 0;
         }
         catch (MySql.Data.MySqlClient.MySqlException e)
         {
         }
      }

      private void LoadPositions() {
         try
         {
            MySqlCommand cmd;
            cmd = this.conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM positions";
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            this.positionsTable = ds.Tables[0];
            //comboboxes
            from_Position_comboBox.ItemsSource = this.positionsTable.DefaultView;
            from_Position_comboBox.DisplayMemberPath = "name";
            from_Position_comboBox.SelectedValuePath = "position_id";

            to_Position_comboBox.ItemsSource = this.positionsTable.DefaultView;
            to_Position_comboBox.DisplayMemberPath = "name";
            to_Position_comboBox.SelectedValuePath = "position_id";

            foreach (DataRow positionRow in positionsTable.Rows) {
               positionsDict.Add(positionRow.Field<string>("name"), positionRow.Field<int>("position_id"));
            }
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

      private void AddNewTechnique(object sender, RoutedEventArgs e)
      {
         try{
            StringBuilder sb = new StringBuilder();
            sb.Append("SET foreign_key_checks = 0;");
            sb.AppendFormat("INSERT INTO techniques (group_id,name,date_learned,belt_level,position_from,position_to) VALUES ({0},'{1}','{2}','{3}', {4} , {5} )", techniqueGroup_comboBox.SelectedValue.ToString(), techniqueName_textbox.Text, dateLearned_datepicker.SelectedDate.Value.Date.ToString("yyyy-MM-dd"), ((ComboBoxItem)belt_comboBox.SelectedItem).Name, from_Position_comboBox.SelectedValue.ToString(), to_Position_comboBox.SelectedValue.ToString());
            MySqlCommand cmd;
            cmd = this.conn.CreateCommand();
            cmd.CommandText = sb.ToString();
            int effectedRows = cmd.ExecuteNonQuery();

            if (effectedRows != 0) {
               LoadTechniques();
               disableAddTechniqueControls();
            }

            //javascript like alert dialog to let user know if adding technique was successful
            AddTechniqueDialog atd = new AddTechniqueDialog(true, mouse_x, mouse_y, "Technique");
            atd.Show();

            string createText = sb.ToString() + Environment.NewLine;
            File.AppendAllText("C:/Users/T420/Documents/Visual Studio 2015/Projects/Jiu Jitsu Assistant/db_creation/data2.txt", createText);
         }
         catch (Exception ex){
            resetAddTechniqueValues();
            AddTechniqueDialog atd = new AddTechniqueDialog(false, mouse_x, mouse_y, "technique");
            atd.Show();
         }
      }

      private void addSetup(object sender, RoutedEventArgs e)
      {
         try
         {
            StringBuilder sb = new StringBuilder();
            sb.Append("SET foreign_key_checks = 0;");
            sb.Append("INSERT INTO setups (technique_id, description) VALUES (");
            sb.AppendFormat("(SELECT technique_id from techniques where name='{0}'), ", techniqueName_textbox.Text);
            sb.AppendFormat("'{0}')", setup_textBox.Text);
            MySqlCommand cmd;
            cmd = this.conn.CreateCommand();
            cmd.CommandText = sb.ToString();

            if (cmd.ExecuteNonQuery() == 1)
            {
               setup_textBox.Clear();
               AddTechniqueDialog atd = new AddTechniqueDialog(true, mouse_x, mouse_y,"Setup");
               atd.Show();
            }

            string createText = sb.ToString() + Environment.NewLine;
            File.AppendAllText("C:/Users/T420/Documents/Visual Studio 2015/Projects/Jiu Jitsu Assistant/db_creation/data2.txt", createText);
         }
         catch (Exception ex)
         {
            AddTechniqueDialog atd = new AddTechniqueDialog(false, mouse_x, mouse_y, "setup");
            atd.Show();
         }
      }

      private void finishAddingTechniqueAndSetups(object sender, RoutedEventArgs e)
      {
         resetAddTechniqueValues();
         enableAddtechniqueForm(true);
         //disable setup part
         setup_textBox.Clear();
         setup_textBox.IsEnabled = false;
         add_setup_button.IsEnabled = false;

      }

      //reset new technique form
      private void resetAddTechniqueValues()
      {
         techniqueName_textbox.Text = "";
         dateLearned_datepicker.SelectedDate = DateTime.Today;
         belt_comboBox.SelectedIndex = 0;
         techniqueGroup_comboBox.SelectedIndex = 0;
         from_Position_comboBox.SelectedIndex = 0;
         to_Position_comboBox.SelectedIndex = to_Position_comboBox.Items.Count - 1;
      }

      private void disableAddTechniqueControls() {
         enableAddtechniqueForm(false);

         setup_textBox.IsEnabled = true;
         add_setup_button.IsEnabled = true;
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
         finish_technique_button.Content = "Finish " + techniqueName_textbox.Text;
         if (string.IsNullOrWhiteSpace(techniqueName_textbox.Text)) {
            addTechnique_button.IsEnabled = false;
            return;
         }
         addTechnique_button.IsEnabled = true;
      }

      private void enableAddtechniqueForm(Boolean flag) {
         techniqueName_textbox.IsEnabled = flag;
         dateLearned_datepicker.IsEnabled = flag;
         belt_comboBox.IsEnabled = flag;
         techniqueGroup_comboBox.IsEnabled = flag;
         from_Position_comboBox.IsEnabled = flag;
         to_Position_comboBox.IsEnabled = flag;
         addTechnique_button.IsEnabled = flag;
      }

      private void mainGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
      {
         Keyboard.ClearFocus();
         mainGrid.Focus();
      }

      private void deletingRowFromDatagrid(object sender, KeyEventArgs e)
      {
         try
         {
            //if it's really delete event and have something selected
            if (e.Key == Key.Delete && techniquesGrid.SelectedItems.Count > 0)
            {
               StringBuilder sb = new StringBuilder();
               List<string> columnValues = new List<string>();
               int rowIndex = techniquesGrid.SelectedIndex;
               DataGridRow row = (DataGridRow)techniquesGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex);

               foreach (DataGridColumn column in techniquesGrid.Columns)
               {
                  if (column.GetCellContent(row) is TextBlock)
                  {
                     TextBlock cellContent = column.GetCellContent(row) as TextBlock;
                     columnValues.Add(cellContent.Text);
                  }
                  else if (column.GetCellContent(row) is TextBox)
                  {
                     return;   //if there's a textbox within a row, it means user is editing something...therefore terminate the function
                  }
               }
               sb.Clear();
               sb.AppendFormat("delete from setups where technique_id = {0};", columnValues[5]);
               sb.AppendFormat("delete from techniques where technique_id = {0}", columnValues[5]);  //last column with index 5 is hidden technique_id column

               MySqlCommand cmd;
               cmd = this.conn.CreateCommand();
               cmd.CommandText = sb.ToString();
               int effectedRows = cmd.ExecuteNonQuery();

               LoadTechniques();
            }
         }
         catch (Exception ex)
         {
         }
      }

      //propagates changes done in datagrid to the database
      private void updateTechniqueFromDataGrid(object sender, DataGridCellEditEndingEventArgs e)
      {
         try
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

               //if names for positions exist
               if (positionsDict.ContainsKey(gridValues[2]) && positionsDict.ContainsKey(gridValues[3]))
               {
                  StringBuilder sb = new StringBuilder();
                  sb.AppendFormat("update techniques set name='{0}',date_learned='{1}', ", gridValues[0], DateTime.Parse(gridValues[1]).ToString("yyyy-MM-dd"));
                  sb.AppendFormat(" position_from={0}, ", positionsDict[gridValues[2]]);
                  sb.AppendFormat(" position_to={0}, ", positionsDict[gridValues[3]]);
                  sb.AppendFormat(" belt_level='{0}' where technique_id = {1}", gridValues[4], gridValues[5]);
                  MySqlCommand cmd;
                  cmd = this.conn.CreateCommand();
                  cmd.CommandText = sb.ToString();
                  int effectedRows = cmd.ExecuteNonQuery();
               }
               LoadTechniques();
            }
         }
         catch (Exception ex)
         {
         }
      }

      private void lookupSetupsForTechnique(object sender, SelectionChangedEventArgs e)
      {
         //clear setups from previous technique
         setups_textblock.Text = String.Empty;

         DataGridRow row = (DataGridRow)techniquesGrid.ItemContainerGenerator.ContainerFromIndex(techniquesGrid.SelectedIndex);
         TextBlock idCell = null;
         foreach (DataGridColumn column in techniquesGrid.Columns)
         {
            if (column.Header.ToString() == "Id")
            {
               idCell = column.GetCellContent(row) as TextBlock;
               break;
            }
         }

         StringBuilder sb = new StringBuilder();
         sb.AppendFormat("Select description from setups where technique_id = {0};", idCell.Text);
         MySqlCommand cmd;
         cmd = this.conn.CreateCommand();
         cmd.CommandText = sb.ToString();
         MySqlDataReader dr = cmd.ExecuteReader();
         int setupsCount = 1;
         while (dr.Read())
         {
            if (string.IsNullOrEmpty(setups_textblock.Text))
               setups_textblock.Text = setupsCount++.ToString() + ". " + dr[0].ToString();
            else
               setups_textblock.Text = setups_textblock.Text + "\n" + setupsCount++.ToString() + ". " + dr[0].ToString();
         }
         dr.Close();
      }
   }
   }

