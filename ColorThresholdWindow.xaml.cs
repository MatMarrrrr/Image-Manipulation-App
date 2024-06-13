using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace Image_Manipulation_App
{
    /// <summary>
    /// Interaction logic for ColorThresholdWindow.xaml
    /// </summary>
    public partial class ColorThresholdWindow : Window
    {
        public Mat binaryImage = new Mat();

        private Mat firstChannel = new Mat();
        private Mat secondChannel = new Mat();
        private Mat thirdChannel = new Mat();

        private Mat firstThresholdChannel = new Mat();
        private Mat secondThresholdChannel = new Mat();
        private Mat thirdThresholdChannel = new Mat();

        private Mat originalImage = new Mat();
        private Mat thresholdedImage = new Mat();

        private bool isWhiteObjects = true;
        private string colorSpace = "rgb";

        /// <summary>
        /// Constructor to initialize the ColorThresholdWindow.
        /// </summary>
        /// <param name="image">The base image to be processed.</param>
        public ColorThresholdWindow(Mat image, string type = "rgb")
        {
            InitializeComponent();

            this.originalImage = image.Clone();
            this.colorSpace = type.ToLower();

            Mat convertedImage = new Mat();
            if (type.ToLower() == "hsv")
            {
                this.firstChannelTextBlock.Text = "Hue channel";
                this.secondChannelTextBlock.Text = "Saturation channel";
                this.thirdChannelTextBlock.Text = "Value channel";
                CvInvoke.CvtColor(image, convertedImage, Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);
            }
            else if (type.ToLower() == "lab")
            {
                this.firstChannelTextBlock.Text = "L(Lightness) channel";
                this.secondChannelTextBlock.Text = "a(Green-Red Component) channel";
                this.thirdChannelTextBlock.Text = "b(Blue-Yellow Component) channel";
                CvInvoke.CvtColor(image, convertedImage, Emgu.CV.CvEnum.ColorConversion.Bgr2Lab);
            }
            else
            {
                convertedImage = image.Clone();
            }

            this.processedImage.Source = BitmapSourceConverter.ToBitmapSource(convertedImage);

            List<Mat> channels = SplitChannels(convertedImage);

            thirdChannel = channels[0];
            secondChannel = channels[1];
            firstChannel = channels[2];

            thirdThresholdChannel = channels[0];
            secondThresholdChannel = channels[1];
            firstThresholdChannel = channels[2];

            DrawHistogram(firstChannel, firstChannelHistogramImage);
            DrawHistogram(secondChannel, secondChannelHistogramImage);
            DrawHistogram(thirdChannel, thirdChannelHistogramImage);

            this.Loaded += (s, e) =>
            {
                UpdateThresholdLines();
                UpdateThresholdPreview();
            };
        }

        #region Methods
        /// <summary>
        /// Splits the input image into its color channels.
        /// </summary>
        /// <param name="image">The input image.</param>
        /// <returns>A list containing the three color channels (BGR).</returns>
        public List<Mat> SplitChannels(Mat image)
        {
            List<Mat> channels = new List<Mat>();

            VectorOfMat vector = new VectorOfMat();
            CvInvoke.Split(image, vector);

            for (int i = 0; i < vector.Size; i++)
            {
                Mat channel = vector[i];
                Mat grayChannel = channel.Clone();
                channels.Add(grayChannel);
            }

            return channels;
        }

        /// <summary>
        /// Draws a histogram for the specified color channel.
        /// </summary>
        /// <param name="channel">The color channel.</param>
        /// <param name="imageControl">The image control where the histogram will be displayed.</param>
        public void DrawHistogram(Mat channel, Image imageControl)
        {
            int[] histogramData = CalculateHistogram(channel); // Calculate histogram data
            int histHeight = 256; // Height of the histogram image
            int histWidth = 256; // Width of the histogram image
            int binWidth = (int)Math.Floor((double)histWidth / 256); // Width of each bin in the histogram

            Mat histImage = new Mat(histHeight, histWidth, DepthType.Cv8U, 1); // Create a Mat to hold the histogram image
            histImage.SetTo(new MCvScalar(255)); // Set background to white

            int maxVal = histogramData.Max(); // Find the maximum value in the histogram data
            for (int i = 0; i < 256; i++)
            {
                int intensity = (int)(histogramData[i] * histHeight / (double)maxVal); // Normalize intensity
                // Draw a line for each bin in the histogram
                CvInvoke.Line(histImage, new System.Drawing.Point(i * binWidth, histHeight), new System.Drawing.Point(i * binWidth, histHeight - intensity), new MCvScalar(0), 1);
            }

            imageControl.Source = BitmapSourceConverter.ToBitmapSource(histImage); // Display the histogram image in the UI
        }

        /// <summary>
        /// Calculates the histogram data for the given image channel.
        /// </summary>
        /// <param name="imageMat">The image matrix representing the color channel.</param>
        /// <returns>An array representing the histogram data.</returns>
        public int[] CalculateHistogram(Mat imageMat)
        {
            int[] histogramData = new int[256];

            IntPtr dataPtr = imageMat.DataPointer;
            int width = imageMat.Width;
            int height = imageMat.Height;
            int step = imageMat.Step;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = y * step + x;
                    byte intensity = Marshal.ReadByte(dataPtr + offset);
                    histogramData[intensity]++;
                }
            }

            return histogramData;
        }

        /// <summary>
        /// Applies thresholding to an image based on the given lower and upper bounds.
        /// </summary>
        /// <param name="image">The image to be thresholded.</param>
        /// <param name="lower">The lower bound of the threshold.</param>
        /// <param name="upper">The upper bound of the threshold.</param>
        /// <param name="minRange">The minimum range of the channel.</param>
        /// <param name="maxRange">The maximum range of the channel.</param>
        /// <returns>The thresholded image.</returns>
        private Mat ThresholdImage(Mat image, int lower, int upper, int minRange, int maxRange, bool normalized = true)
        {
            int adjustedLower = lower;
            int adjustedUpper = upper;

            if (!normalized)
            {
                adjustedLower = (int)(((double)(lower - 0) / (255 - 0)) * (maxRange - minRange) + minRange);
                adjustedUpper = (int)(((double)(upper - 0) / (255 - 0)) * (maxRange - minRange) + minRange);
            }

            Mat thresholdedImage = new Mat(image.Size, DepthType.Cv8U, 1);
            IntPtr dataPtr = image.DataPointer;
            IntPtr outputPtr = thresholdedImage.DataPointer;
            int width = image.Width;
            int height = image.Height;
            int step = image.Step;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = (y * step) + x;
                    byte pixelValue = Marshal.ReadByte(dataPtr, offset);
                    byte thresholdedPixelValue = (pixelValue >= adjustedLower && pixelValue <= adjustedUpper) ? (byte)255 : (byte)0;
                    Marshal.WriteByte(outputPtr, offset, thresholdedPixelValue);
                }
            }

            return thresholdedImage;
        }

        /// <summary>
        /// Combines the thresholded red, green, and blue channels into a binary map.
        /// </summary>
        /// <param name="redChannel">The red channel.</param>
        /// <param name="greenChannel">The green channel.</param>
        /// <param name="blueChannel">The blue channel.</param>
        /// <returns>The combined binary map.</returns>
        public Mat CombineChannelsToBinaryMap(Mat redChannel, Mat greenChannel, Mat blueChannel)
        {
            Mat binaryMap = new Mat();
            CvInvoke.BitwiseAnd(redChannel, greenChannel, binaryMap);
            CvInvoke.BitwiseAnd(binaryMap, blueChannel, binaryMap);
            return binaryMap;
        }

        /// <summary>
        /// Inverts a binary image (255 becomes 0 and 0 becomes 255).
        /// </summary>
        /// <param name="binaryImage">The binary image to be inverted.</param>
        /// <returns>The inverted binary image.</returns>
        private Mat InvertBinaryImage(Mat binaryImage)
        {
            Mat invertedImage = new Mat(binaryImage.Size, binaryImage.Depth, binaryImage.NumberOfChannels);
            CvInvoke.BitwiseNot(binaryImage, invertedImage);
            return invertedImage;
        }

        /// <summary>
        /// Updates the threshold sliders to ensure threshold1 is always less than or equal to threshold2.
        /// </summary>
        /// <param name="threshold1">The first threshold slider.</param>
        /// <param name="threshold2">The second threshold slider.</param>
        /// <param name="type">The type of comparison ('more' or 'less').</param>
        private void UpdateThresholdSliders(Slider threshold1, Slider threshold2, String type)
        {
            if (type == "more" && threshold1.Value > threshold2.Value)
            {
                threshold2.Value = threshold1.Value;
            }

            if (type == "less" && threshold2.Value < threshold1.Value)
            {
                threshold1.Value = threshold2.Value;
            }
        }

        /// <summary>
        /// Updates the positions of the threshold lines based on the current values of the sliders.
        /// </summary>
        private void UpdateThresholdLines()
        {
            double canvasWidth = 380.0;
            double marginMultiplier = canvasWidth / 255.0;

            Canvas.SetLeft(RedLine1, FirstChannelThreshold1.Value * marginMultiplier);
            Canvas.SetLeft(RedLine2, FirstChannelThreshold2.Value * marginMultiplier);

            Canvas.SetLeft(GreenLine1, SecondChannelThreshold1.Value * marginMultiplier);
            Canvas.SetLeft(GreenLine2, SecondChannelThreshold2.Value * marginMultiplier);

            Canvas.SetLeft(BlueLine1, ThirdChannelThreshold1.Value * marginMultiplier);
            Canvas.SetLeft(BlueLine2, ThirdChannelThreshold2.Value * marginMultiplier);
        }

        /// <summary>
        /// Updates the text labels to display the current threshold values.
        /// </summary>
        private void UpdateThresholdLabels()
        {
            this.RedThresholdText.Text = $"Choosen range: {FirstChannelThreshold1.Value} - {FirstChannelThreshold2.Value}";
            this.GreenThresholdText.Text = $"Choosen range: {SecondChannelThreshold1.Value} - {SecondChannelThreshold2.Value}";
            this.BlueThresholdText.Text = $"Choosen range: {ThirdChannelThreshold1.Value} - {ThirdChannelThreshold2.Value}";
        }

        /// <summary>
        /// Updates the threshold preview by applying the thresholds to the color channels
        /// and combining them into a binary map.
        /// </summary>
        private void UpdateThresholdPreview()
        {

            int[] minRanges = { 0, 0, 0 };
            int[] maxRanges = { 255, 255, 255 };
            if (colorSpace == "hsv")
            {
                minRanges = new int[] { 0, 0, 0 };
                maxRanges = new int[] { 360, 100, 100 };
            }
            else if (colorSpace == "lab")
            {
                minRanges = new int[] { 0, -128, -128 };
                maxRanges = new int[] { 100, 127, 127 };
            }

            this.firstThresholdChannel = ThresholdImage(this.firstChannel, (int)FirstChannelThreshold1.Value, (int)FirstChannelThreshold2.Value, minRanges[0], maxRanges[0]);
            this.secondThresholdChannel = ThresholdImage(this.secondChannel, (int)SecondChannelThreshold1.Value, (int)SecondChannelThreshold2.Value, minRanges[1], maxRanges[1]);
            this.thirdThresholdChannel = ThresholdImage(this.thirdChannel, (int)ThirdChannelThreshold1.Value, (int)ThirdChannelThreshold2.Value, minRanges[2], maxRanges[2]);

            Mat binaryMap = CombineChannelsToBinaryMap(this.firstThresholdChannel, this.secondThresholdChannel, this.thirdThresholdChannel);

            if (!isWhiteObjects)
            {
                binaryMap = InvertBinaryImage(binaryMap);
            }

            this.processedImage.Source = BitmapSourceConverter.ToBitmapSource(binaryMap);
            this.thresholdedImage = binaryMap;
        }

        #endregion

        #region Event handlers

        /// <summary>
        /// Event handler for when the background selection changes.
        /// </summary>
        private void BackgroundObjectsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsInitialized)
            {
                return;
            }

            isWhiteObjects = (BackgroundObjectsComboBox.SelectedIndex == 0);

            UpdateThresholdPreview();
        }

        /// <summary>
        /// Event handler for when the value of a red threshold slider changes.
        /// </summary>
        private void FirstChannelThreshold_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            if (!this.IsInitialized)
            {
                return;
            }

            var slider = sender as Slider;
            if (slider == null) return;

            string compareType = (string)slider.Tag;

            UpdateThresholdSliders(FirstChannelThreshold1, FirstChannelThreshold2, compareType);
            UpdateThresholdLines();
            UpdateThresholdLabels();

            UpdateThresholdPreview();
        }

        /// <summary>
        /// Event handler for when the value of a green threshold slider changes.
        /// </summary>
        private void SecondThreshold_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!this.IsInitialized)
            {
                return;
            }

            var slider = sender as Slider;
            if (slider == null) return;

            string compareType = (string)slider.Tag;

            UpdateThresholdSliders(SecondChannelThreshold1, SecondChannelThreshold2, compareType);
            UpdateThresholdLines();
            UpdateThresholdLabels();

            UpdateThresholdPreview();
        }

        /// <summary>
        /// Event handler for when the value of a blue threshold slider changes.
        /// </summary>
        private void ThirdThreshold_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!this.IsInitialized)
            {
                return;
            }

            var slider = sender as Slider;
            if (slider == null) return;

            string compareType = (string)slider.Tag;

            UpdateThresholdSliders(ThirdChannelThreshold1, ThirdChannelThreshold2, compareType);
            UpdateThresholdLines();
            UpdateThresholdLabels();

            UpdateThresholdPreview();
        }

        #endregion

        /// <summary>
        /// Event handler for the Apply button click event.
        /// </summary>
        private void ApplyColorThreshold_Click(object sender, RoutedEventArgs e)
        {
            this.binaryImage = this.thresholdedImage;
            this.DialogResult = true;
        }
    }
}
