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
using static System.Net.Mime.MediaTypeNames;
using Image_Manipulation_App.ParamWindows;
using static SkiaSharp.HarfBuzz.SKShaper;
using static Image_Manipulation_App.ImageOperations;

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
        delegate Mat PointOperationWithAlpha(Mat image1, Mat image2, double alpha);

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
            if (this.selectedImageMat == null || this.activeImageWindow == null)
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
            this.PerformTwoMatOperation("Add", ImageOperations.AddImages);
        }

        private void SubtractImages_Click(object sender, RoutedEventArgs e)
        {
            this.PerformTwoMatOperation("Subtract", ImageOperations.SubtractImages);
        }

        private void BlendImages_Click(object sender, RoutedEventArgs e)
        {
            this.PerformTwoMatOperation("Blend", ImageOperations.BlendImages);
        }

        private void AndImages_Click(object sender, RoutedEventArgs e)
        {
            this.PerformTwoMatOperation("AND", ImageOperations.AndImages);
        }

        private void OrImages_Click(object sender, RoutedEventArgs e)
        {
            this.PerformTwoMatOperation("OR", ImageOperations.OrImages);
        }

        private void NotImage_Click(object sender, RoutedEventArgs e)
        {
            this.Negate_Click(sender, e);
        }

        private void XorImages_Click(object sender, RoutedEventArgs e)
        {
            this.PerformTwoMatOperation("XOR", ImageOperations.XorImages);
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

            OneParamWindow dialog = new OneParamWindow("Posterization params", "Number of levels", "Posterize", 2, 255, "int");
            if (dialog.ShowDialog() == true)
            {
                int levels = dialog.IntParam;
                this.selectedImageMat = ImageOperations.PosterizeImage(this.selectedImageMat, levels);
                activeImageWindow.UpdateImageAndHistogram(this.selectedImageMat);
            }
        }

        private void Blur_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            if (this.selectedImageMat.NumberOfChannels != 1)
            {
                MessageBox.Show("Blur can only be applied to grayscale images.");
                return;
            }

            BorderPixelsHandlingMethodParamWindow dialog = new BorderPixelsHandlingMethodParamWindow("Blur border handling method", "Apply");
            if (dialog.ShowDialog() == true)
            {
                BorderType borderType = dialog.borderMethod;
                Mat blurredImage = new Mat();
                CvInvoke.Blur(this.selectedImageMat, blurredImage, new System.Drawing.Size(5, 5), new System.Drawing.Point(-1, -1), borderType);
                this.selectedImageMat = blurredImage;
                activeImageWindow.UpdateImageAndHistogram(this.selectedImageMat);
            }
        }

        private void GaussianBlur_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            if (this.selectedImageMat.NumberOfChannels != 1)
            {
                MessageBox.Show("Gaussian blur can only be applied to grayscale images.");
                return;
            }

            BorderPixelsHandlingMethodParamWindow dialog = new BorderPixelsHandlingMethodParamWindow("Gaussian Blur border handling method", "Apply");
            if (dialog.ShowDialog() == true)
            {
                BorderType borderType = dialog.borderMethod;
                Mat gaussianBlurredImage = new Mat();
                CvInvoke.GaussianBlur(this.selectedImageMat, gaussianBlurredImage, new System.Drawing.Size(5, 5), 1.5, 0, borderType);
                this.selectedImageMat = gaussianBlurredImage;
                activeImageWindow.UpdateImageAndHistogram(this.selectedImageMat);
            }
        }

        private void Convolve_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            if (this.selectedImageMat.NumberOfChannels != 1)
            {
                MessageBox.Show("Convolutions can only be applied to grayscale images.");
                return;
            }

            ConvolverWindow dialog = new ConvolverWindow(this.selectedImageMat);
            if (dialog.ShowDialog() == true && dialog.result != null)
            {
                this.selectedImageMat = dialog.result;
                activeImageWindow.UpdateImageAndHistogram(this.selectedImageMat);
            }
        }

        private void MedianFilter_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            if (this.selectedImageMat.NumberOfChannels != 1)
            {
                MessageBox.Show("Median filtration can only be applied to grayscale images.");
                return;
            }

            MedianFIlterParamsWindow dialog = new MedianFIlterParamsWindow();
            if (dialog.ShowDialog() == true)
            {
                int kernelSize = dialog.kernelSize;
                BorderType borderType = dialog.borderMethod;
                Mat filteredImage = new Mat();
                CvInvoke.MedianBlur(this.selectedImageMat, filteredImage, kernelSize);
                this.selectedImageMat = filteredImage;
                activeImageWindow.UpdateImageAndHistogram(this.selectedImageMat);
            }
        }

        private void SingleAndTwoStage_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null || this.selectedImageFileName == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            if (this.selectedImageMat.NumberOfChannels != 1)
            {
                MessageBox.Show("Single and two stage filtration can only be applied to grayscale images.");
                return;
            }

            SingleAndTwoStageFiltrationParamsWindow dialog = new SingleAndTwoStageFiltrationParamsWindow();
            if (dialog.ShowDialog() == true)
            {
                int[] firstKernelArray = dialog.firstKernel;
                int[] secondKernelArray = dialog.secondKernel;
                BorderType borderMethod = dialog.borderMethod;

                Matrix<float> firstKernel = new Matrix<float>(3, 3);
                Matrix<float> secondKernel = new Matrix<float>(3, 3);
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        firstKernel[i, j] = firstKernelArray[i * 3 + j];
                        secondKernel[i, j] = secondKernelArray[i * 3 + j];
                    }
                }

                Mat firstStageResult = new Mat(this.selectedImageMat.Size, this.selectedImageMat.Depth, this.selectedImageMat.NumberOfChannels);

                double firstKernelSum = CvInvoke.Sum(firstKernel).V0;
                if (firstKernelSum != 0)
                {
                    firstKernel *= (1.0 / firstKernelSum);
                }
                CvInvoke.Filter2D(this.selectedImageMat, firstStageResult, firstKernel, new System.Drawing.Point(-1, -1), 0, borderMethod);

                Mat secondStageResult = new Mat(this.selectedImageMat.Size, this.selectedImageMat.Depth, this.selectedImageMat.NumberOfChannels);

                double secondKernelSum = CvInvoke.Sum(secondKernel).V0;
                if (secondKernelSum != 0)
                {
                    secondKernel *= (1.0 / secondKernelSum);
                }
                CvInvoke.Filter2D(firstStageResult, secondStageResult, secondKernel, new System.Drawing.Point(-1, -1), 0, borderMethod);

                Matrix<float> combinedKernel = ImageOperations.ConvolveKernels(firstKernel, secondKernel);
                double combinedKernelSum = CvInvoke.Sum(combinedKernel).V0;
                if (combinedKernelSum != 0)
                {
                    combinedKernel *= (1.0 / combinedKernelSum);
                }

                Mat singleStageResult = new Mat(this.selectedImageMat.Size, this.selectedImageMat.Depth, this.selectedImageMat.NumberOfChannels);
                CvInvoke.Filter2D(this.selectedImageMat, singleStageResult, combinedKernel, new System.Drawing.Point(-1, -1), 0, borderMethod);

                //Mat firstStageResult = new Mat(this.selectedImageMat.Size, this.selectedImageMat.Depth, this.selectedImageMat.NumberOfChannels);
                //CvInvoke.Filter2D(this.selectedImageMat, firstStageResult, firstKernel, new System.Drawing.Point(-1, -1), 0, borderMethod);

                //Mat secondStageResult = new Mat(this.selectedImageMat.Size, this.selectedImageMat.Depth, this.selectedImageMat.NumberOfChannels);
                //CvInvoke.Filter2D(firstStageResult, secondStageResult, secondKernel, new System.Drawing.Point(-1, -1), 0, borderMethod);

                //Matrix<float> combinedKernel = ImageOperations.ConvolveKernels(firstKernel, secondKernel);

                //Mat singleStageResult = new Mat(this.selectedImageMat.Size, this.selectedImageMat.Depth, this.selectedImageMat.NumberOfChannels);
                //CvInvoke.Filter2D(this.selectedImageMat, singleStageResult, combinedKernel, new System.Drawing.Point(-1, -1), 0, borderMethod);

                DisplayImageInNewWindow(secondStageResult, this.selectedImageFileName, $"Two 3x3 Result {this.selectedImageShortFileName}", true);
                DisplayImageInNewWindow(singleStageResult, this.selectedImageFileName, $"One 5x5 Result {this.selectedImageShortFileName}", true);
            }
        }

        private void ErodeImage_Click(object sender, EventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            if (this.selectedImageMat.NumberOfChannels != 1)
            {
                MessageBox.Show("Blur can only be applied to grayscale images.");
                return;
            }

            StructureSizeParamWindow dialog = new StructureSizeParamWindow("Erode params window", "Erode");
            if (dialog.ShowDialog() == true)
            {
                Mat structureElement = ImageOperations.GetStructuralElement(dialog.structure, dialog.size);
                this.selectedImageMat = ImageOperations.Erode(this.selectedImageMat, structureElement);
                activeImageWindow.UpdateImageAndHistogram(this.selectedImageMat);
            }
        }
        private void DilateImage_Click(object sender, EventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            if (this.selectedImageMat.NumberOfChannels != 1)
            {
                MessageBox.Show("Dilate can only be applied to grayscale images.");
                return;
            }

            StructureSizeParamWindow dialog = new StructureSizeParamWindow("Dilate params window", "Dilate");
            if (dialog.ShowDialog() == true)
            {
                Mat structureElement = ImageOperations.GetStructuralElement(dialog.structure, dialog.size);
                this.selectedImageMat = ImageOperations.Dilate(this.selectedImageMat, structureElement);
                activeImageWindow.UpdateImageAndHistogram(this.selectedImageMat);
            }
        }

        private void OpenImage_Click(object sender, EventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            if (this.selectedImageMat.NumberOfChannels != 1)
            {
                MessageBox.Show("Open can only be applied to grayscale images.");
                return;
            }

            StructureSizeParamWindow dialog = new StructureSizeParamWindow("Open params window", "Open");
            if (dialog.ShowDialog() == true)
            {
                Mat structureElement = ImageOperations.GetStructuralElement(dialog.structure, dialog.size);
                this.selectedImageMat = ImageOperations.Open(this.selectedImageMat, structureElement);
                activeImageWindow.UpdateImageAndHistogram(this.selectedImageMat);
            }
        }

        private void CloseImage_Click(object sender, EventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            if (this.selectedImageMat.NumberOfChannels != 1)
            {
                MessageBox.Show("Close can only be applied to grayscale images.");
                return;
            }

            StructureSizeParamWindow dialog = new StructureSizeParamWindow("Close params window", "Close");
            if (dialog.ShowDialog() == true)
            {
                Mat structureElement = ImageOperations.GetStructuralElement(dialog.structure, dialog.size);
                this.selectedImageMat = ImageOperations.Close(this.selectedImageMat, structureElement);
                activeImageWindow.UpdateImageAndHistogram(this.selectedImageMat);
            }
        }

        private void SkeletizeImage_Click(object sender, EventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            if (this.selectedImageMat.NumberOfChannels != 1)
            {
                MessageBox.Show("Skeletization can only be applied to grayscale images.");
                return;
            }

            StructureSizeParamWindow dialog = new StructureSizeParamWindow("Skeletization params window", "Skeletize");
            if (dialog.ShowDialog() == true)
            {
                Mat structureElement = ImageOperations.GetStructuralElement(dialog.structure, dialog.size);
                this.selectedImageMat = ImageOperations.Skeletonize(this.selectedImageMat, structureElement);
                activeImageWindow.UpdateImageAndHistogram(this.selectedImageMat);
            }
        }

        private void ImagePyramidUp_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            Mat result = ImageOperations.ImagePyramidUp(this.selectedImageMat);
            this.selectedImageMat = result;
            activeImageWindow.UpdateImageAndHistogram(this.selectedImageMat);
        }

        private void ImagePyramidDown_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            Mat result = ImageOperations.ImagePyramidDown(this.selectedImageMat);
            this.selectedImageMat = result;
            activeImageWindow.UpdateImageAndHistogram(this.selectedImageMat);
        }

        private void Hough_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            if (this.selectedImageMat.NumberOfChannels != 1)
            {
                MessageBox.Show("Hough transform can only be applied to grayscale images.");
                return;
            }

            Mat result = ImageOperations.ApplyHoughTransform(this.selectedImageMat);
            this.selectedImageMat = result;
            activeImageWindow.UpdateImageAndHistogram(this.selectedImageMat);
        }

        private void ProfileLineOn_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            if (this.selectedImageMat.NumberOfChannels != 1)
            {
                MessageBox.Show("Profile line can only be created for grayscale images.");
                return;
            }

            this.activeImageWindow.isProfileLine = true;
            this.profileLineMenuItem.Header = "Profile Line (On)";
        }

        private void ProfileLineOff_Click(object sender, RoutedEventArgs e)
        {
            if (this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            this.activeImageWindow.isProfileLine = false;
            this.activeImageWindow.profileLineWindow?.Close();
            this.profileLineMenuItem.Header = "Profile Line (Off)";
            this.activeImageWindow.profileLineWindow = null;
            this.activeImageWindow.RestoreImage();
        }

        private void Threshold_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            if (this.selectedImageMat.NumberOfChannels != 1)
            {
                MessageBox.Show("Thresholding can only be applied for grayscale images.");
                return;
            }

            ThresholdWindow thresholdWindow = new ThresholdWindow(this.selectedImageMat);
            if(thresholdWindow.ShowDialog() == true)
            {
                this.selectedImageMat = thresholdWindow.finalImage;
                activeImageWindow.UpdateImageAndHistogram(this.selectedImageMat);
            }
        }

        private void Inpainting_Click(object sender, RoutedEventArgs e)
        {
            this.PerformTwoMatOperation("Inpaint", ImageOperations.PerformInpainting);
        }

        private void GrabCut_Click(object sender, RoutedEventArgs e)
        {
            this.PerformTwoMatOperation("GrabCut", ImageOperations.PerformGrabCutWithMask);
        }

        private void Watershed_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            Mat result = ImageOperations.ApplyWatershed(this.selectedImageMat);
            this.selectedImageMat = result;
            activeImageWindow.UpdateImageAndHistogram(this.selectedImageMat);
        }

        private void AnalyzeImage_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            Tuple<Mat, List<AnalysisResult>> result = ImageOperations.AnalyzeImage(this.selectedImageMat);
            this.selectedImageMat = result.Item1;
            activeImageWindow.UpdateImageAndHistogram(this.selectedImageMat);

            AnalyzedImageDataWindow dataWindow = new AnalyzedImageDataWindow(result.Item2);
            dataWindow.Show();
        }


        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            ImageOperations.SaveImage(this.selectedImageMat);
        }

        private void SaveAsCompression_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }
            (Mat compressed, double SK) = ImageOperations.CompressImage(this.selectedImageMat);
            MessageBox.Show($"Stopeń kompresji SK: {SK}");
            ImageOperations.SaveCompressedImage(compressed);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                $"Aplikacja zbiorcza z ćwiczeń laboratoryjnych i projektu" +
                $"{Environment.NewLine}" +
                $"Tytuł projektu: Image Manipulation App" +
                $"{Environment.NewLine}" +
                $"Autor: Mateusz Marek" +
                $"{Environment.NewLine}" +
                $"Prowadzący: mgr inż. Łukasz Roszkowiak" +
                $"{Environment.NewLine}" +
                $"Algorytmy Przetwarzania Obrazów 2024" +
                $"{Environment.NewLine}" +
                $"WIT grupa ID06IO01"
                );
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            var windowsToClose = imageWindows.ToList();
            foreach (var window in windowsToClose)
            {
                window.Close();
            }
        }

        private void PerformTwoMatOperation(string operationName, PointOperation operation)
        {
            int countGrayScaleImages = imageWindows.Count(window => window?.imageMat?.NumberOfChannels == 1);

            if (countGrayScaleImages < 2)
            {
                MessageBox.Show("You must have at least two greyscale images to perform math operations");
                return;
            }

            PointOperationParamsWindow dialog = new PointOperationParamsWindow(this.imageWindows, $"{operationName} images window", operationName);
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

        private void PerformTwoMatOperation(string operationName, PointOperationWithAlpha operation)
        {
            int countGrayScaleImages = imageWindows.Count(window => window?.imageMat?.NumberOfChannels == 1);

            if (countGrayScaleImages < 2)
            {
                MessageBox.Show("You must have at least two greyscale images to perform math operations");
                return;
            }

            PointOperationParamsWindow dialog = new PointOperationParamsWindow(this.imageWindows, $"{operationName} images window", operationName);
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
                        OneParamWindow dialog_ = new OneParamWindow("Blending params", "Alpha value:", "Blend", 0, 1, "double");
                        if (dialog_.ShowDialog() == true)
                        {
                            double alpha = dialog_.DoubleParam;
                            this.selectedImageMat = operation(image1, image2, alpha);
                            DisplayImageInNewWindow(this.selectedImageMat, $"{operationName} {window1.shortFileName}, {window2.shortFileName}", null, true);
                        }

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

        private void UpdateSelectedImage(ImageWindow imageWindow, Mat imageMat, string fileName, string shortFileName, bool isProfileLine)
        {
            this.selectedImageMat = imageMat;
            this.selectedImageFileName = fileName;
            this.selectedImageShortFileName = shortFileName;
            this.activeImageWindow = imageWindow;
            this.labelSelectedImage.Content = $"Selected Image: {activeImageWindow?.Title}";
            this.profileLineMenuItem.Header = isProfileLine ? "Profile Line (On)" : "Profile Line (Off)";
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

        private void RgbColorThreshold_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            if (this.selectedImageMat.NumberOfChannels != 3)
            {
                MessageBox.Show("Color thresholding can only be applied for color images.");
                return;
            }

            ColorThresholdWindow thresholdWindow = new ColorThresholdWindow(this.selectedImageMat);
            if (thresholdWindow.ShowDialog() == true)
            {
                string title = $"Binary {this.selectedImageFileName}";
                DisplayImageInNewWindow(thresholdWindow.binaryImage, title, null, true);
            }
        }

        private void HsvColorThreshold_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            if (this.selectedImageMat.NumberOfChannels != 3)
            {
                MessageBox.Show("Color thresholding can only be applied for color images.");
                return;
            }

            ColorThresholdWindow thresholdWindow = new ColorThresholdWindow(this.selectedImageMat, "hsv");
            if (thresholdWindow.ShowDialog() == true)
            {
                string title = $"Binary {this.selectedImageFileName}";
                DisplayImageInNewWindow(thresholdWindow.binaryImage, title, null, true);
            }
        }

        private void LabColorThreshold_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            if (this.selectedImageMat.NumberOfChannels != 3)
            {
                MessageBox.Show("Color thresholding can only be applied for color images.");
                return;
            }

            ColorThresholdWindow thresholdWindow = new ColorThresholdWindow(this.selectedImageMat, "lab");
            if (thresholdWindow.ShowDialog() == true)
            {
                string title = $"Binary {this.selectedImageFileName}";
                DisplayImageInNewWindow(thresholdWindow.binaryImage, title, null, true);
            }
        }

        private void CountWhiteBinaryMap_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            if (this.selectedImageMat.NumberOfChannels != 1)
            {
                MessageBox.Show("Counting objects can only be done on a binary map");
                return;
            }

            bool isBinary = true;

            IntPtr dataPtr = this.selectedImageMat.DataPointer;
            int width = this.selectedImageMat.Width;
            int height = this.selectedImageMat.Height;
            int step = this.selectedImageMat.Step;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = (y * step) + x;
                    byte pixelValue = Marshal.ReadByte(dataPtr, offset);
                    if (pixelValue != 0 && pixelValue != 255)
                    {
                        isBinary = false;
                        break;
                    }
                }
            }

            if (!isBinary)
            {
                MessageBox.Show("Counting objects can only be done on a binary map");
                return;
            }

            MessageBox.Show($"Object count on binary map: {ImageOperations.CountObjectsUsingContours(this.selectedImageMat)}");
        }

        private void CountBlackBinaryMap_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedImageMat == null || this.activeImageWindow == null)
            {
                MessageBox.Show("No image selected");
                return;
            }

            if (this.selectedImageMat.NumberOfChannels != 1)
            {
                MessageBox.Show("Counting objects can only be done on a binary map");
                return;
            }

            bool isBinary = true;

            IntPtr dataPtr = this.selectedImageMat.DataPointer;
            int width = this.selectedImageMat.Width;
            int height = this.selectedImageMat.Height;
            int step = this.selectedImageMat.Step;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = (y * step) + x;
                    byte pixelValue = Marshal.ReadByte(dataPtr, offset);
                    if (pixelValue != 0 && pixelValue != 255)
                    {
                        isBinary = false;
                        break;
                    }
                }
            }

            if (!isBinary)
            {
                MessageBox.Show("Counting objects can only be done on a binary map");
                return;
            }

            Mat invertedImage = new Mat(this.selectedImageMat.Size, DepthType.Cv8U, 1);
            CvInvoke.BitwiseNot(this.selectedImageMat, invertedImage);

            MessageBox.Show($"Object count on binary map: {ImageOperations.CountObjectsUsingContours(invertedImage)}");
        }
    }
}
