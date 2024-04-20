using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Point = System.Drawing.Point;

namespace Image_Manipulation_App
{

    public partial class ConvolverWindow : Window
    {
        private Mat img;
        public Mat? result;
        Dictionary<string, int[]> kernels = new Dictionary<string, int[]>()
        {
            // Edge Detection
            {"SobelX", new int[]{ -1, 0, 1, -2, 0, 2, -1, 0, 1 }},
            {"SobelY", new int[]{ -1, -2, -1, 0, 0, 0, 1, 2, 1 }},
            {"Laplacian", new int[]{ 1, 1, 1, 1, -8, 1, 1, 1, 1 }},
            // Sharpen
            {"Laplacian1", new int[]{ 0, -1, 0, -1, 5, -1, 0, -1, 0 }},
            {"Laplacian2", new int[]{ -1, -1, -1, -1, 9, -1, -1, -1, -1 }},
            {"Laplacian3", new int[]{ 1, -2, 1, -2, 5, -2, 1, -2, 1 }},
            // Directional Edge Detection
            {"PrewittN", new int[]{ -1, -1, -1, 0, 0, 0, 1, 1, 1 }},
            {"PrewittNE", new int[]{ -1, -1, 0, -1, 0, 1, 0, 1, 1 }},
            {"PrewittE", new int[]{ 0, -1, 1, -1, 0, 1, 0, -1, 1 }},
            {"PrewittSE", new int[]{ 0, 1, 1, -1, 0, 1, -1, -1, 0 }},
            {"PrewittS", new int[]{ 1, 1, 1, 0, 0, 0, -1, -1, -1 }},
            {"PrewittSW", new int[]{ 1, 1, 0, 1, 0, -1, 0, -1, -1 }},
            {"PrewittW", new int[]{ 1, -1, 0, 1, 0, -1, 1, -1, 0 }},
            {"PrewittNW", new int[]{ 0, -1, -1, 1, 0, -1, 1, 1, 0 }}
        };

        BorderType[] borderMethodTypes = new BorderType[] { BorderType.Isolated, BorderType.Reflect, BorderType.Replicate };

        public ConvolverWindow(Mat img)
        {
            InitializeComponent();
            this.img = img;
        }

        public void LoadKernel(string kernelName)
        {
            int[] kernel = this.kernels[kernelName];
            for (int i = 0; i < kernel.Length; i++)
            {
                TextBox? textBox = this.FindName($"Value{i + 1}") as TextBox;
                if (textBox != null)
                {
                    textBox.Text = kernel[i].ToString();
                }
            }
            this.SelectedMaskLabel.Content = $"Selected kernel: {kernelName}";
        }

        public int[] ExtractKernelValues()
        {
            int[] kernelValues = new int[9];
            for (int i = 0; i < kernelValues.Length; i++)
            {
                TextBox? textBox = this.FindName($"Value{i + 1}") as TextBox;
                if (textBox != null && int.TryParse(textBox.Text, out int value))
                {
                    kernelValues[i] = value;
                }
                else
                {
                    return Array.Empty<int>();
                }
            }
            return kernelValues;
        }

        private bool AreAllValuesValid()
        {
            TextBox[] textBoxes = { this.Value1, this.Value2, this.Value3, this.Value4, this.Value5, this.Value6, this.Value7, this.Value8, this.Value9 };
            foreach (TextBox textBox in textBoxes)
            {
                if (!double.TryParse(textBox.Text, out _))
                {
                    return false;
                }
            }
            return true;
        }

        private void ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!this.IsInitialized)
            {
                return;
            }

            int[] currentKernelValues = ExtractKernelValues();

            var matchingKernel = kernels.FirstOrDefault(kernel => kernel.Value.SequenceEqual(currentKernelValues));

            if (matchingKernel.Key != null)
            {
                this.SelectedMaskLabel.Content = $"Selected kernel: {matchingKernel.Key}";
            }
            else
            {
                this.SelectedMaskLabel.Content = "Selected kernel: Custom";
            }

            ApplyButton.IsEnabled = AreAllValuesValid();
        }

        private void LoadKernel_Click(object sender, RoutedEventArgs e)
        {
            MenuItem? menuItem = sender as MenuItem;

            if (menuItem != null)
            {
                string? kernelName = menuItem.CommandParameter as string;
                if (!string.IsNullOrEmpty(kernelName) && kernels.ContainsKey(kernelName))
                {
                    LoadKernel(kernelName);
                }
            }
        }

        private Mat ApplyKernel()
        {
            BorderType selectedBorderMethod = borderMethodTypes[this.EdgeHandlingMethodComboBox.SelectedIndex];

            int[] kernelValues = ExtractKernelValues();

            float[] kernelFloats = Array.ConvertAll(kernelValues, item => (float)item);

            Matrix<float> kernelMatrix = new Matrix<float>(3, 3);
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    kernelMatrix[i, j] = kernelFloats[i * 3 + j];
                }
            }

            Mat resultImage = new Mat(img.Size, img.Depth, img.NumberOfChannels);
            CvInvoke.Filter2D(img, resultImage, kernelMatrix, new Point(-1, -1), 0, selectedBorderMethod);
            return resultImage;
        }

        private void Canny_Click(object sender, RoutedEventArgs e)
        {
            if(this.CannyInfoLabel.Visibility == Visibility.Hidden && this.SelectedMaskLabel.Content.ToString() != "Selected kernel: Canny")
            {
                this.CannyInfoLabel.Visibility = Visibility.Visible;
                this.SelectedMaskLabel.Content = "Selected kernel: Canny";
            }

            TwoParamsWindow dialog = new TwoParamsWindow(
                "Canny params", 
                "Threshold 1 value",
                "Threshold 2 value",
                "Apply",
                0,
                255,
                0,
                255,
                "int",
                "int",
                "less"
                );
            dialog.Closing += OnCannyParamsClosed;
            if (dialog.ShowDialog() == true)
            {
                int threshold1 = dialog.IntParam1;
                int threshold2 = dialog.IntParam2;

                Mat resultImage = new Mat(img.Size, img.Depth, img.NumberOfChannels);
                CvInvoke.Canny(this.img, resultImage, threshold1, threshold2);
                this.result = resultImage;
                this.DialogResult = true;
            }
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            this.result = ApplyKernel();
            this.DialogResult = true;
        }

        private void OnCannyParamsClosed(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            this.SelectedMaskLabel.Content = "Selected kernel: Custom";
            this.CannyInfoLabel.Visibility = Visibility.Hidden;
        }
    }
}
