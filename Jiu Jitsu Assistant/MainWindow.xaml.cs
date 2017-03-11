using System;
using System.Windows;
using MySql.Data.MySqlClient;
using System.Windows.Media;

namespace Jiu_Jitsu_Assistant
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      public MainWindow()
      {
         InitializeComponent();
         WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
         if (!ConnectToDatabase())
         {
            MessageBox.Show("Database connection error!");
         }
         
         window.Background = new SolidColorBrush(Colors.Black);
         //this.Height = (System.Windows.SystemParameters.PrimaryScreenHeight);
         //this.Width = (System.Windows.SystemParameters.PrimaryScreenWidth);
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
         win2.ShowDialog();
      }
   }
}
