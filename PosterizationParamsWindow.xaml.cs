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
    /// Interaction logic for PosterizationParamsWindow.xaml
    /// </summary>
    public partial class PosterizationParamsWindow : Window
    {
        public int? levels { get; private set; }

        public PosterizationParamsWindow()
        {
            InitializeComponent();
        }

        private void NumberOfLevelsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (int.TryParse(textBox.Text, out int value))
                {
                    if (value < 2) textBox.Text = "2";
                    else if (value > 255) textBox.Text = "255";
                }
            }

            PosterizeButton.IsEnabled = int.TryParse(NumberOfLevelsTextBox.Text, out _);
        }

        private void OnPosterizeClicked(object sender, EventArgs e)
        {
            if (int.TryParse(NumberOfLevelsTextBox.Text, out int levels))
            {
                this.levels = levels;
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please enter valid numbers.");
            }
        }
    }
}
