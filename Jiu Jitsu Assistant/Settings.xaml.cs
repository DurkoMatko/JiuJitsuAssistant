using System;
using System.Collections.Generic;
using System.IO;
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

namespace Jiu_Jitsu_Assistant
{
   /// <summary>
   /// Interaction logic for Settings.xaml
   /// </summary>
   public partial class Settings : Window
   {
      public FileInfo[] MyCollection { get; set; }

      public Settings()
      {
         var di = new DirectoryInfo(System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName));
         MyCollection = di.GetFiles();
         InitializeComponent();
         DataContext = this;
      }
   }
}
