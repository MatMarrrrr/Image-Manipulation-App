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
    /// Interaction logic for OneParamWindow.xaml
    /// </summary>
    public partial class OneParamWindow : Window
    {
        public int Param { get; private set; }
        public int Min { get; private set; }
        public int Max { get; private set; }
        

        public OneParamWindow(string windowTitle, string labelText, string buttonText, int min, int max)
        {
            InitializeComponent();
            this.Title = windowTitle;
            this.ParamTextBoxLabel.Text = $"{labelText}:";
            this.ParamButton.Content = buttonText;
            this.Min = min;
            this.Max = max;
        }

        private void ParamTextBox_TextChanged(object sender, EventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (int.TryParse(textBox.Text, out int value))
                {
                    if (value < this.Min) textBox.Text = $"{this.Min}";
                    else if (value > this.Max) textBox.Text = $"{this.Max}";
                }
            }

            this.ParamButton.IsEnabled = int.TryParse(this.ParamTextBox.Text, out _);
        }

        private void OnParamButtonClicked(object sender, EventArgs e)
        {
            if (int.TryParse(ParamTextBox.Text, out int levels))
            {
                this.Param = levels;
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please enter valid number");
            }
        }
    }
}
