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
using System.Windows.Navigation;
using System.Windows.Shapes;

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
         //this.Height = (System.Windows.SystemParameters.PrimaryScreenHeight);
         //this.Width = (System.Windows.SystemParameters.PrimaryScreenWidth);
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
