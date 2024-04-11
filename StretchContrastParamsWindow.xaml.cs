using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class StretchContrastParamsWindow : Window
    {
        public int? P1 { get; private set; }
        public int? P2 { get; private set; }
        public int? Q3 { get; private set; }
        public int? Q4 { get; private set; }

        public StretchContrastParamsWindow()
        {
            InitializeComponent();
        }

        private void ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (int.TryParse(textBox.Text, out int value))
                {
                    if (value < 0) textBox.Text = "0";
                    else if (value > 255) textBox.Text = "255";
                }
            }

            bool isValidValueRange = int.TryParse(BottomValueRangeTextBox.Text, out int bottomRange) &&
                                int.TryParse(TopValueRangeTextBox.Text, out int topRange) &&
                                bottomRange < topRange;

            bool isValidNewValueRange = int.TryParse(NewBottomValueRangeTextBox.Text, out int newBottomRange) &&
                                int.TryParse(NewTopValueRangeTextBox.Text, out int newTopRange) &&
                                newBottomRange < newTopRange;

            StretchButton.IsEnabled = isValidValueRange && isValidNewValueRange;
        }

        private void OnOkClicked(object sender, RoutedEventArgs e)
        {
            if (
                int.TryParse(BottomValueRangeTextBox.Text, out int p1) &&
                int.TryParse(TopValueRangeTextBox.Text, out int p2) &&
                int.TryParse(NewBottomValueRangeTextBox.Text, out int q3) &&
                int.TryParse(NewTopValueRangeTextBox.Text, out int q4)
                )
            {
                this.P1 = p1;
                this.P2 = p2;
                this.Q3 = q3;
                this.Q4 = q4;
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please enter valid numbers.");
            }
        }
    }
}
