using System;
using System.Windows;
using System.Windows.Controls;

namespace Image_Manipulation_App
{
    public partial class OneParamWindow : Window
    {
        public double DoubleParam { get; private set; }
        public int IntParam { get; private set; }
        public double Min { get; private set; }
        public double Max { get; private set; }
        private bool isDouble;

        public OneParamWindow(string windowTitle, string labelText, string buttonText, double min, double max, string paramType)
        {
            InitializeComponent();
            this.Title = windowTitle;
            this.ParamTextBoxLabel.Text = $"{labelText}:";
            this.ParamButton.Content = buttonText;
            this.Min = min;
            this.Max = max;

            this.isDouble = paramType.ToLower() == "double";
        }

        private void ParamTextBox_TextChanged(object sender, EventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (this.isDouble && double.TryParse(textBox.Text, out double doubleValue))
                {
                    AdjustValueInRange(textBox, doubleValue);
                }
                else if (!this.isDouble && int.TryParse(textBox.Text, out int intValue))
                {
                    AdjustValueInRange(textBox, intValue);
                }
            }

            this.ParamButton.IsEnabled = ValidateInput(this.ParamTextBox.Text);
        }

        private void OnParamButtonClicked(object sender, EventArgs e)
        {
            if (this.isDouble && double.TryParse(ParamTextBox.Text, out double doubleValue))
            {
                this.DoubleParam = doubleValue;
                this.DialogResult = true;
            }
            else if (!this.isDouble && int.TryParse(ParamTextBox.Text, out int intValue))
            {
                this.IntParam = intValue;
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please enter a valid number");
            }
        }

        private void AdjustValueInRange(TextBox textBox, double value)
        {
            if (value < this.Min)
                textBox.Text = $"{this.Min}";
            else if (value > this.Max)
                textBox.Text = $"{this.Max}";
        }

        private bool ValidateInput(string text)
        {
            return this.isDouble ? double.TryParse(text, out _) : int.TryParse(text, out _);
        }
    }
}