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

namespace APO_Mateusz_Marek_20456
{
    public partial class MainWindow : Window
    {
        public Mat? selectedImageMat;
        public ImageWindow? activeImageWindow;

        public MainWindow()
        {
            InitializeComponent();
            ImageWindow.ImageWindowFocused += UpdateSelectedImageMat;
            ImageWindow.ImageWindowClosing += ClearSelectedImageMat;
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

        private void OpenMonochrome_Click(object sender, RoutedEventArgs e)
        {
            OpenImage_Click(sender, e, false);
        }

        private void DisplayImageInNewWindow(Mat img, string fileName)
        {
            string shortFileName = Path.GetFileName(fileName);
            BitmapSource imageSource = BitmapSourceConverter.ToBitmapSource(img);
            ImageWindow imageWindow = new ImageWindow(imageSource, img, fileName, shortFileName)
            {
                Title = shortFileName,
                Width = Math.Min(700, img.Width),
                Height = Math.Min(700, img.Height + 38),
            };

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

        private void CreateHistogram_Click(object sender, RoutedEventArgs e)
        {
            if(this.selectedImageMat != null)
            {
                this.activeImageWindow?.ShowHistogram();
            }
            else
            {
                MessageBox.Show("No image selected");
            }
        }

        private void NegateImage()
        {
            unsafe
            {
                byte* dataPtr = (byte*)selectedImageMat.DataPointer;
                int width = selectedImageMat.Width;
                int height = selectedImageMat.Height;
                int step = selectedImageMat.Step;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        byte* pixelPtr = dataPtr + (y * step) + x;
                        *pixelPtr = (byte)(255 - *pixelPtr);
                    }
                }
            }

            activeImageWindow?.UpdateImage(this.selectedImageMat);
            activeImageWindow?.UpdateHistogram();
        }

        private void Negate_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat != null && this.selectedImageMat.NumberOfChannels == 1)
            {
                NegateImage();
            }
            else if (this.selectedImageMat != null)
            {
                MessageBox.Show("Negation can only be applied to grayscale images.");
            }
            else
            {
                MessageBox.Show("No image selected");
            }
        }

    }
}
