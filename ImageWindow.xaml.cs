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

        private HistogramWindow? histogramWindow;
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
        }

        public void UpdateHistogram()
        {
            if (imageMat != null)
            {
                histogramWindow?.DisplayHistogram(imageMat);
            }
        }

        private void ImageWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Window is closing");
            ImageWindowClosing?.Invoke();
            if(imageMat  != null)
            {
                imageMat.Dispose();
                imageMat = null;
            }

        }
    }
}