using System.Collections.Generic;
using System.Windows;
using static Image_Manipulation_App.ImageOperations;

namespace Image_Manipulation_App
{
    public class AnalysisResult
    {
        public int No { get; set; }
        public int Moments { get; set; }
        public double Area { get; set; }
        public double Perimeter { get; set; }
        public double AspectRatio { get; set; }
        public double Extent { get; set; }
        public double Solidity { get; set; }
        public double EquivalentDiameter { get; set; }
    }
    public partial class AnalyzedImageDataWindow : Window
    {
        public AnalyzedImageDataWindow(List<AnalysisResult> analysisResults)
        {
            InitializeComponent();
            AnalysisDataGrid.ItemsSource = analysisResults;
        }
    }
}