using Emgu.CV;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
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

namespace APO_Mateusz_Marek_20456
{
    public partial class HistogramWindow : Window
    {
        public HistogramWindow(string title)
        {
            InitializeComponent();
            this.Title = title;
        }

        public void DisplayHistogram(Mat imageMat)
        {
            if (imageMat.NumberOfChannels != 1)
            {
                MessageBox.Show("Histogram can only be displayed for monochrome images.");
                return;
            }

            var histogramData = CalculateHistogram(imageMat);
            var series = new ColumnSeries<int>
            {
                Values = histogramData,
                Fill = new SolidColorPaint(SKColors.Black),
                YToolTipLabelFormatter = (chartPoint) => FormatHistogramTooltip(chartPoint.Coordinate.ToString())
            };


            histogramChart.Series = new ISeries[] { series };

            var histogramTableData = new List<HistogramDataRow>();
            for (int i = 0; i < histogramData.Length; i++)
            {
                histogramTableData.Add(new HistogramDataRow { Intensity = i, Count = histogramData[i] });
            }

            pixelDataTable.ItemsSource = histogramTableData;

            Show();
        }


        private int[] CalculateHistogram(Mat imageMat)
        {
            int[] histogramData = new int[256];

            if (imageMat.NumberOfChannels == 1)
            {
                IntPtr scan0 = imageMat.DataPointer;

                int step = imageMat.Step;
                int width = imageMat.Width;
                int height = imageMat.Height;

                unsafe
                {
                    for (int i = 0; i < height; i++)
                    {
                        byte* row = (byte*)scan0.ToPointer() + (i * step);
                        for (int j = 0; j < width; j++)
                        {
                            byte intensity = row[j];
                            histogramData[intensity]++;
                        }
                    }
                }
            }

            return histogramData;
        }

        private string FormatHistogramTooltip(string oldText)
        {
            if (string.IsNullOrEmpty(oldText))
                return oldText;

            string[] newTextArr = oldText.Replace("(", "").Replace(")", "").Trim().Split(",");
            return $"Value: {newTextArr[0]}{Environment.NewLine}Count: {newTextArr[1]}";
        }

        public class HistogramDataRow
        {
            public int Intensity { get; set; }
            public int Count { get; set; }
        }
    }
}
