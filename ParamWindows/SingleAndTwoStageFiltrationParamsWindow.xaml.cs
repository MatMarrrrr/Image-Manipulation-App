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

            ApplyButton.IsEnabled = AreAllValuesValid();
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

        public Matrix<float> ConvolveKernels(Matrix<float> kernel1, Matrix<float> kernel2)
        {
            int finalSize = 5;
            Matrix<float> result = new Matrix<float>(finalSize, finalSize);

            for (int x = 0; x < finalSize; x++)
            {
                for (int y = 0; y < finalSize; y++)
                {
                    float sum = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            int ni = x - i + 1;
                            int nj = y - j + 1;
                            if (ni >= 0 && ni < 3 && nj >= 0 && nj < 3)
                            {
                                sum += kernel1[i, j] * kernel2[ni, nj];
                            }
                        }
                    }
                    result[x, y] = sum;
                }
            }

            return result;
        }
    }
}
