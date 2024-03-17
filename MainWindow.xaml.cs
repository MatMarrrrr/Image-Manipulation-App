using System;
using System.Windows;
using Microsoft.Win32;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;

namespace APO_Mateusz_Marek_20456
{
    public partial class MainWindow : Window
    {
        private List<ImageWindow> imageWindows = new List<ImageWindow>();
        public Mat? selectedImageMat;
        public string? selectedImageFileName;
        public string? selectedImageShortFileName;
        public ImageWindow? activeImageWindow;

        public MainWindow()
        {
            InitializeComponent();
            ImageWindow.ImageWindowFocused += UpdateSelectedImageMat;
            ImageWindow.ImageWindowClosing += ClearSelectedImageMat;
            Closing += MainWindow_Closing;
        }

        private void OpenImage_Click(object sender, RoutedEventArgs e, bool isColor)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = openFileDialog.FileName;
                ImreadModes mode = isColor ? ImreadModes.Color : ImreadModes.Grayscale;
                Mat img = CvInvoke.Imread(fileName, mode);
                DisplayImageInNewWindow(img, fileName);
            }
        }


        private void OpenColor_Click(object sender, RoutedEventArgs e)
        {
            OpenImage_Click(sender, e, true);
        }

        private void OpenGrayScale_Click(object sender, RoutedEventArgs e)
        {
            OpenImage_Click(sender, e, false);
        }

        private void CreateHistogram_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat != null)
            {
                this.activeImageWindow?.ShowHistogram();
            }
            else
            {
                MessageBox.Show("No image selected");
            }
        }

        private void ConvertToGrayScale_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null)
            {
                MessageBox.Show("No image selected");
                return;
            }
            else if (this.selectedImageMat.NumberOfChannels == 1)
            {
                MessageBox.Show("Image is already gray scale");
                return;
            }
            else
            {
                this.selectedImageMat = ImageOperarions.ConvertToGrayScale(this.selectedImageMat);
                activeImageWindow?.UpdateImage(this.selectedImageMat);
                activeImageWindow?.UpdateHistogram();
            }
        }

        private void Negate_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null)
            {
                MessageBox.Show("No image selected");
                return;
            }
            else if (this.selectedImageMat.NumberOfChannels != 1)
            {
                MessageBox.Show("Negation can only be applied to grayscale images.");
                return;
            }
            else
            {
                this.selectedImageMat = ImageOperarions.NegateImage(this.selectedImageMat);
                activeImageWindow?.UpdateImage(this.selectedImageMat);
                activeImageWindow?.UpdateHistogram();
            }

        }

        private void StretchContrast_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null)
            {
                MessageBox.Show("No image selected");
                return;
            }
            else if (this.selectedImageMat.NumberOfChannels != 1)
            {
                MessageBox.Show("Contrast stretching can only be applied to grayscale images.");
                return;
            }
            else
            {
                var dialog = new StretchContrastParamsWindow();
                if (dialog.ShowDialog() == true)
                {
                    int minValue = dialog.MinValue ?? 0;
                    int maxValue = dialog.MaxValue ?? 255;

                    this.selectedImageMat = ImageOperarions.StretchHistogram(this.selectedImageMat, (byte)minValue, (byte)maxValue);
                    activeImageWindow?.UpdateImage(this.selectedImageMat);
                    activeImageWindow?.UpdateHistogram();
                }
            }

        }

        private void SplitChannels_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.selectedImageFileName == null)
            {
                MessageBox.Show("No image selected");
                return;
            }
            else if (this.selectedImageMat.NumberOfChannels < 3)
            {
                MessageBox.Show("Splitting channels can only be applied to images with at least 3 channels");
                return;
            }
            else
            {
                var channels = ImageOperarions.SplitChannels(this.selectedImageMat, "RGB");
                foreach(var channel in channels)
                {
                    string windowTitle = $"{channel.channelName} {this.selectedImageShortFileName}";
                    DisplayImageInNewWindow(channel.image, this.selectedImageFileName, windowTitle);
                }

            }
        }

        private void ConvertToHSVAndSplitChannels_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.selectedImageFileName == null)
            {
                MessageBox.Show("No image selected");
                return;
            }
            else if (this.selectedImageMat.NumberOfChannels < 3)
            {
                MessageBox.Show("Conversion to HSV and splitting channels can only be applied to images with at least 3 channels");
                return;
            }
            else
            {
                var hsvChannels = ImageOperarions.ConvertAndSplitRgb(this.selectedImageMat, "HSV");

                foreach (var channel in hsvChannels)
                {
                    string windowTitle = $"{channel.channelName} {this.selectedImageShortFileName}";
                    DisplayImageInNewWindow(channel.image, this.selectedImageFileName, windowTitle);
                }
            }
        }

        private void ConvertToLabAndSplitChannels_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.selectedImageFileName == null)
            {
                MessageBox.Show("No image selected");
                return;
            }
            else if (this.selectedImageMat.NumberOfChannels < 3)
            {
                MessageBox.Show("Conversion to Lab and splitting channels can only be applied to images with at least 3 channels");
                return;
            }
            else
            {
                var labChannels = ImageOperarions.ConvertAndSplitRgb(this.selectedImageMat, "Lab");

                foreach (var channel in labChannels)
                {
                    string windowTitle = $"{channel.channelName} {this.selectedImageShortFileName}";
                    DisplayImageInNewWindow(channel.image, this.selectedImageFileName, windowTitle);
                }
            }
        }

        private void EqualizeHistogram_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null)
            {
                MessageBox.Show("No image selected");
                return;
            }
            else if (this.selectedImageMat.NumberOfChannels != 1)
            {
                MessageBox.Show("Histogram equalization can only be applied to grayscale images.");
                return;
            }
            else
            {
                this.selectedImageMat = ImageOperarions.EqualizeHistogram(this.selectedImageMat);
                activeImageWindow?.UpdateImage(this.selectedImageMat);
                activeImageWindow?.UpdateHistogram();
            }
        }

        private void StretchHistogram_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null)
            {
                MessageBox.Show("No image selected");
                return;
            }
            else if (this.selectedImageMat.NumberOfChannels != 1)
            {
                MessageBox.Show("Histogram stretching can only be applied to grayscale images.");
                return;
            }
            else
            {
                this.selectedImageMat = ImageOperarions.StretchHistogram(this.selectedImageMat);
                activeImageWindow?.UpdateImage(this.selectedImageMat);
                activeImageWindow?.UpdateHistogram();
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Image Manipulation App{Environment.NewLine}{Environment.NewLine}Created by: Mateusz Marek");
        }

        /*
        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            foreach (var window in Application.Current.Windows.OfType<ImageWindow>())
            {
                window.Close();
            }
        }
        */

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            var windowsToClose = imageWindows.ToList();
            foreach (var window in windowsToClose)
            {
                window.Close();
            }
        }

        private ImageWindow DisplayImageInNewWindow(Mat img, string fileName, string? customWindowTitle = null)
        {
            string shortFileName = Path.GetFileName(fileName);
            string windowTitle = "";

            if(customWindowTitle == null)
            {
                string imageType = img.NumberOfChannels == 1 ? "GrayScale" : "Color";
                windowTitle = $"({imageType}) {shortFileName}";
            }
            else
            {
                windowTitle = customWindowTitle;
            }

            BitmapSource imageSource = BitmapSourceConverter.ToBitmapSource(img);
            ImageWindow imageWindow = new ImageWindow(imageSource, img, fileName, shortFileName)
            {
                Title = windowTitle,
                Width = Math.Min(700, img.Width),
                Height = Math.Min(700, img.Height + 38),
            };

            imageWindows.Add(imageWindow);
            imageWindow.Closing += (s, e) => imageWindows.Remove(imageWindow);

            imageWindow.Show();
            return imageWindow;
        }

        private void UpdateSelectedImageMat(Mat imageMat, string fileName, string shortFileName)
        {
            this.selectedImageMat = imageMat;
            this.selectedImageFileName = fileName;
            this.selectedImageShortFileName = shortFileName;
            this.activeImageWindow = Application.Current.Windows
                .OfType<ImageWindow>()
                .FirstOrDefault(window => window.imageMat == imageMat);
            this.labelSelectedImage.Content = activeImageWindow?.Title;
        }

        private void ClearSelectedImageMat()
        {
            this.selectedImageMat = null;
            labelSelectedImage.Content = "Selected Image: Null";
        }

        private void ConvertAndSplitChannels(string conversionType)
        {
            if (this.selectedImageMat == null)
            {
                MessageBox.Show("No image selected");
                return;
            }
            if (this.selectedImageMat.NumberOfChannels < 3)
            {
                MessageBox.Show($"Conversion to {conversionType} and splitting channels can only be applied to images with at least 3 channels");
                return;
            }

            ImageWindow? imageWindowToClose = this.activeImageWindow;
            var channels = ImageOperarions.ConvertAndSplitRgb(this.selectedImageMat, conversionType);

            foreach (var channel in channels)
            {
                string windowTitle = $"{channel.channelName} {this.selectedImageShortFileName}";
                DisplayImageInNewWindow(channel.image, this.selectedImageFileName, windowTitle);
            }

            imageWindowToClose?.Close();
        }

    }
}
