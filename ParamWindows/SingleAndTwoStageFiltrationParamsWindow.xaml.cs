using Emgu.CV;
using Emgu.CV.CvEnum;
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
    
    public partial class SingleAndTwoStageFiltrationParamsWindow : Window
    {
        public int[] firstKernel = new int[9];
        public int[] secondKernel = new int[9];
        public BorderType borderMethod = BorderType.Isolated;

        private BorderType[] borderMethodTypes = new BorderType[] { BorderType.Isolated, BorderType.Reflect, BorderType.Replicate };
        public SingleAndTwoStageFiltrationParamsWindow()
        {
            InitializeComponent();
        }

        private void ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!this.IsInitialized)
            {
                return;
            }

            ApplyButton.IsEnabled = false;
            ResultKernelContainer.Visibility = Visibility.Hidden;

            if (AreAllValuesValid())
            {
                (int[] kernel1, int[] kernel2) = ExtractKernelValues();
                Matrix<float> firstKernel = new Matrix<float>(3, 3);
                Matrix<float> secondKernel = new Matrix<float>(3, 3);
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        firstKernel[i, j] = kernel1[i * 3 + j];
                        secondKernel[i, j] = kernel2[i * 3 + j];
                    }
                }
                Matrix<float> combinedKernel = ImageOperations.ConvolveKernels(firstKernel, secondKernel);
                int textBoxIndex = 0;
                for (int i = 19; i <= 43; i++)
                {
                    TextBlock? textBlock = this.FindName($"Value{i}") as TextBlock;
                    if (textBlock != null)
                    {
                        int j = textBoxIndex / 5;
                        int k = textBoxIndex % 5;
                        textBlock.Text = combinedKernel[j, k].ToString();
                        textBoxIndex++;
                    }
                }
                ApplyButton.IsEnabled = true;
                ResultKernelContainer.Visibility = Visibility.Visible;
            }



        }

        private bool AreAllValuesValid()
        {
            for (int i = 1; i <= 18; i++)
            {
                TextBox? textBox = this.FindName($"Value{i}") as TextBox;
                if (!double.TryParse(textBox?.Text, out _))
                {
                    return false;
                }
            }
            return true;
        }

        public (int[], int[]) ExtractKernelValues()
        {
            int[] kernel1Values = new int[9];
            int[] kernel2Values = new int[9];

            for (int i = 0; i < 9; i++)
            {
                TextBox? textBox1 = this.FindName($"Value{i + 1}") as TextBox;
                if (textBox1 != null && int.TryParse(textBox1.Text, out int value1))
                {
                    kernel1Values[i] = value1;
                }
            }

            for (int i = 0; i < 9; i++)
            {
                TextBox? textBox2 = this.FindName($"Value{i + 10}") as TextBox;
                if (textBox2 != null && int.TryParse(textBox2.Text, out int value2))
                {
                    kernel2Values[i] = value2;
                }
            }

            return (kernel1Values, kernel2Values);
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            (int[] kernel1, int[] kernel2) = ExtractKernelValues();
            BorderType selectedBorderMethod = borderMethodTypes[this.EdgeHandlingMethodComboBox.SelectedIndex];
            this.firstKernel = kernel1;
            this.secondKernel = kernel2;
            this.borderMethod = selectedBorderMethod;
            this.DialogResult = true;
        }
    }
}
