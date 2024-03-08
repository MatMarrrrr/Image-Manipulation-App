using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Emgu.CV;

namespace APO_Mateusz_Marek_20456
{
    public partial class ImageWindow : Window
    {
        public static event Action<Mat, string>? ImageWindowFocused;
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

            Activated += (sender, e) => ImageWindowFocused?.Invoke(imageMat, shortFileName);
            Closing += ImageWindow_Closing;
        }

        public void ShowHistogram()
        {
            if (imageMat != null)
            {
                if (histogramWindow == null)
                {
                    histogramWindow = new HistogramWindow(shortFileName + " - histogram");
                    histogramWindow.Closed += (sender, e) => histogramWindow = null;
                    histogramWindow.DisplayHistogram(imageMat);
                }
                else
                {
                    histogramWindow.Focus();
                }
            }
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