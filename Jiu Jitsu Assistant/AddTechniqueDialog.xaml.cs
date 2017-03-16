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
using System.Windows.Threading;

namespace Jiu_Jitsu_Assistant
{
   /// <summary>
   /// Interaction logic for AddTechniqueDialog.xaml
   /// </summary>
   public partial class AddTechniqueDialog : Window
   {
      public AddTechniqueDialog()
      {
         InitializeComponent();

      }

      public AddTechniqueDialog(bool success,double X, double Y, string element) : this()
      {
         this.Left = X - 300 ;
         this.Top = Y - 150;
         if (success)
         {
            textBlock.Text = element + " successfully added!";
            textBlock.Foreground = new SolidColorBrush(Colors.Green);
         }
         else {
            textBlock.Text = string.Format("There was an error while {0} adding " + element + "!", Environment.NewLine);
            textBlock.Foreground = new SolidColorBrush(Colors.Red);
         }
      }


      private void StartCloseTimer(object sender, RoutedEventArgs e)
      {
         DispatcherTimer timer = new DispatcherTimer();
         timer.Interval = TimeSpan.FromSeconds(1d);
         timer.Tick += TimerTick;
         timer.Start();
      }

      private void TimerTick(object sender, EventArgs e)
      {
         DispatcherTimer timer = (DispatcherTimer)sender;
         timer.Stop();
         timer.Tick -= TimerTick;
         this.Close();
      }

      private void closeOnOkButton(object sender, RoutedEventArgs e) {
         this.Close();
      }
   }
}
