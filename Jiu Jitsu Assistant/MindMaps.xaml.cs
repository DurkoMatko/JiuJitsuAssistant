﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
      Dictionary<int, string> setupsDict = new Dictionary<int, string>();

      private string currentPosition { get; set; }
      private string lastTechnique { get; set; }

      double difficultyTreshold { get; set; }


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
            Console.WriteLine("test");
            int availableTechniquesCount = 0;
            foreach (DataRow techniqueRow in myTechniquesTable.Rows)
            {
               if (techniqueRow.Field<string>("position_from") == currentPosition &&
                   techniqueRow.Field<string>("name") != lastTechnique){
                  availableTechniquesCount++;
               }
            }

            foreach (DataRow techniqueRow in myTechniquesTable.Rows)
            {
               if (techniqueRow.Field<string>("position_from") == currentPosition &&
                   techniqueRow.Field<string>("name") != lastTechnique)
               {

               string resourceBrushKey = "techniqueGroupGrad_" + techniqueRow.Field<int>("group_id").ToString() + "_disabled";
               Button button = new Button()
               {
                  //this is kind of "hackerei"...using unrelated button properties for my purpose...in the future define my own button class should be optimal

                  //Content = techniqueRow.Field<string>("name").ToString(),
                  Content = new TextBlock { Text = techniqueRow.Field<string>("name").ToString(), Foreground= new SolidColorBrush(Colors.Gray)  },
                     ToolTip = "Leads to: " + techniqueRow.Field<string>("position_to").ToString(),
                     Tag = techniqueRow.Field<int>("technique_id").ToString(),
                     Uid = techniqueRow.Field<string>("position_to").ToString(),
                     Name = "_" + techniqueRow.Field<int>("group_id").ToString(),
                     //resource defined in app.xaml
                     Style = (Style)FindResource("MindMapsButton_disabled"),
                     IsEnabled = false,
                     Background = (System.Windows.Media.Brush)FindResource(resourceBrushKey),
                     Width = (900 / availableTechniquesCount) * 0.8,
                     Height = (800 / availableTechniquesCount) * 0.8
                  };

                  button.Click += new RoutedEventHandler(techniqueClicked);
                  this.buttonsGrid.Children.Add(button);
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

         this.buttonsGrid.Children.Clear();
         CreateButtons();
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
            sb.Append("SELECT t.technique_id, t.group_id, t.name, t.date_learned, t.belt_level, pFrom.name as position_from, pTo.name as position_to FROM techniques as t ");
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
            setupsDict.Add(setupRow.Field<int>("technique_id"), setupRow.Field<string>("description"));
         }
      } 

      private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
      {
         if (this.conn.State == ConnectionState.Open)
            this.conn.Close();
      }

      private void enableTechniques(object sender, TextChangedEventArgs e)
      {
         // check each button if should be enabled
         foreach (Button button in this.buttonsGrid.Children) {
            string resourceBrushKey = "techniqueGroupGrad" + button.Name;
            int toChange = LevenshteinDistance.Compute(setup_textBox.Text, setupsDict[int.Parse(button.Tag.ToString())]);
            int matchedLetters = setupsDict[int.Parse(button.Tag.ToString())].Length - toChange;
            //if matched letters more that required threshold, enable button
            if (matchedLetters > setupsDict[int.Parse(button.Tag.ToString())].Length * difficultyTreshold)
            {
               button.IsEnabled = true;
               button.Style = (Style)FindResource("MindMapsButton_enabled");
               button.Background = (System.Windows.Media.Brush)FindResource(resourceBrushKey);
               var textBlock = button.Content as TextBlock;
               textBlock.Foreground = new SolidColorBrush(Colors.Black);
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

      private void newRoundClicked(object sender, RoutedEventArgs e)
      {
         currentPosition = "Standing";
         currentPositionLabel.Content = currentPosition;
         lastTechniqueLabel.Content = "none";
         var selectedDifficulty = difficultyCombobox.SelectedItem as ComboBoxItem;
         difficultyTreshold = Double.Parse(selectedDifficulty.Tag.ToString());
         difficultyCombobox.IsEnabled = false;
         newRoundButton.IsEnabled = false;
         CreateButtons();
      }

      private void resetClicked(object sender, RoutedEventArgs e) {
         buttonsGrid.Children.Clear();
         difficultyCombobox.IsEnabled = true;
         newRoundButton.IsEnabled = true;
         currentPositionLabel.Content = "none";
         lastTechniqueLabel.Content = "none";
      }
      
   }
}
