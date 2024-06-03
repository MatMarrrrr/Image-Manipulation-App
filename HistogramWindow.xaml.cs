using Emgu.CV;
using Emgu.CV.Reg;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
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
using static Image_Manipulation_App.ImageOperations;

namespace Image_Manipulation_App
{
    public partial class HistogramWindow : Window
    {
        public HistogramWindow(string title, bool profileLine = false)
        {
            InitializeComponent();
            this.Title = title;
            if (profileLine)
            {
                tableChart.Visibility = Visibility.Hidden;
            }
        }

        public void DisplayHistogram(Mat imageMat)
        {
            int[] histogramData = CalculateHistogram(imageMat);
            List<HistogramTableDataRow> histogramTableData = CalculateTableHistogramData(histogramData);

            ColumnSeries<int> series = new ColumnSeries<int>
            {
                Values = histogramData,
                Fill = new SolidColorPaint(SKColors.Black),
                Padding = 0,
                YToolTipLabelFormatter = (chartPoint) => FormatHistogramTooltip(chartPoint.Coordinate.ToString())
            };

            histogramChart.Series = new ISeries[] { series };
            HistogramTable.ItemsSource = histogramTableData;

            this.Show();
        }

        public void ShowProfileLine(List<int> data)
        {
            var lineSeries = new LineSeries<int>
            {
                Values = data,
                GeometrySize = 5,
                LineSmoothness = 0,
                Stroke = new SolidColorPaint(SKColors.Blue),
                Fill = new SolidColorPaint(SKColors.Transparent)
            };

            histogramChart.Series = new ISeries[] { lineSeries };

            this.Show();
        }

        private string FormatHistogramTooltip(string oldText)
        {
            if (string.IsNullOrEmpty(oldText))
                return oldText;

            string[] newTextArr = oldText.Replace("(", "").Replace(")", "").Trim().Split(",");
            return $"Intensity: {newTextArr[0]}{Environment.NewLine}Count: {newTextArr[1]}";
        }
    }
}
