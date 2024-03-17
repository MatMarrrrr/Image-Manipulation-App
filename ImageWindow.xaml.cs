using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Emgu.CV;

namespace Image_Manipulation_App
{
    public partial class ImageWindow : Window
    {
        public static event Action<ImageWindow, Mat, string, string>? ImageWindowFocused;
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

            Activated += (sender, e) => ImageWindowFocused?.Invoke(this, imageMat, fileName, shortFileName);
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
            this.imageMat = imageMat;
            BitmapSource imageSource = BitmapSourceConverter.ToBitmapSource(imageMat);
            imageControl.Source = imageSource;
        }

        public void UpdateTitlePrefix(string title)
        {
            this.Title = $"({title}) {shortFileName}";
        }

        public void UpdateHistogram()
        {
            if (imageMat != null)
            {
                histogramWindow?.DisplayHistogram(imageMat);
            }
        }

        public void UpdateImageAndHistogram(Mat imageMat)
        {
            this.UpdateImage(imageMat);
            this.UpdateHistogram();
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

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                const double zoomFactor = 0.1;
                var currentScale = imageScaleTransform.ScaleX;
                if (e.Delta > 0)
                {
                    imageScaleTransform.ScaleX = currentScale + zoomFactor;
                    imageScaleTransform.ScaleY = currentScale + zoomFactor;
                }
                else if (e.Delta < 0 && currentScale - zoomFactor > 0)
                {
                    imageScaleTransform.ScaleX = currentScale - zoomFactor;
                    imageScaleTransform.ScaleY = currentScale - zoomFactor;
                }

                e.Handled = true;
            }
        }
    }
}