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
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Jiu_Jitsu_Assistant
{
   /// <summary>
   /// Interaction logic for MindMaps.xaml
   /// </summary>
   public partial class Roll : Window
   {
      MySqlConnection conn;
      private DataTable myTechniquesTable { get; set; }
      Dictionary<int, string> techniquesDict = new Dictionary<int, string>();
      private DataTable setupsTable { get; set; }
      Dictionary<int, List<string>> setupsDict = new Dictionary<int, List<string>>();
      Dictionary<string, string> positionPairsDict = new Dictionary<string, string>();

      string _currentPosition;
      public string currentPosition
      {
         get { return _currentPosition; }
         set
         {
            _currentPosition = value;
            if (_currentPosition == "Submission")
            {
               submissionHappened();
            }
         }
      }
      private string lastTechnique { get; set; }


      string _opponent_currentPosition;
      public string opponent_currentPosition
      {
         get { return _opponent_currentPosition; }
         set
         {
            _opponent_currentPosition = value;
            if (_opponent_currentPosition == "Submission")
            {
               submissionHappened();
            }
         }
      }
      private string opponent_lastTechnique { get; set; }

      double difficultyTreshold { get; set; }
      bool nogi_flag { get; set; }

      DispatcherTimer fightTimer = new DispatcherTimer();
      DispatcherTimer opponentMoveTimer = new DispatcherTimer();

      public Roll()
      {
         InitializeComponent();
         fightTimer.Interval = TimeSpan.FromSeconds(1);
         fightTimer.Tick += dispatcherTimer_Tick;

         if (ConnectToDatabase())
         {
            LoadTechniques();
            LoadSetups();
            LoadPositionPairs();
            setDictionaries();
         }
      }

      private void CreateButtons()
      {
         buttonsGrid.Children.Clear();

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
         lastTechniqueLabel.Content = lastTechnique;
         opponent_currentPosition = positionPairsDict[currentPosition];
         opponentCurrentPositionLabel.Content = opponent_currentPosition;
         opponentLastTechniqueLabel.Content = "-";

         //restart opponent thinking time
         opponentMoveTimer.Stop();
         opponentMoveTimer.Start();

         this.buttonsGrid.Children.Clear();
         CreateButtons();

         if (currentPosition == "Submission")
            newRoundButton.IsEnabled = true;
      }

      void mouseDownOnTechnique(object sender, MouseButtonEventArgs args) {
         var techButton = sender as Button;
         techButton.Background = (System.Windows.Media.Brush)FindResource("clickedTechnique");
      }

      public Roll(double left, double top, double height, double width) : this()
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

      private void LoadPositionPairs()
      {
         StringBuilder sb = new StringBuilder(); 
         sb.Append("select p.name as my_position, p2.name as opponent_position from positionpairs as pp ");
         sb.Append("left join positions as p ");
         sb.Append("on pp.my_position_id = p.position_id ");
         sb.Append("left join positions as p2 ");
         sb.Append("on pp.opponent_position_id = p2.position_id");

         MySqlCommand cmd;
         cmd = this.conn.CreateCommand();
         cmd.CommandText = sb.ToString();
         MySqlDataAdapter da = new MySqlDataAdapter(cmd);
         DataSet ds = new DataSet();
         da.Fill(ds);

         foreach (DataRow pairRow in ds.Tables[0].Rows)
         {
            positionPairsDict.Add(pairRow.Field<string>("my_position"), pairRow.Field<string>("opponent_position"));
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
            else
            { //if new technique, create new list and dict entry
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
                  //needed to disable buttons again if user decides to go for another technique
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
         //reset player
         currentPosition = "Both standing";
         currentPositionLabel.Content = currentPosition;
         lastTechniqueLabel.Content = "-";
         lastTechnique = "-";

         //reset opponent
         opponent_currentPosition = "Both standing";
         opponentCurrentPositionLabel.Content = opponent_currentPosition;
         opponentLastTechniqueLabel.Content = "-"; 
         opponent_lastTechnique = "-";

         //set timer label
         setTimerLabel(selectedTime_textbox.Text);

         //settings controls
         var selectedDifficulty = difficultyCombobox.SelectedItem as ComboBoxItem;
         difficultyTreshold = Double.Parse(selectedDifficulty.Tag.ToString());
            
         //start timer
         fightTimer.Start();

         //set opponent timer
         opponentMoveTimer.Interval = TimeSpan.FromSeconds(getOpponentsSetupTime());
         opponentMoveTimer.Tick += opponentMove;
         opponentMoveTimer.Start();

         settingsControlsEnabled(false);
         CreateButtons();
      }

      private void resetClicked(object sender, RoutedEventArgs e)
      {
         buttonsGrid.Children.Clear();
         currentPositionLabel.Content = "-";
         lastTechniqueLabel.Content = "-";
         opponentCurrentPositionLabel.Content = "-";
         opponentLastTechniqueLabel.Content = "-";

         //clear setup textboxes and disable them - so user can't prepare texts
         setup_textBox.Clear();
         helper_textBox.Clear();
         helper_textBox2.Clear();

         //reset timer
         setTimerLabel(selectedTime_textbox.Text);
         fightTimer.Stop();

         //stop opponent
         opponentMoveTimer.Stop();

         settingsControlsEnabled(true);
        }

      private void setTimerLabel(string time)
      {
         string minutes;
         string seconds;
         DateTime ignored = DateTime.ParseExact(time, "m:s", null);
         if (ignored.Minute < 10)
         {
            minutes = "0" + ignored.Minute.ToString();
         }
         else
         {
            minutes = ignored.Minute.ToString();
         }

         if (ignored.Second < 10)
         {
            seconds = "0" + ignored.Second.ToString();
         }
         else
         {
            seconds = ignored.Second.ToString();
         }

         //timer_label is null during InitializeComponents()
         try
         {
            timer_label.Content = minutes + ":" + seconds;
         }
         catch (NullReferenceException nullEx) { }
      }


      private void timeTextBoxChanged(object sender, TextChangedEventArgs e)
      {
         DateTime ignored;
         var textbox = sender as TextBox;
         if (DateTime.TryParseExact(textbox.Text, "m:s", CultureInfo.InvariantCulture, DateTimeStyles.None, out ignored))
         {
            newRoundButton.IsEnabled = true;
            textbox.ClearValue(TextBox.BorderBrushProperty);
            textbox.ClearValue(TextBox.BackgroundProperty);
            //setTimerLabel(ignored.ToString("mm:ss"));
         }
         else {
            newRoundButton.IsEnabled = false;
            textbox.BorderBrush = new SolidColorBrush(Colors.LightCoral);
            textbox.Background = new SolidColorBrush(Colors.LightCoral);
         }
      }

      private void dispatcherTimer_Tick(object sender, EventArgs e)
      {
         DateTime time = DateTime.ParseExact(timer_label.Content.ToString(), "m:s", null);
         var remaining = time.AddSeconds(-1);
         //not checking for 00:00 to allow it appear on timerLabel during gameplay
         if (remaining.ToString("mm:ss") != "59:59")
         {
            setTimerLabel(remaining.ToString("mm:ss"));
         }
         else {
            //if time is 59:59, raise reset button click event (end round)
            resetButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
         }
      }

      private void opponentMove(object sender, EventArgs e)
      {
         List<int> opponentAvailableTechniquesId = getAvailableTechniques(opponent_currentPosition, opponent_lastTechnique);
         foreach (int combo in opponentAvailableTechniquesId)
         {
            Console.WriteLine(combo);
         }

         if (opponentAvailableTechniquesId.Count != 0)
         {
            //randomly choose from available techniques
            Random rnd = new Random(DateTime.Now.Second);
            int chosen = rnd.Next(1, opponentAvailableTechniquesId.Count + 1) - 1;
            Console.WriteLine("chosen: "+opponentAvailableTechniquesId[chosen]);
            foreach (DataRow techniqueRow in myTechniquesTable.Rows)
            {
               if (techniqueRow.Field<int>("technique_id") == opponentAvailableTechniquesId[chosen])
               {
                  //set background variables for positions
                  opponent_currentPosition = techniqueRow.Field<string>("position_to");
                  currentPosition = positionPairsDict[opponent_currentPosition];
                  opponent_lastTechnique = techniqueRow.Field<string>("name");
                  lastTechnique = "-";

                  //set labels in UI
                  opponentCurrentPositionLabel.Content = opponent_currentPosition;
                  currentPositionLabel.Content = currentPosition;
                  opponentLastTechniqueLabel.Content = opponent_lastTechnique;
                  lastTechniqueLabel.Content = lastTechnique;

                  //create buttons and break
                  CreateButtons();
                  break;
               }
            }
         }
      }

      private void submissionHappened()
      {
         newRoundButton.IsEnabled = true;
         opponentMoveTimer.Stop();
         fightTimer.Stop();
         setTimerLabel(selectedTime_textbox.Text);
      }

      private List<int> getAvailableTechniques(string curr_pos,string last_tech) {
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

      private int getOpponentsSetupTime() {
         DateTime ignored_from = DateTime.ParseExact(opponentSetupTimeMin_textBox.Text, "m:s", null);
         DateTime ignored_to = DateTime.ParseExact(opponentSetupTimeMax_textBox.Text, "m:s", null);
         Random rnd = new Random(DateTime.Now.Second);
         return rnd.Next(ignored_from.Minute * 60 + ignored_from.Second, ignored_to.Minute * 60 + ignored_to.Second);
      }

      private void gi_radioButton_Checked(object sender, RoutedEventArgs e)
      {
         nogi_flag = false;
      }

      private void nogi_radioButton_Checked(object sender, RoutedEventArgs e)
      {
         nogi_flag = true;
      }

      private void settingsControlsEnabled(bool flag) {
         selectedTime_textbox.IsEnabled = flag;
         difficultyCombobox.IsEnabled = flag;
         newRoundButton.IsEnabled = flag;
         nogi_radioButton.IsEnabled = flag;
         gi_radioButton.IsEnabled = flag;
         opponentSetupTimeMin_textBox.IsEnabled = flag;
         opponentSetupTimeMax_textBox.IsEnabled = flag;

         setup_textBox.IsEnabled = !flag;
         helper_textBox.IsEnabled = !flag;
         helper_textBox2.IsEnabled = !flag;
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