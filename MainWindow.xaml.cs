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
using System.Threading.Channels;
using System.Windows.Input;

namespace Image_Manipulation_App
{
    public partial class MainWindow : Window
    {
        private List<ImageWindow> imageWindows = new List<ImageWindow>();
        private List<String> imageWindowNames = new List<String>();
        public Mat? selectedImageMat;
        public string? selectedImageFileName;
        public string? selectedImageShortFileName;
        public ImageWindow? activeImageWindow;

        delegate Mat PointOperation(Mat image1, Mat image2);

        public MainWindow()
        {
            InitializeComponent();
            ImageWindow.ImageWindowFocused += UpdateSelectedImage;
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
                DisplayImageInNewWindow(img, fileName, null, true);
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
            if(this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            this.activeImageWindow.ShowHistogram();
        }

        private void ConvertToGrayScale_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }
            else if (this.selectedImageMat.NumberOfChannels == 1)
            {
                MessageBox.Show("Image is already gray scale");
                return;
            }

            this.selectedImageMat = ImageOperations.ConvertToGrayScale(this.selectedImageMat);
            activeImageWindow.UpdateImageAndHistogram(this.selectedImageMat);
            activeImageWindow.UpdateTitlePrefix("GrayScale");
            this.labelSelectedImage.Content = $"Selected Image: {activeImageWindow.Title}";
        }

        private void Negate_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            if (this.selectedImageMat.NumberOfChannels != 1)
            {
                MessageBox.Show("Negation can only be applied to grayscale images.");
                return;
            }

