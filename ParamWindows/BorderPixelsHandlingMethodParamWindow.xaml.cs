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

namespace Image_Manipulation_App.ParamWindows
{
    public partial class BorderPixelsHandlingMethodParamWindow : Window
    {
        public BorderType borderMethod { get; private set; }
        private BorderType[] borderMethodTypes = new BorderType[] { BorderType.Isolated, BorderType.Reflect, BorderType.Replicate };
        public BorderPixelsHandlingMethodParamWindow(string windowTitle, string buttonText)
        {
            InitializeComponent();
            this.Title = windowTitle;
            this.ApplyButton.Content = buttonText;
        }

        private void OnApplyButton_Click(object sender, RoutedEventArgs e)
        {
            this.borderMethod = borderMethodTypes[this.HandleBorderPixelsMethodComboBox.SelectedIndex];
            this.DialogResult = true;
        }
    }
}
