using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Image_Manipulation_App
{
    /// <summary>
    /// Interaction logic for ThresholdWindow.xaml
    /// </summary>
    public partial class ThresholdWindow : Window
    {
        private Mat originalImage;
        private Mat thresholdedImage;
        public Mat finalImage;

        public ThresholdWindow(Mat image)
        {
            InitializeComponent();
            this.originalImage = image;
            this.thresholdedImage = new Mat();
            this.finalImage = new Mat();
            this.processedImage.Source = BitmapSourceConverter.ToBitmapSource(image);
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (this.IsInitialized)
            {
                this.processedImage.Source = BitmapSourceConverter.ToBitmapSource(originalImage);
                sliderThreshold.IsEnabled = manualThresholdingRadioButton.IsChecked ?? false;

                if (manualThresholdingRadioButton.IsChecked == true)
                {
                    thresholdedImage = ImageOperations.ManualThreshold(originalImage, sliderThreshold.Value);
                }
                else if (adaptiveThresholdingRadioButton.IsChecked == true)
                {
                    thresholdedImage = ImageOperations.AdaptiveThreshold(originalImage);
                }
                else if (otsusThresholdingRadioButton.IsChecked == true)
                {
                    (Mat otsuImage, double otsuThreshold) = ImageOperations.OtsuThreshold(originalImage);
                    thresholdedImage = otsuImage;
                    sliderValueDisplay.Text = $"Threshold: {otsuThreshold}";
                    sliderThreshold.Value = otsuThreshold;
                }
                else if (originalImageRadioButton.IsChecked == true)
                {
                    thresholdedImage = originalImage;
                }

                this.processedImage.Source = BitmapSourceConverter.ToBitmapSource(thresholdedImage);
            }
        }

        private void Slider_Threshold_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sliderValueDisplay != null)
            {
                sliderValueDisplay.Text = $"Threshold: {sliderThreshold.Value}";
            }

            if (manualThresholdingRadioButton.IsChecked == true)
            {
                thresholdedImage = ImageOperations.ManualThreshold(originalImage, sliderThreshold.Value);
                this.processedImage.Source = BitmapSourceConverter.ToBitmapSource(thresholdedImage);
            }
        }

        private void ApplyThreshold_Click(object sender, RoutedEventArgs e)
        {
            this.finalImage = thresholdedImage;
            this.DialogResult = true;
        }
    }
}
