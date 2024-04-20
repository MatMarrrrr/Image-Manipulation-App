using System;
using System.Collections.Generic;
using System.Globalization;
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
using static Emgu.CV.ML.SVM;

namespace Image_Manipulation_App
{
    public partial class TwoParamsWindow : Window
    {
        public double DoubleParam1 { get; private set; }
        public double DoubleParam2 { get; private set; }
        public int IntParam1 { get; private set; }
        public int IntParam2 { get; private set; }
        public double Min1 { get; private set; }
        public double Max1 { get; private set; }
        public double Min2 { get; private set; }
        public double Max2 { get; private set; }
        public ParameterRelation relation { get; private set; }

        private bool isDouble1;
        private bool isDouble2;

        public TwoParamsWindow(string windowTitle, string label1Text, string label2Text, string buttonText, double min1, double max1, double min2, double max2, string param1Type, string param2Type, string relation)
        {
            InitializeComponent();
            this.Title = windowTitle;
            this.Param1TextBoxLabel.Text = $"{label1Text}:";
            this.Param2TextBoxLabel.Text = $"{label2Text}:";
            this.ParamsButton.Content = buttonText;
            this.Min1 = min1;
            this.Max1 = max1;
            this.Min2 = min2;
            this.Max2 = max2;
            this.relation = (ParameterRelation)Enum.Parse(typeof(ParameterRelation), relation, true);

            this.isDouble1 = param1Type.ToLower() == "double";
            this.isDouble2 = param2Type.ToLower() == "double";
        }

        private void ParamTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string input = textBox.Text.Replace(',', '.');

                if (textBox.Name == "Param1TextBox" && double.TryParse(input, out double doubleValue))
                {
                    AdjustValueInRange(textBox, doubleValue, Min1, Max1);
                }
                else if (textBox.Name == "Param2TextBox" && double.TryParse(input, out doubleValue))
                {
                    AdjustValueInRange(textBox, doubleValue, Min2, Max2);
                }
                else if (textBox.Name == "Param1TextBox" && int.TryParse(input, out int intValue))
                {
                    AdjustValueInRange(textBox, intValue, Min1, Max1);
                }
                else if (textBox.Name == "Param2TextBox" && int.TryParse(input, out intValue))
                {
                    AdjustValueInRange(textBox, intValue, Min2, Max2);
                }
            }

            this.ParamsButton.IsEnabled = ValidateInput(Param1TextBox.Text, isDouble1) && ValidateInput(Param2TextBox.Text, isDouble2);
        }

        private void OnParamsButtonClicked(object sender, RoutedEventArgs e)
        {
            if (isDouble1)
            {
                DoubleParam1 = double.Parse(Param1TextBox.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
            }
            else
            {
                IntParam1 = int.Parse(Param1TextBox.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
            }

            if (isDouble2)
            {
                DoubleParam2 = double.Parse(Param2TextBox.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
            }
            else
            {
                IntParam2 = int.Parse(Param2TextBox.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
            }

            if (ValidateInputs())
            {
                this.DialogResult = true;
            }
            else
            {
                ShowConditionMessage();
            }
        }

        private void AdjustValueInRange(TextBox textBox, double value, double min, double max)
        {
            if (value < min)
                textBox.Text = $"{min}";
            else if (value > max)
                textBox.Text = $"{max}";
        }

        private bool ValidateInput(string text, bool isDouble)
        {
            string input = text.Replace(',', '.');
            return isDouble ? double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out _) : int.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
        }

        private bool ValidateInputs()
        {
            double value1 = isDouble1 ? DoubleParam1 : IntParam1;
            double value2 = isDouble2 ? DoubleParam2 : IntParam2;

            switch (this.relation)
            {
                case ParameterRelation.Greater:
                    return value1 > value2;
                case ParameterRelation.Less:
                    return value1 < value2;
                case ParameterRelation.Equal:
                    return value1 == value2;
                case ParameterRelation.None:
                    return true;
                default:
                    return true;
            }
        }

        private void ShowConditionMessage()
        {
            string message = "";
            string? param1 = this.Param1TextBoxLabel.Text;
            string? param2 = this.Param2TextBoxLabel.Text.ToLower();

            switch (this.relation)
            {
                case ParameterRelation.Greater:
                    message = $"{param1} has to be greater than {param2}";
                    break;
                case ParameterRelation.Less:
                    message = $"{param1} has to be less than {param2}";
                    break;
                case ParameterRelation.Equal:
                    message = $"{param1} has to equal {param2}";
                    break;
            }
            MessageBox.Show(message);
        }
    }

    public enum ParameterRelation
    {
        None,
        Greater,
        Less,
        Equal
    }
}
