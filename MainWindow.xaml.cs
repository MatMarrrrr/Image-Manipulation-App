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
                this.selectedImageMat = ImageOperarions.StretchContrast(this.selectedImageMat);
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

        private void DisplayImageInNewWindow(Mat img, string fileName)
        {
            string shortFileName = Path.GetFileName(fileName);
            string imageType = img.NumberOfChannels == 1 ? "GrayScale" : "Color";
            string windowTitle = $"({imageType}) {shortFileName}";
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
        }

        private void UpdateSelectedImageMat(Mat imageMat, string fileName)
        {
            this.selectedImageMat = imageMat;
            this.labelSelectedImage.Content = $"Selected Image: {fileName}";
            this.activeImageWindow = Application.Current.Windows
                .OfType<ImageWindow>()
                .FirstOrDefault(window => window.imageMat == imageMat);
        }

        private void ClearSelectedImageMat()
        {
            this.selectedImageMat = null;
            labelSelectedImage.Content = "Selected Image: Null";
        }

    }
}
