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
        private Mat baseImage = new Mat(); // Oryginalny obraz
        public Mat binaryImage = new Mat();
        private Mat redChannel = new Mat();  // Kanał czerwony obrazu
        private Mat greenChannel = new Mat(); // Kanał zielony obrazu
        private Mat blueChannel = new Mat();  // Kanał niebieski obrazu

        private Mat redThresholdChannel = new Mat();  // Kanał czerwony obrazu
        private Mat greenThresholdChannel = new Mat(); // Kanał zielony obrazu
        private Mat blueThresholdChannel = new Mat();  // Kanał niebieski obrazu

        private Mat originalImage = new Mat(); // Przechowywanie oryginalnego obrazu
        private Mat thresholdedImage = new Mat(); // Przechowywanie progowanego obrazu

        /// <summary>
        /// Constructor to initialize the ColorThresholdWindow.
        /// </summary>
        /// <param name="image">The base image to be processed.</param>
        public ColorThresholdWindow(Mat image)
        {
            InitializeComponent(); // Initialize the WPF components

            this.baseImage = image; // Set the base image
            this.originalImage = image.Clone(); // Zapisz oryginalny obraz
            this.processedImage.Source = BitmapSourceConverter.ToBitmapSource(image); // Display the base image in the UI
            
            List<Mat> channels = SplitChannels(baseImage); // Split the image into color channels
            
            blueChannel = channels[0]; // Assign blue channel
            greenChannel = channels[1]; // Assign green channel
            redChannel = channels[2]; // Assign red channel

            blueThresholdChannel = channels[0]; // Assign blue channel
            greenThresholdChannel = channels[1]; // Assign green channel
            redThresholdChannel = channels[2]; // Assign red channel

            // Draw histograms for each color channel
            DrawHistogram(redChannel, redHistogramImage);
            DrawHistogram(greenChannel, greenHistogramImage);
            DrawHistogram(blueChannel, blueHistogramImage);

            // Update threshold lines once the window is loaded
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
            List<Mat> channels = new List<Mat>(); // List to hold the three color channels

            VectorOfMat vector = new VectorOfMat(); // Vector to hold the split channels
            CvInvoke.Split(image, vector); // Split the image into separate channels

            // Iterate through each channel and clone it
            for (int i = 0; i < vector.Size; i++)
            {
                Mat channel = vector[i]; // Get the current channel
                Mat grayChannel = channel.Clone(); // Clone the channel to avoid modification
                channels.Add(grayChannel); // Add the cloned channel to the list
            }

            return channels; // Return the list of channels
        }

        /// <summary>
        /// Combines three separate Mat objects (red, green, blue) into a single color image.
        /// </summary>
        /// <param name="redChannel">The red channel Mat.</param>
        /// <param name="greenChannel">The green channel Mat.</param>
        /// <param name="blueChannel">The blue channel Mat.</param>
        /// <returns>A single Mat object representing the combined color image.</returns>
        public Mat CombineChannels(Mat redChannel, Mat greenChannel, Mat blueChannel)
        {
            VectorOfMat channels = new VectorOfMat(blueChannel, greenChannel, redChannel);

            Mat colorImage = new Mat();

            CvInvoke.Merge(channels, colorImage);

            return colorImage;
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
            int[] histogramData = new int[256]; // Array to hold histogram data

            IntPtr dataPtr = imageMat.DataPointer; // Pointer to the image data
            int width = imageMat.Width; // Width of the image
            int height = imageMat.Height; // Height of the image
            int step = imageMat.Step; // Step size (number of bytes per row)

            // Iterate through each pixel in the image
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = y * step + x; // Calculate the offset of the current pixel
                    byte intensity = Marshal.ReadByte(dataPtr + offset); // Read the pixel intensity
                    histogramData[intensity]++; // Increment the corresponding histogram bin
                }
            }

            return histogramData; // Return the histogram data
        }

        /// <summary>
        /// Applies a threshold to the given image.
        /// </summary>
        /// <param name="image">The input image to threshold.</param>
        /// <param name="left">The lower threshold value.</param>
        /// <param name="right">The upper threshold value.</param>
        /// <returns>A Mat object representing the thresholded image.</returns>
        private Mat ApplyThreshold(Mat channel, int threshold1, int threshold2)
        {
            Mat thresholded = new Mat();
            CvInvoke.InRange(channel, new ScalarArray(threshold1), new ScalarArray(threshold2), thresholded);
            return thresholded;
        }

        private Mat ThresholdImage(Mat image, int left, int right)
        {
            IntPtr dataPtr = image.DataPointer;
            int width = image.Width;
            int height = image.Height;
            int step = image.Step;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = (y * step) + x;
                    byte pixelValue = Marshal.ReadByte(dataPtr, offset);
                    byte thresholdedPixelValue = (pixelValue >= left && pixelValue <= right) ? (byte)255 : (byte)0;
                    Marshal.WriteByte(dataPtr, offset, thresholdedPixelValue);
                }
            }

            return image;
        }

        public Mat CombineChannelsToBinary(Mat image)
        {
            Mat[] channels = image.Split();

            Mat binaryImage = new Mat(image.Rows, image.Cols, DepthType.Cv8U, 1);

            IntPtr dataPtrR = channels[2].DataPointer; // Red channel
            IntPtr dataPtrG = channels[1].DataPointer; // Green channel
            IntPtr dataPtrB = channels[0].DataPointer; // Blue channel
            IntPtr dataPtrBinary = binaryImage.DataPointer;

            int step = binaryImage.Step;

            for (int y = 0; y < image.Rows; y++)
            {
                for (int x = 0; x < image.Cols; x++)
                {
                    int offset = y * step + x;

                    // Get pixel values from each channel
                    byte pixelR = Marshal.ReadByte(dataPtrR, offset);
                    byte pixelG = Marshal.ReadByte(dataPtrG, offset);
                    byte pixelB = Marshal.ReadByte(dataPtrB, offset);

                    // Set binary value if the red channel is 255 and the other channels are 0 (considering only red objects)
                    byte binaryValue = (pixelR == 255 || pixelG == 255 || pixelB == 255) ? (byte)255 : (byte)0;
                    Marshal.WriteByte(dataPtrBinary, offset, binaryValue);
                }
            }

            return binaryImage;
        }

        /// <summary>
        /// Updates the threshold sliders to ensure threshold1 is always less than or equal to threshold2.
        /// </summary>
        /// <param name="threshold1">The first threshold slider.</param>
        /// <param name="threshold2">The second threshold slider.</param>
        /// <param name="type">The type of comparison ('more' or 'less').</param>
        private void UpdateThresholdSliders(Slider threshold1, Slider threshold2, String type)
        {
            // Ensure threshold1 is always less than or equal to threshold2
            if (type == "more" && threshold1.Value > threshold2.Value)
            {
                threshold2.Value = threshold1.Value;
            }
            // Ensure threshold2 is always greater than or equal to threshold1
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
            double canvasWidth = 380.0; // Width of the canvas containing the threshold lines
            double marginMultiplier = canvasWidth / 255.0; // Calculate the margin multiplier

            // Update red threshold lines
            Canvas.SetLeft(RedLine1, RedThreshold1.Value * marginMultiplier);
            Canvas.SetLeft(RedLine2, RedThreshold2.Value * marginMultiplier);

            // Update green threshold lines
            Canvas.SetLeft(GreenLine1, GreenThreshold1.Value * marginMultiplier);
            Canvas.SetLeft(GreenLine2, GreenThreshold2.Value * marginMultiplier);

            // Update blue threshold lines
            Canvas.SetLeft(BlueLine1, BlueThreshold1.Value * marginMultiplier);
            Canvas.SetLeft(BlueLine2, BlueThreshold2.Value * marginMultiplier);
        }

        /// <summary>
        /// Updates the text labels to display the current threshold values.
        /// </summary>
        private void UpdateThresholdLabels()
        {
            // Update red threshold text
            this.RedThresholdText.Text = $"Choosen range: {RedThreshold1.Value} - {RedThreshold2.Value}";
            // Update green threshold text
            this.GreenThresholdText.Text = $"Choosen range: {GreenThreshold1.Value} - {GreenThreshold2.Value}";
            // Update blue threshold text
            this.BlueThresholdText.Text = $"Choosen range: {BlueThreshold1.Value} - {BlueThreshold2.Value}";
        }

        private void UpdateThresholdPreview()
        {
            // Apply thresholds to each channel
            this.redThresholdChannel = ApplyThreshold(this.redChannel, (int)RedThreshold1.Value, (int)RedThreshold2.Value);
            this.greenThresholdChannel = ApplyThreshold(this.greenChannel, (int)GreenThreshold1.Value, (int)GreenThreshold2.Value);
            this.blueThresholdChannel = ApplyThreshold(this.blueChannel, (int)BlueThreshold1.Value, (int)BlueThreshold2.Value);

            // Perform logical AND operation on the thresholded channels
            Mat binaryMap = new Mat();
            CvInvoke.BitwiseAnd(this.redThresholdChannel, this.greenThresholdChannel, binaryMap);
            CvInvoke.BitwiseAnd(binaryMap, this.blueThresholdChannel, binaryMap);

            // Update the processed image in the UI
            this.processedImage.Source = BitmapSourceConverter.ToBitmapSource(binaryMap);

            this.thresholdedImage = binaryMap;
        }

        #endregion

        #region Event handlers

        /// <summary>
        /// Event handler for when the value of a red threshold slider changes.
        /// </summary>
        private void RedThreshold_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Ensure the window is fully initialized before handling the event
            if (!this.IsInitialized)
            {
                return;
            }

            var slider = sender as Slider; // Cast the sender to a Slider
            if (slider == null) return;

            string compareType = (string)slider.Tag; // Get the comparison type from the slider tag

            UpdateThresholdSliders(RedThreshold1, RedThreshold2, compareType); // Update the red threshold sliders
            UpdateThresholdLines(); // Update the positions of the threshold lines
            UpdateThresholdLabels(); // Update the threshold text labels

            // Update the threshold preview for the red channel
            UpdateThresholdPreview();
        }

        /// <summary>
        /// Event handler for when the value of a green threshold slider changes.
        /// </summary>
        private void GreenThreshold_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Ensure the window is fully initialized before handling the event
            if (!this.IsInitialized)
            {
                return;
            }

            var slider = sender as Slider; // Cast the sender to a Slider
            if (slider == null) return;

            string compareType = (string)slider.Tag; // Get the comparison type from the slider tag

            UpdateThresholdSliders(GreenThreshold1, GreenThreshold2, compareType); // Update the green threshold sliders
            UpdateThresholdLines(); // Update the positions of the threshold lines
            UpdateThresholdLabels(); // Update the threshold text labels

            // Update the threshold preview for the green channel
            UpdateThresholdPreview();
        }

        /// <summary>
        /// Event handler for when the value of a blue threshold slider changes.
        /// </summary>
        private void BlueThreshold_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Ensure the window is fully initialized before handling the event
            if (!this.IsInitialized)
            {
                return;
            }

            var slider = sender as Slider; // Cast the sender to a Slider
            if (slider == null) return;

            string compareType = (string)slider.Tag; // Get the comparison type from the slider tag

            UpdateThresholdSliders(BlueThreshold1, BlueThreshold2, compareType); // Update the blue threshold sliders
            UpdateThresholdLines(); // Update the positions of the threshold lines
            UpdateThresholdLabels(); // Update the threshold text labels

            // Update the threshold preview for the blue channel
            UpdateThresholdPreview();
        }

        #endregion

        private void ApplyColorThreshold_Click(object sender, RoutedEventArgs e)
        {
            this.binaryImage = this.thresholdedImage;
            this.DialogResult = true;
        }
    }
}
