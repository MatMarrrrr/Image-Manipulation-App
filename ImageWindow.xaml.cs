using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emgu.CV;


namespace Image_Manipulation_App
{
    public partial class ImageWindow : Window
    {
        public static event Action<ImageWindow, Mat, string, string, bool>? ImageWindowFocused;
        public static event Action? ImageWindowClosing;

        public HistogramWindow? histogramWindow;
        public HistogramWindow? profileLineWindow;
        public WriteableBitmap writableBitmap;
        public Mat? imageMat;
        public string fileName;
        public string shortFileName;
        private bool isDisposed = false;
        public bool isProfileLine = false;

        private List<System.Windows.Point> points = new List<System.Windows.Point>();
        public List<int> pointsValues = new List<int>();

        public ImageWindow(BitmapSource image, Mat mat, string fileName, string shortFileName)
        {
            InitializeComponent();
            writableBitmap = new WriteableBitmap(image);
            this.imageControl.Source = image;
            this.imageMat = mat.Clone();
            this.fileName = fileName;
            this.shortFileName = shortFileName;

            Activated += (sender, e) => ImageWindowFocused?.Invoke(this, imageMat, fileName, shortFileName, isProfileLine);
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
            histogramWindow?.Close();
            profileLineWindow?.Close();
            this.DisposeImage();
        }

        public void DisposeImage()
        {
            if (!isDisposed && imageMat != null)
            {
                imageMat.Dispose();
                isDisposed = true;
                imageMat = null;
            }
        }
        private Point ScalePointToImage(Point point)
        {
            if (imageControl.Source is BitmapSource bitmapSource)
            {
                var actualWidth = bitmapSource.PixelWidth;
                var actualHeight = bitmapSource.PixelHeight;
                var displayedWidth = imageControl.ActualWidth;
                var displayedHeight = imageControl.ActualHeight;

                var scaleX = actualWidth / displayedWidth;
                var scaleY = actualHeight / displayedHeight;

                return new Point(point.X * scaleX, point.Y * scaleY);
            }

            return point;
        }

        private void ImageControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isProfileLine)
            {
                var position = e.GetPosition(imageControl);
                var scaledPosition = ScalePointToImage(position);

                if (points.Count == 2)
                {
                    points.Clear();
                    RestoreOriginalImage();
                }

                points.Add(scaledPosition);

                if (points.Count == 2)
                {
                    DrawLineBetweenPoints();
                }
            }

        }

        private void RestoreOriginalImage()
        {
            imageControl.Source = BitmapSourceConverter.ToBitmapSource(imageMat);
        }

        private void DrawLineBetweenPoints()
        {
            if (points.Count >= 2 && imageControl.Source is BitmapSource bitmapSource)
            {
                WriteableBitmap writableBitmap = ConvertToWriteableBitmap(bitmapSource);

                DrawingVisual visual = new DrawingVisual();
                using (DrawingContext drawingContext = visual.RenderOpen())
                {
                    drawingContext.DrawImage(writableBitmap, new Rect(0, 0, writableBitmap.PixelWidth, writableBitmap.PixelHeight));
                    drawingContext.DrawLine(new Pen(Brushes.Red, 3), points[0], points[1]);
                }

                RenderTargetBitmap rtb = new RenderTargetBitmap(writableBitmap.PixelWidth, writableBitmap.PixelHeight, writableBitmap.DpiX, writableBitmap.DpiY, PixelFormats.Default);
                rtb.Render(visual);
                imageControl.Source = rtb;
                DisplayLinePixelValues();
            }
        }

        private WriteableBitmap ConvertToWriteableBitmap(BitmapSource source)
        {
            WriteableBitmap writable = new WriteableBitmap(source);
            return writable;
        }

        private void DisplayLinePixelValues()
        {
            if (points.Count < 2 || imageMat == null)
            {
                MessageBox.Show("Two points must be selected and the image must be loaded.");
                return;
            }

            Point start = points[0];
            Point end = points[1];
            var linePixels = GetLinePixels(start, end);
            ExtractAndFormatPixelValues(linePixels);
            
            if(this.profileLineWindow == null)
            {
                profileLineWindow = new HistogramWindow($"Profile line - {this.Title}", true);
                profileLineWindow.Closed += (sender, e) => profileLineWindow = null;
                profileLineWindow.ShowProfileLine(pointsValues);
            }
            else
            {
                profileLineWindow.ShowProfileLine(pointsValues);
            }
        }

        private List<Point> GetLinePixels(Point start, Point end)
        {
            List<Point> line = new List<Point>();
            int x = (int)start.X;
            int y = (int)start.Y;
            int x2 = (int)end.X;
            int y2 = (int)end.Y;

            int w = x2 - x;
            int h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            for (int i = 0; i <= longest; i++)
            {
                line.Add(new Point(x, y));
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }
            return line;
        }

        private void ExtractAndFormatPixelValues(List<Point> linePixels)
        {
            pointsValues = new List<int>();
            foreach (Point p in linePixels)
            {
                int x = (int)p.X;
                int y = (int)p.Y;
                if (x >= 0 && y >= 0 && x < imageMat.Width && y < imageMat.Height)
                {
                    IntPtr lineStart = imageMat.DataPointer + y * imageMat.Step;
                    byte pixelValue = Marshal.ReadByte(lineStart, x);
                    pointsValues.Add(pixelValue);
                }
            }
        }

        public void RestoreImage()
        {
            this.imageControl.Source = BitmapSourceConverter.ToBitmapSource(imageMat);
        }
    }
}