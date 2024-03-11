﻿using System;
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

namespace APO_Mateusz_Marek_20456
{
    public partial class StretchContrastParamsWindow : Window
    {
        public int? MinValue { get; private set; }
        public int? MaxValue { get; private set; }

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

                OkButton.IsEnabled = int.TryParse(MinValueTextBox.Text, out _) &&
                                     int.TryParse(MaxValueTextBox.Text, out _);
            }
        }

        private void OnOkClicked(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(MinValueTextBox.Text, out int minValue) &&
                int.TryParse(MaxValueTextBox.Text, out int maxValue))
            {
                MinValue = minValue;
                MaxValue = maxValue;
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please enter valid numbers.");
            }
        }
    }
}