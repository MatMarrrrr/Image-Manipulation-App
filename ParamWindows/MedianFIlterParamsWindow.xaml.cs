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
    public partial class MedianFIlterParamsWindow : Window
    {
        public int kernelSize { get; private set; }
        public BorderType borderMethod { get; private set; }
        private int[] kernelSizes = { 3, 5, 7 };
        private BorderType[] borderMethodTypes = new BorderType[] { BorderType.Isolated, BorderType.Reflect, BorderType.Replicate };
        public MedianFIlterParamsWindow()
        {
            InitializeComponent();
        }

        private void OnApplyMedianFilterButton_Click(object sender, RoutedEventArgs e)
        {
            this.kernelSize = kernelSizes[this.KernelSizeComboBox.SelectedIndex];
            this.borderMethod = borderMethodTypes[this.HandleBorderPixelsMethodComboBox.SelectedIndex];
            this.DialogResult = true;
        }
    }
}
