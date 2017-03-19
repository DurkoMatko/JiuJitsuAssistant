using System;
using System.Windows;
using MySql.Data.MySqlClient;
using System.Windows.Media;
using System.Windows.Threading;
using System.Diagnostics;
using System.Windows.Input;

namespace Jiu_Jitsu_Assistant
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      DispatcherTimer settingsTimer = new DispatcherTimer();
      int secondsCounter { get; set; }

      public MainWindow()
      {
         InitializeComponent();
         WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
         if (!ConnectToDatabase())
         {
            MessageBox.Show("Database connection error!");
            this.Close();
         }
         
         window.Background = new SolidColorBrush(Colors.Black);

         settingsTimer.Interval = TimeSpan.FromSeconds(0.5);
         settingsTimer.Tick += settingsTimer_Tick;
         settingsTimer.Start();
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

            MySqlConnection conn = new MySqlConnection(MyConString);
            conn.Open();
            return true;
         }
         catch (MySql.Data.MySqlClient.MySqlException e)
         {
            return false;
         }
      }

      private void button_Copy_Click(object sender, RoutedEventArgs e)
      {
         MyTechniques win2 = new MyTechniques(this.Left, this.Top, this.Height, this.Width);
         win2.ShowDialog();
         //this.Close();
      }

      private void Settings_Click(object sender, RoutedEventArgs e)
      {
         Statistics win2 = new Statistics(this.Left, this.Top, this.Height, this.Width);
         try
         {
            win2.ShowDialog();
         }
         catch (LiveCharts.Helpers.LiveChartsException ex) {
            MessageBox.Show("Statistics available once several techniques are added! Keep training and you'll get there soon");
         }
      }

      private void mindMaps_Click(object sender, RoutedEventArgs e)
      {
         MindMaps win2 = new MindMaps(this.Left, this.Top, this.Height, this.Width);
         win2.ShowDialog();
      }

      private void roll_Button_Click(object sender, RoutedEventArgs e)
      {
         Roll win2 = new Roll(this.Left, this.Top, this.Height, this.Width);
         win2.ShowDialog();
      }

      private void settingsTimer_Tick(object sender, EventArgs e) {
         secondsCounter++;
         if (secondsCounter % 2 == 0) 
            settingsLabel.Foreground = new SolidColorBrush(Colors.Gray);
         else
            settingsLabel.Foreground = new SolidColorBrush(Colors.White);
      }

      private void KeyPressed(object sender, KeyEventArgs e)
      {
         if (e.Key == Key.S)
         {
            Settings settingsWindow = new Settings();
            settingsWindow.ShowDialog();
            return;
         }
      }
   }
}
