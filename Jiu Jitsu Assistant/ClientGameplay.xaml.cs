using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Jiu_Jitsu_Assistant
{
   /// <summary>
   /// Interaction logic for MindMaps.xaml
   /// </summary>
   public partial class ClientGameplay : Window
   {
      MySqlConnection conn;
      private DataTable myTechniquesTable { get; set; }
      Dictionary<int, string> techniquesDict = new Dictionary<int, string>();
      private DataTable setupsTable { get; set; }
      Dictionary<int, List<string>> setupsDict = new Dictionary<int, List<string>>();
      Dictionary<string, string> positionPairsDict = new Dictionary<string, string>();

      private string currentPosition { get; set; }
      private string lastTechnique { get; set; }

      double difficultyTreshold { get; set; }
      bool nogi_flag { get; set; }
      DispatcherTimer fightTimer = new DispatcherTimer();

      //Socket communication members
      // The port number for the remote device.  
      private const int port = 11000;
      Socket connectionSocket;
      // ManualResetEvent instances signal completion.  
      private static ManualResetEvent connectDone = new ManualResetEvent(false);
      private static ManualResetEvent sendDone = new ManualResetEvent(false);
      private static ManualResetEvent receiveDone = new ManualResetEvent(false);

      // The response from the remote device.  
      private static String response = String.Empty;

      public ClientGameplay(string ipAddress)
      {
         InitializeComponent();

         if (ConnectToDatabase())
         {
            LoadTechniques();
            LoadSetups();
            LoadPositionPairs();
            setDictionaries();
         }
         StartClient(ipAddress);
         newGameplay();
         fightTimer.Interval = TimeSpan.FromSeconds(1);
         fightTimer.Tick += dispatcherTimer_Tick;
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
               if (availableTechniques.Contains(techniqueRow.Field<int>("technique_id")))
               {
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

         sendChangeToOpponent(buttonTextBlock.Text + "$" + currentPosition);

         this.buttonsGrid.Children.Clear();
         CreateButtons();
      }

      void techniqueClicked_opponent(string hisTechnique, string hisPosition)
      {
         this.Dispatcher.Invoke(() =>
         {
            currentPosition = positionPairsDict[hisPosition];
            lastTechnique = "-";
            currentPositionLabel.Content = currentPosition;
            lastTechniqueLabel.Text = lastTechnique;

            opponentCurrentPositionLabel.Content = hisPosition;
            opponentLastTechniqueLabel.Text = hisTechnique;

            this.buttonsGrid.Children.Clear();
            CreateButtons();
         });
      }

      void mouseDownOnTechnique(object sender, MouseButtonEventArgs args)
      {
         var techButton = sender as Button;
         techButton.Background = (System.Windows.Media.Brush)FindResource("clickedTechnique");
      }


      public ClientGameplay(double left, double top, double height, double width, string ipAddress) : this(ipAddress)
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

      private void startNewGameplay(object sender, RoutedEventArgs e)
      {
         newGameplay();
      }

      private void newGameplay()
      {
         buttonsGrid.Children.Clear();
         timer_label.Content = "05:00";
         fightTimer.Start();
         currentPosition = "Both standing";
         currentPositionLabel.Content = currentPosition;
         lastTechniqueLabel.Text = "none";
         lastTechnique = "none";
         difficultyTreshold = 0.4;
         CreateButtons();
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

      private void dispatcherTimer_Tick(object sender, EventArgs e)
      {
         DateTime time = DateTime.ParseExact(timer_label.Content.ToString(), "m:s", null);
         var remaining = time.AddSeconds(-1);
         //not checking for 00:00 to allow it appear on timerLabel during gameplay
         if (remaining.ToString("mm:ss") != "59:59")
         {
            setTimerLabel(remaining.ToString("mm:ss"));
         }
         else
         {
            //if time is 59:59, raise reset button click event (end round)
            newRoundButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
         }
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



      //COMMUNICATION SOCKETS PARt ///////////////////////////////////////////////////////////////////////////////////////////////////
      public void StartClient(string ipAddressString)
      {
         // Connect to a remote device.  
         // Establish the remote endpoint for the socket.  
         // The name of the   
         // remote device is "host.contoso.com".  
         IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
         IPAddress ipAddress2 = ipHostInfo.AddressList[0];
         IPAddress ipAddress = IPAddress.Parse(ipAddressString);
         IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

         // Create a TCP/IP socket.  
         connectionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

         // Connect to the remote endpoint.  
         connectionSocket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), connectionSocket);
         connectDone.WaitOne();

         // Create the state object and start listening  
         StateObject state = new StateObject();
         state.workSocket = connectionSocket;
         connectionSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
      }

      public void ReadCallback(IAsyncResult ar)
      {
         String content = String.Empty;

         // Retrieve the state object and the handler socket  
         // from the asynchronous state object.  
         StateObject state = (StateObject)ar.AsyncState;
         Socket connectionSocket_ref = state.workSocket;

         // Read data from the client socket.   
         int bytesRead = connectionSocket_ref.EndReceive(ar);

         if (bytesRead > 0)
         {
            // Check for end-of-file tag. If it is not there, read more data.  
            content = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);
            // All the data has been read from the client. Display it on the console.  
            Console.WriteLine("Opponent has usd technique: {0}", content);
            techniqueClicked_opponent(content.Split('$')[0], content.Split('$')[1]);
            connectionSocket_ref.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
         }
      }

      private void ConnectCallback(IAsyncResult ar)
      {
         // Retrieve the socket from the state object.  
         connectionSocket = (Socket)ar.AsyncState;

         // Complete the connection.  
         connectionSocket.EndConnect(ar);

         Console.WriteLine("Socket connected to {0}", connectionSocket.RemoteEndPoint.ToString());

         // Signal that the connection has been made.  
         connectDone.Set();
      }

      private void sendChangeToOpponent(String data)
      {
         // Convert the string data to byte data using ASCII encoding.  
         byte[] byteData = Encoding.ASCII.GetBytes(data);

         // Begin sending the data to the remote device.  
         connectionSocket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), connectionSocket);
      }

      private void SendCallback(IAsyncResult ar)
      {
         // Retrieve the socket from the state object.  
         connectionSocket = (Socket)ar.AsyncState;

         // Complete sending the data to the remote device.  
         int bytesSent = connectionSocket.EndSend(ar);
         Console.WriteLine("Sent {0} bytes to server.", bytesSent);

         // Signal that all bytes have been sent.  
         sendDone.Set();
      }


      ///NOT NEEDED FUNCTIONS IN GAMEPLAY///////////////////////////////////////////////////////////////////////////////
      ///
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
         else
         {
            newRoundButton.IsEnabled = false;
            textbox.BorderBrush = new SolidColorBrush(Colors.LightCoral);
            textbox.Background = new SolidColorBrush(Colors.LightCoral);
         }
      }



      private void button_Click(object sender, RoutedEventArgs e)
      {
         sendChangeToOpponent("Client message");
      }
   }
}