            this.selectedImageMat = ImageOperations.NegateImage(this.selectedImageMat);
            activeImageWindow.UpdateImageAndHistogram(this.selectedImageMat);
        }

        private void StretchContrast_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            if (this.selectedImageMat.NumberOfChannels != 1)
            {
                MessageBox.Show("Contrast stretching can only be applied to grayscale images.");
                return;
            }

            StretchContrastParamsWindow dialog = new StretchContrastParamsWindow();
            if (dialog.ShowDialog() == true)
            {
                int p1 = dialog.P1 ?? 0;
                int p2 = dialog.P2 ?? 255;
                int q3 = dialog.Q3 ?? 0;
                int q4 = dialog.Q4 ?? 255;

                this.selectedImageMat = ImageOperations.StretchContrast(this.selectedImageMat, (byte)p1, (byte)p2, (byte)q3, (byte)q4);
                activeImageWindow.UpdateImageAndHistogram(this.selectedImageMat);
            }
        }

        private void SplitChannels_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.selectedImageFileName == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            if (this.selectedImageMat.NumberOfChannels < 3)
            {
                MessageBox.Show("Splitting channels can only be applied to images with at least 3 channels");
                return;
            }

            var channels = ImageOperations.SplitChannels(this.selectedImageMat, "RGB");
            foreach (var channel in channels)
            {
                string windowTitle = $"{channel.channelName} {this.selectedImageShortFileName}";
                DisplayImageInNewWindow(channel.image, this.selectedImageFileName, windowTitle, true);
            }
        }

        private void ConvertToHSVAndSplitChannels_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.selectedImageFileName == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            if (this.selectedImageMat.NumberOfChannels < 3)
            {
                MessageBox.Show("Conversion to HSV and splitting channels can only be applied to images with at least 3 channels");
                return;
            }

            var hsvChannels = ImageOperations.ConvertAndSplitRgb(this.selectedImageMat, "HSV");

            foreach (var channel in hsvChannels)
            {
                string windowTitle = $"{channel.channelName} {this.selectedImageShortFileName}";
                DisplayImageInNewWindow(channel.image, this.selectedImageFileName, windowTitle, true);
            }
        }

        private void ConvertToLabAndSplitChannels_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.selectedImageFileName == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            if (this.selectedImageMat.NumberOfChannels < 3)
            {
                MessageBox.Show("Conversion to Lab and splitting channels can only be applied to images with at least 3 channels");
                return;
            }

            var labChannels = ImageOperations.ConvertAndSplitRgb(this.selectedImageMat, "Lab");

            foreach (var channel in labChannels)
            {
                string windowTitle = $"{channel.channelName} {this.selectedImageShortFileName}";
                DisplayImageInNewWindow(channel.image, this.selectedImageFileName, windowTitle, true);
            }
        }

        private void EqualizeHistogram_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            if (this.selectedImageMat.NumberOfChannels != 1)
            {
                MessageBox.Show("Histogram equalization can only be applied to grayscale images.");
                return;
            }

            this.selectedImageMat = ImageOperations.EqualizeHistogram(this.selectedImageMat);
            activeImageWindow.UpdateImageAndHistogram(this.selectedImageMat);
        }

        private void StretchHistogram_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            if (this.selectedImageMat.NumberOfChannels != 1)
            {
                MessageBox.Show("Histogram stretching can only be applied to grayscale images.");
                return;
            }

            this.selectedImageMat = ImageOperations.StretchHistogram(this.selectedImageMat);
            activeImageWindow.UpdateImageAndHistogram(this.selectedImageMat);
        }

        private void DuplicateImage_Click(object sender, RoutedEventArgs e)
        {
            this.DuplicateCurrentImage();
        }

        private void AddImages_Click(object sender, RoutedEventArgs e)
        {
            PerformPointOperation("Add", ImageOperations.AddImages);
        }

        private void SubtractImages_Click(object sender, RoutedEventArgs e)
        {
            PerformPointOperation("Subtract", ImageOperations.SubtractImages);
        }

        private void Posterize_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            if (this.selectedImageMat.NumberOfChannels != 1)
            {
                MessageBox.Show("Posterization can only be applied to grayscale images.");
                return;
            }

            PosterizationParamsWindow dialog = new PosterizationParamsWindow();
            if (dialog.ShowDialog() == true)
            {
                int levels = dialog.levels ?? 2;
                this.selectedImageMat = ImageOperations.PosterizeImage(this.selectedImageMat, levels);
                activeImageWindow.UpdateImageAndHistogram(this.selectedImageMat);
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Image Manipulation App{Environment.NewLine}{Environment.NewLine}Created by: Mateusz Marek");
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            var windowsToClose = imageWindows.ToList();
            foreach (var window in windowsToClose)
            {
                window.Close();
            }
        }

        private void PerformPointOperation(string operationName, PointOperation operation)
        {
            int countGrayScaleImages = imageWindows.Count(window => window?.imageMat?.NumberOfChannels == 1);

            if (countGrayScaleImages < 2)
            {
                MessageBox.Show("You must have at least two greyscale images to perform math operations");
                return;
            }

            MathOperationParamsWindow dialog = new MathOperationParamsWindow(this.imageWindows, $"{operationName} images window", operationName);
            if (dialog.ShowDialog() == true)
            {
                ImageWindow window1 = imageWindows[dialog.FirstImageIndex];
                ImageWindow window2 = imageWindows[dialog.SecondImageIndex];

                if (window1?.imageMat != null && window2?.imageMat != null)
                {
                    Mat image1 = window1.imageMat;
                    Mat image2 = window2.imageMat;
                    if (image1 != null && image2 != null)
                    {
                        this.selectedImageMat = operation(image1, image2);
                        DisplayImageInNewWindow(this.selectedImageMat, $"{operationName} {window1.shortFileName}, {window2.shortFileName}", null, true);
                    }
                }
            }
        }

        private ImageWindow DisplayImageInNewWindow(Mat img, string fileName, string? customWindowTitle = null, bool checkDuplicates = false)
        {
            string shortFileName = Path.GetFileName(fileName);
            string windowTitle = "";

            string imageType = img.NumberOfChannels == 1 ? "GrayScale" : "Color";

            if (customWindowTitle != null)
            {
                windowTitle = customWindowTitle;
            }
            else
            {
                windowTitle = $"({imageType}) {shortFileName}";
            }

            if (checkDuplicates)
            {
                int countNameDuplicates = this.imageWindowNames.Count(name => name == shortFileName);
                if (countNameDuplicates > 0)
                {
                    windowTitle += $" -{countNameDuplicates}";
                }
            }

            BitmapSource imageSource = BitmapSourceConverter.ToBitmapSource(img);
            ImageWindow imageWindow = new ImageWindow(imageSource, img, fileName, shortFileName)
            {
                Title = windowTitle,
                Width = Math.Min(500, img.Width),
                Height = Math.Min(500, img.Height + 38),
            };

            this.imageWindows.Add(imageWindow);
            this.imageWindowNames.Add(shortFileName);
            imageWindow.Closing += (s, e) => imageWindows.Remove(imageWindow);
            imageWindow.Closing += (s, e) => imageWindowNames.Remove(shortFileName);
            imageWindow.KeyDown += Window_KeyDown;

            imageWindow.Show();
            return imageWindow;
        }

        private void UpdateSelectedImage(ImageWindow imageWindow, Mat imageMat, string fileName, string shortFileName)
        {
            this.selectedImageMat = imageMat;
            this.selectedImageFileName = fileName;
            this.selectedImageShortFileName = shortFileName;
            this.activeImageWindow = imageWindow;
            this.labelSelectedImage.Content = $"Selected Image: {activeImageWindow?.Title}";
        }

        private void ClearSelectedImageMat()
        {
            this.selectedImageMat = null;
            labelSelectedImage.Content = "Selected Image: None";
        }

        private void DuplicateCurrentImage()
        {
            if (this.selectedImageMat == null || this.selectedImageFileName == null)
            {
                MessageBox.Show("No image selected");
                return;
            }
            DisplayImageInNewWindow(this.selectedImageMat, this.selectedImageFileName, null, true);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl + Shift + D => Duplicate Image
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
            {
                if (e.KeyboardDevice.IsKeyDown(Key.LeftShift) || e.KeyboardDevice.IsKeyDown(Key.RightShift))
                {
                    if (e.Key == Key.D)
                    {
                        this.DuplicateCurrentImage();
                    }
                }
            }


        }

    }
}
