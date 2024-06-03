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
    public partial class StructureSizeParamWindow : Window
    {
        public string structure { get; private set; } = "";
        public int size { get; private set; }
        public StructureSizeParamWindow(string title, string buttonValue)
        {
            InitializeComponent();
            this.Title = title;
            this.ApplyButton.Content = buttonValue;
        }

        private void OnApplyButton_Click(object sender, RoutedEventArgs e)
        {
            this.structure = StructureTypeComboBox.SelectedIndex switch
            {
                0 => "diamond",
                1 => "square",
                _ => "diamond"
            };
            this.size = StructureSizeComboBox.SelectedIndex switch
            {
                0 => 3,
                1 => 5,
                _ => 3
            };

            this.DialogResult = true;
        }
    }
}
