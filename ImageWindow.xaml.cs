using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Emgu.CV;

namespace APO_Mateusz_Marek_20456
{
    public partial class ImageWindow : Window
    {
        public static event Action<Mat, string, string>? ImageWindowFocused;
        public static event Action? ImageWindowClosing;

        public HistogramWindow? histogramWindow;
        public Mat ?imageMat;
        public string fileName;
        public string shortFileName;

        public ImageWindow(BitmapSource image, Mat mat, string fileName, string shortFileName)
        {
            InitializeComponent();
            this.imageControl.Source = image;
            this.imageMat = mat;
            this.fileName = fileName;
            this.shortFileName = shortFileName;

            Activated += (sender, e) => ImageWindowFocused?.Invoke(imageMat, fileName, shortFileName);
            Closing += ImageWindow_Closing;
        }

        public void ShowHistogram()
        {
            if (imageMat == null || imageMat.NumberOfChannels != 1)
            {
                MessageBox.Show("Histogram can only be displayed for grayscale images.");
                return;
            }

            if (histogramWindow == null)
            {
                histogramWindow = new HistogramWindow($"histogram - {this.Title}");
                histogramWindow.Closed += (sender, e) => histogramWindow = null;
                histogramWindow.DisplayHistogram(imageMat);
            }
            else
            {
                histogramWindow.Focus();
            }
        }

        public void UpdateImage(Mat imageMat)
        {
            string imageType = imageMat.NumberOfChannels == 1 ? "GrayScale" : "Color";

            this.Title = $"({imageType}) {shortFileName}";
            this.imageMat = imageMat;
            BitmapSource imageSource = BitmapSourceConverter.ToBitmapSource(imageMat);
            imageControl.Source = imageSource;
        }

        public void UpdateHistogram()
        {
            if (imageMat != null)
            {
                histogramWindow?.DisplayHistogram(imageMat);
            }
        }

        public void ClearHistogramWindowReference()
        {
            histogramWindow = null;
        }

        public void ImageWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            ImageWindowClosing?.Invoke();
            if (imageMat != null)
            {
                imageMat.Dispose();
                imageMat = null;
            }
            histogramWindow?.Close();
        }
    }
}