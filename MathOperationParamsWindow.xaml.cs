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
    public partial class MathOperationParamsWindow : Window
    {
        public int FirstImageIndex { get; private set; }
        public int SecondImageIndex { get; private set; }
        public MathOperationParamsWindow(List<ImageWindow> imageWindows, string windowTitle, string buttonText)
        {
            InitializeComponent();

            this.Title = windowTitle;
            this.OperationButton.Content = buttonText;
            FillComboBoxes(imageWindows);
        }

        public void FillComboBoxes(List<ImageWindow> imageWindows)
        {
            for (int i = 0; i < imageWindows.Count; i++)
            {
                if (imageWindows[i]?.imageMat?.NumberOfChannels == 1)
                {
                    ImagesList1.Items.Add(new ComboBoxItem(imageWindows[i].Title, i));
                    ImagesList2.Items.Add(new ComboBoxItem(imageWindows[i].Title, i));
                }

            }

            ImagesList1.SelectedIndex = 0;
            ImagesList2.SelectedIndex = 0;
        }

        public void OperationButton_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem? firstSelectedItem = ImagesList1.SelectedItem as ComboBoxItem;
            this.FirstImageIndex = firstSelectedItem?.HiddenValue ?? 0;

            ComboBoxItem? secondSelectedItem = ImagesList2.SelectedItem as ComboBoxItem;
            this.SecondImageIndex = secondSelectedItem?.HiddenValue ?? 0;

            this.DialogResult = true;
        }
    }

    public class ComboBoxItem
    {
        public string DisplayValue { get; set; }
        public int HiddenValue { get; set; }

        public ComboBoxItem(string display, int hidden)
        {
            DisplayValue = display;
            HiddenValue = hidden;
        }

        public override string ToString()
        {
            return DisplayValue;
        }
    }
}
