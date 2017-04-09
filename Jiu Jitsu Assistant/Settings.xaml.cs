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
      public List<String> languageFilesList = new List<String>();
      public String[] languageFiles { get; set; }

      public Settings()
      {
         foreach(ResourceDictionary rd in Application.Current.Resources.MergedDictionaries)
         {
            if (rd.Source.ToString() != "StringResources.xaml")
            {
               int start = rd.Source.ToString().IndexOf(".") + 1;
               int end = rd.Source.ToString().LastIndexOf(".");
               string languageAbbreviation = rd.Source.ToString().Substring(start, end - start);
               //show user just the culture part of filename
               languageFilesList.Add(languageAbbreviation);
            }
         }
         languageFiles = languageFilesList.ToArray();
         InitializeComponent();
         DataContext = this;
      }

      private void changedLanguageSelection(object sender, SelectionChangedEventArgs e)
      {
         LanguageSetter.SelectCulture((sender as ComboBox).SelectedItem.ToString());
      }
   }
}
