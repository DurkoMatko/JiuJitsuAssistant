using System;
using System.Collections.ObjectModel;
using System.Windows;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Media;

namespace Jiu_Jitsu_Assistant
{
   /// <summary>
   /// Interaction logic for Statistics.xaml
   /// </summary>
   public partial class Statistics : Window
   {
      public SeriesCollection SeriesCollection { get; set; }
      public string[] Labels { get; set; }
      public Func<double, string> YFormatter { get; set; }

      public Statistics()
      {
         InitializeComponent();
      }

      public Statistics(double left, double top, double height, double width):this() {
         this.Left = left;
         this.Top = top;
         this.Height = height;
         this.Width = width;

         InitializeComponent();

         SeriesCollection = new SeriesCollection
            {
                new LiveCharts.Wpf.LineSeries
                {
                    Title = "Series 1",
                    Values = new ChartValues<double> { 4, 6, 5, 2 ,4 }
                },
                new LineSeries
                {
                    Title = "Series 2",
                    Values = new ChartValues<double> { 6, 7, 3, 4 ,6 },
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Series 3",
                    Values = new ChartValues<double> { 4,2,7,2,7 },
                    PointGeometry = DefaultGeometries.Square,
                    PointGeometrySize = 15
                }
            };

         Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May" };
         YFormatter = value => value.ToString("C");

         //modifying the series collection will animate and update the chart
         SeriesCollection.Add(new LineSeries
         {
            Title = "Series 4",
            Values = new ChartValues<double> { 5, 3, 2, 4 },
            LineSmoothness = 0, //0: straight lines, 1: really smooth lines
            PointGeometry = Geometry.Parse("m 25 70.36218 20 -28 -20 22 -8 -6 z"),
            PointGeometrySize = 50,
            PointForeround = Brushes.Gray
         });

         //modifying any series values will also animate and update the chart
         SeriesCollection[3].Values.Add(5d);

         DataContext = this;
      }
      
   }
}
