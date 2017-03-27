using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Jiu_Jitsu_Assistant
{
   /// <summary>
   /// Interaction logic for MindMaps.xaml
   /// </summary>
   public partial class MindMaps : Window
   {
      MySqlConnection conn;
      private DataTable myTechniquesTable { get; set; }
      Dictionary<int, string> techniquesDict = new Dictionary<int, string>();
      private DataTable setupsTable { get; set; }
      Dictionary<int, List<string>> setupsDict = new Dictionary<int, List<string>>();

      private string currentPosition { get; set; }
      private string lastTechnique { get; set; }

      double difficultyTreshold { get; set; }
      int sequenceCounter { get; set; }
      bool nogi_flag { get; set; }


      public MindMaps()
      {
         InitializeComponent();

         if (ConnectToDatabase())
         {
            LoadTechniques();
            LoadSetups();
            setDictionaries();
         }
      }

      private void CreateButtons()
      {
         List<int> availableTechniques = getAvailableTechniques(currentPosition, lastTechnique);
         int availableTechniquesCount = availableTechniques.Count;

         //if there are some buttons to be created (if there are some techniques known from current position)
         if (availableTechniquesCount != 0)
         {
            int arenaWidth = 900;
            int arenaHeight = 800;
            double oneButtonSpace = (arenaHeight * arenaWidth) / availableTechniquesCount;

            foreach (DataRow techniqueRow in myTechniquesTable.Rows)
            {
               if (availableTechniques.Contains(techniqueRow.Field<int>("technique_id"))) { 
                  string resourceBrushKey = "techniqueGroupGrad_" + techniqueRow.Field<int>("group_id").ToString() + "_disabled";

                  Button button = new Button()
                  {
                     //this is kind of "hackerei"...using unrelated button properties for my purpose...in the future define my own button class should be optimal

                     //Content = techniqueRow.Field<string>("name").ToString(),
                     Content = new TextBlock { Text = techniqueRow.Field<string>("name").ToString(), Foreground = new SolidColorBrush(Colors.Gray), TextWrapping = TextWrapping.Wrap },
                     ToolTip = techniqueRow.Field<string>("name").ToString() + " -> " + techniqueRow.Field<string>("position_to").ToString(),
                     Tag = techniqueRow.Field<int>("technique_id").ToString(),
                     Uid = techniqueRow.Field<string>("position_to").ToString(),
                     Name = "_" + techniqueRow.Field<int>("group_id").ToString(),
                     //resource defined in app.xaml
                     Style = (Style)FindResource("MindMapsButton_disabled"),
                     IsEnabled = false,
                     Background = (System.Windows.Media.Brush)FindResource(resourceBrushKey),
                     Width = (int)Math.Sqrt(oneButtonSpace * 0.35),
                     Height = (int)Math.Sqrt(oneButtonSpace * 0.35)
                  };

                  ToolTipService.SetShowOnDisabled(button, true);
                  button.Click += new RoutedEventHandler(techniqueClicked);
                  button.PreviewMouseDown += new MouseButtonEventHandler(mouseDownOnTechnique);
                  this.buttonsGrid.Children.Add(button);
               }
            }
         }
      }


      void techniqueClicked(object sender, RoutedEventArgs e)
      {
         setup_textBox.Clear();
         currentPosition = (sender as Button).Uid.ToString();
         var buttonTextBlock = (sender as Button).Content as TextBlock;
         lastTechnique = buttonTextBlock.Text;

         currentPositionLabel.Content = currentPosition;
         lastTechniqueLabel.Text = lastTechnique;

         this.buttonsGrid.Children.Clear();
         CreateButtons();

         if (currentPosition == "Submission")
            newRoundButton.IsEnabled = true;

         if (string.IsNullOrEmpty(sequence_textblock.Text))
            sequence_textblock.Text = sequence_textblock.Text + sequenceCounter++ + ". " + lastTechnique + "    [" + currentPosition + "]";
         else
            sequence_textblock.Text = sequence_textblock.Text + "\n" + sequenceCounter++ + ". " + lastTechnique + "   [" + currentPosition + "]";
      }

      void mouseDownOnTechnique(object sender, MouseButtonEventArgs args)
      {
         var techButton = sender as Button;
         techButton.Background = (System.Windows.Media.Brush)FindResource("clickedTechnique");
      }


      public MindMaps(double left, double top, double height, double width):this() {
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
            sb.Append("SELECT t.technique_id, t.group_id, t.name, t.date_learned, t.belt_level, pFrom.name as position_from, pTo.name as position_to, t.nogi_flag FROM techniques as t ");
            sb.Append("LEFT JOIN positions as pFrom ");
            sb.Append("ON pFrom.position_id = t.position_from ");
            sb.Append("LEFT JOIN positions as pTo ");
            sb.Append("ON pTo.position_id = t.position_to ");
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

      private void LoadSetups()
      {
         try
         {
            MySqlCommand cmd;
            cmd = this.conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM setups";
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);

            this.setupsTable = ds.Tables[0];
         }
         catch (MySql.Data.MySqlClient.MySqlException e)
         {
         }
      }

      private void setDictionaries()
      {
         foreach (DataRow techniqueRow in myTechniquesTable.Rows)
         {
            techniquesDict.Add(techniqueRow.Field<int>("technique_id"), techniqueRow.Field<string>("name"));
         }

         foreach (DataRow setupRow in setupsTable.Rows)
         {
            if (setupsDict.ContainsKey(setupRow.Field<int>("technique_id")))
               setupsDict[setupRow.Field<int>("technique_id")].Add(setupRow.Field<string>("description"));
            else { //if new technique, create new list and dict entry
               List<string> newSetupList = new List<string>();
               newSetupList.Add(setupRow.Field<string>("description"));
               setupsDict.Add(setupRow.Field<int>("technique_id"), newSetupList);
            }
         }
      } 

      private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
      {
         if (this.conn.State == ConnectionState.Open)
            this.conn.Close();
      }

      private void enableTechniques(object sender, TextChangedEventArgs e)
      {
         try
         {
            // check each button if should be enabled
            foreach (Button button in this.buttonsGrid.Children)
            {
               string resourceBrushKey = "techniqueGroupGrad" + button.Name;
               //check all possible setups
               foreach (string setup in setupsDict[int.Parse(button.Tag.ToString())])
               {
                  int toChange = LevenshteinDistance.Compute(setup_textBox.Text, setup);
                  int matchedLetters = setup.Length - toChange;
                  //if matched letters more that required threshold, enable button
                  if (matchedLetters > setup.Length * difficultyTreshold)
                  {
                     button.IsEnabled = true;
                     button.Style = (Style)FindResource("MindMapsButton_enabled");
                     button.Background = (System.Windows.Media.Brush)FindResource(resourceBrushKey);
                     var textBlock = button.Content as TextBlock;
                     textBlock.Foreground = new SolidColorBrush(Colors.Black);
                     //if matching one setup, immediately break - it's enough to allow technique
                     break;
                  }
                  else
                  {
                     button.IsEnabled = false;
                     button.Style = (Style)FindResource("MindMapsButton_disabled");
                     button.Background = (System.Windows.Media.Brush)FindResource(resourceBrushKey + "_disabled");
                     var textBlock = button.Content as TextBlock;
                     textBlock.Foreground = new SolidColorBrush(Colors.Gray);
                  }

               }
            }
         }
         catch (Exception ex) { }
      }

      private void newRoundClicked(object sender, RoutedEventArgs e)
      {
         sequenceCounter = 1;
         sequence_textblock.Text="";

         currentPosition = "Both standing";
         currentPositionLabel.Content = currentPosition;
         lastTechniqueLabel.Text = "none";
         lastTechnique = "none";
         var selectedDifficulty = difficultyCombobox.SelectedItem as ComboBoxItem;
         difficultyTreshold = Double.Parse(selectedDifficulty.Tag.ToString());
         difficultyCombobox.IsEnabled = false;
         newRoundButton.IsEnabled = false;
         CreateButtons();
      }

      private void resetClicked(object sender, RoutedEventArgs e) {
         sequence_textblock.Text = "";

         buttonsGrid.Children.Clear();
         difficultyCombobox.IsEnabled = true;
         newRoundButton.IsEnabled = true;
         currentPositionLabel.Content = "none";
         lastTechniqueLabel.Text = "none";
      }

      private List<int> getAvailableTechniques(string curr_pos, string last_tech)
      {
         List<int> availableTechniqueIds = new List<int>();
         foreach (DataRow techniqueRow in myTechniquesTable.Rows)
         {
            //starting in current position and can't repeat the technique
            if (techniqueRow.Field<string>("position_from") == curr_pos &&
                  techniqueRow.Field<string>("name") != last_tech)
            {
               //if no_gi gameplay, check if technique is nogi allowed
               if (nogi_flag)
               {
                  if (techniqueRow.Field<bool>("nogi_flag") == true)
                     availableTechniqueIds.Add((techniqueRow.Field<int>("technique_id")));
               }
               else
                  availableTechniqueIds.Add((techniqueRow.Field<int>("technique_id")));
            }
         }
         return availableTechniqueIds;
      }

      private void gi_radioButton_Checked(object sender, RoutedEventArgs e)
      {
         nogi_flag = false;
      }

      private void nogi_radioButton_Checked(object sender, RoutedEventArgs e)
      {
         nogi_flag = true;
      }


      private void newRoundButton_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
      {
         var button = sender as Button;
         if (button.IsEnabled)
            button.Style = (Style)FindResource("StyleButton");
         else
            button.Style = (Style)FindResource("StyleButton_Disabled");
      }
   }
}
