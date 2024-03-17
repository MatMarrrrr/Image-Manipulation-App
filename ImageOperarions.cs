using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace Image_Manipulation_App
{
    internal static class ImageOperarions
    {
        public class HistogramTableDataRow
        {
            public int Intensity { get; set; }
            public int Count { get; set; }
        }

        public static Mat ConvertToGrayScale(Mat image)
        {
            Mat grayImage = new Mat();
            CvInvoke.CvtColor(image, grayImage, ColorConversion.Bgr2Gray);

            return grayImage;
        }

        public static int[] CalculateHistogram(Mat imageMat)
        {
            int[] histogramData = new int[256];

            IntPtr dataPtr = imageMat.DataPointer;
            int width = imageMat.Width;
            int height = imageMat.Height;
            int step = imageMat.Step;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = y * step + x;
                    byte intensity = Marshal.ReadByte(dataPtr + offset);
                    histogramData[intensity]++;
                }
            }

            return histogramData;
        }

        public static List<HistogramTableDataRow> CalculateTableHistogramData(int[] histogramData)
        {
            List<HistogramTableDataRow> histogramTableData = new List<HistogramTableDataRow>();
            for (int i = 0; i < histogramData.Length; i++)
            {
                histogramTableData.Add(new HistogramTableDataRow { Intensity = i, Count = histogramData[i] });
            }
            return histogramTableData;
        }
        public static Mat NegateImage(Mat image)
        {
            Mat result = image.Clone();

            IntPtr dataPtr = result.DataPointer;
            int width = result.Width;
            int height = result.Height;
            int step = result.Step;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = (y * step) + x;
                    byte pixelValue = Marshal.ReadByte(dataPtr, offset);

                    byte negatedValue = (byte)(255 - pixelValue);

                    Marshal.WriteByte(dataPtr, offset, negatedValue);
                }
            }

            return result;
        }

        public static Mat StretchHistogram(Mat image, byte q3 = 0, byte q4 = 255)
        {
            double minVal, maxVal;
            CvInvoke.MinMaxIdx(image, out minVal, out maxVal, null, null);

            byte p1 = (byte)minVal;
            byte p2 = (byte)maxVal;

            Mat result = image.Clone();

            IntPtr dataPtr = result.DataPointer;
            int width = result.Width;
            int height = result.Height;
            int step = result.Step;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = (y * step) + x;
                    byte pixelValue = Marshal.ReadByte(dataPtr, offset);

                    byte stretchedValue;
                    if (pixelValue < p1)
                    {
                        stretchedValue = q3;
                    }
                    else if (pixelValue > p2)
                    {
                        stretchedValue = q4;
                    }
                    else
                    {
                        stretchedValue = (byte)(((pixelValue - p1) * (q4 - q3)) / (p2 - p1) + q3);
                    }

                    Marshal.WriteByte(dataPtr, offset, stretchedValue);
                }
            }

            return result;
        }

        public static List<(Mat image, string channelName)> SplitChannels(Mat image, string type)
        {
            List<(Mat image, string channelName)> channelsWithNames = new List<(Mat image, string channelName)>();

            VectorOfMat vector = new VectorOfMat();
            CvInvoke.Split(image, vector);

            for (int i = 0; i < vector.Size; i++)
            {
                Mat channel = vector[i];
                Mat grayChannel = channel.Clone();

                string name = type switch
                {
                    "HSV" => i switch
                    {
                        0 => "Hue (H)",
                        1 => "Saturation (S)",
                        2 => "Value (V)",
                        _ => $"Channel {i}"
                    },
                    "Lab" => i switch
                    {
                        0 => "Lightness (L)",
                        1 => "Green-Red (a)",
                        2 => "Blue-Yellow (b)",
                        _ => $"Channel {i}"
                    },
                    "RGB" => i switch
                    {
                        0 => "Blue (B)",
                        1 => "Green (G)",
                        2 => "Red (R)",
                        _ => $"Channel {i}"
                    },
                    _ => $"Channel {i}"
                };

                channelsWithNames.Add((grayChannel, $"{name} - {type}"));
            }

            return channelsWithNames;
        }

        public static (List<(Mat image, string channelName)> hsv, List<(Mat image, string channelName)> lab) ConvertAndSplitRgbToHsvAndLab(Mat rgbImage)
        {
            var hsvChannels = ConvertAndSplitRgb(rgbImage, "HSV");
            var labChannels = ConvertAndSplitRgb(rgbImage, "Lab");

            return (hsvChannels, labChannels);
        }

        public static List<(Mat image, string channelName)> ConvertAndSplitRgb(Mat rgbImage, string output = "HSV")
        {
            ColorConversion conversion = output == "HSV" ? ColorConversion.Bgr2Hsv : ColorConversion.Bgr2Lab;
            Mat Image = new Mat();
            CvInvoke.CvtColor(rgbImage, Image, conversion);
            var channels = SplitChannels(Image, output);

            return channels;
        }

        public static Mat EqualizeHistogram(Mat image)
        {

            int[] histogram = CalculateHistogram(image);
            int total = image.Width * image.Height;

            int sum = 0;
            float scale = 255.0f / total;
            byte[] lut = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                sum += histogram[i];
                lut[i] = (byte)(sum * scale);
            }

            Mat equalizedImage = new Mat(image.Size, DepthType.Cv8U, 1);
            unsafe
            {
                byte* srcPtr = (byte*)image.DataPointer;
                byte* dstPtr = (byte*)equalizedImage.DataPointer;
                int width = image.Width;
                int height = image.Height;
                int step = image.Step;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        byte val = srcPtr[y * step + x];
                        dstPtr[y * step + x] = lut[val];
                    }
                }
            }

            return equalizedImage;
        }


    }
}
