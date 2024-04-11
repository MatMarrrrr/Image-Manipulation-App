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
    internal static class ImageOperations
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
            IntPtr dataPtr = image.DataPointer;
            int width = image.Width;
            int height = image.Height;
            int step = image.Step;

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

            return image;
        }

        public static Mat StretchContrast(Mat image, int p1, int p2, int q3, int q4)
        {
            byte minValue = 255;
            byte maxValue = 0;

            IntPtr dataPtr = image.DataPointer;
            int width = image.Width;
            int height = image.Height;
            int step = image.Step;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = (y * step) + x;
                    byte pixelValue = Marshal.ReadByte(dataPtr, offset);
                    if (pixelValue >= p1 && pixelValue <= p2)
                    {
                        if (pixelValue < minValue) minValue = pixelValue;
                        if (pixelValue > maxValue) maxValue = pixelValue;
                    }
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = (y * step) + x;
                    double pixelValue = Marshal.ReadByte(dataPtr, offset);
                    if (pixelValue >= p1 && pixelValue <= p2)
                    {
                        byte newPixelValue = (byte)Math.Round(((pixelValue - minValue) / (maxValue - minValue)) * (q4 - q3) + q3);
                        Marshal.WriteByte(dataPtr, offset, newPixelValue);
                    }
                }
            }

            return image;
        }

        public static Mat StretchHistogram(Mat image)
        {
            CvInvoke.MinMaxIdx(image, out double minValue, out double maxValue, null, null);
            return ImageOperations.StretchContrast(image, (int)minValue, (int)maxValue, 0, 255);
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
            IntPtr srcDataPtr = image.DataPointer;
            IntPtr dstDataPtr = equalizedImage.DataPointer;
            int width = image.Width;
            int height = image.Height;
            int step = image.Step;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = y * step + x;
                    byte val = Marshal.ReadByte(srcDataPtr, offset);
                    Marshal.WriteByte(dstDataPtr, offset, lut[val]);
                }
            }

            return equalizedImage;
        }

        public static Mat AddImages(Mat image1, Mat image2)
        {
            int Width = Math.Min(image1.Width, image2.Width);
            int Height = Math.Min(image1.Height, image2.Height);

            Mat result = new Mat(Height, Width, image1.Depth, image1.NumberOfChannels);

            IntPtr ptr1 = image1.DataPointer;
            IntPtr ptr2 = image2.DataPointer;
            IntPtr resultPtr = result.DataPointer;

            int step1 = image1.Step;
            int step2 = image2.Step;
            int resultStep = result.Step;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int offset1 = (y * step1) + x;
                    int offset2 = (y * step2) + x;
                    int resultOffset = (y * resultStep) + x;

                    byte pixelValue1 = Marshal.ReadByte(ptr1, offset1);
                    byte pixelValue2 = Marshal.ReadByte(ptr2, offset2);

                    int sum = pixelValue1 + pixelValue2;
                    byte sumValue = sum > 255 ? (byte)255 : (byte)sum;

                    Marshal.WriteByte(resultPtr, resultOffset, sumValue);
                }
            }

            return result;
        }

        public static Mat SubtractImages(Mat image1, Mat image2)
        {
            int Width = Math.Min(image1.Width, image2.Width);
            int Height = Math.Min(image1.Height, image2.Height);

            Mat result = new Mat(Height, Width, image1.Depth, image1.NumberOfChannels);

            IntPtr ptr1 = image1.DataPointer;
            IntPtr ptr2 = image2.DataPointer;
            IntPtr resultPtr = result.DataPointer;

            int step1 = image1.Step;
            int step2 = image2.Step;
            int resultStep = result.Step;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int offset1 = (y * step1) + x;
                    int offset2 = (y * step2) + x;
                    int resultOffset = (y * resultStep) + x;

                    byte pixelValue1 = Marshal.ReadByte(ptr1, offset1);
                    byte pixelValue2 = Marshal.ReadByte(ptr2, offset2);

                    int difference = pixelValue1 - pixelValue2;
                    byte differenceValue = (byte)Math.Max(0, difference);

                    Marshal.WriteByte(resultPtr, resultOffset, differenceValue);
                }
            }

            return result;
        }

        public static Mat PosterizeImage(Mat image, int levels)
        {
            IntPtr dataPtr = image.DataPointer;
            int width = image.Width;
            int height = image.Height;
            int step = image.Step;

            float levelStep = 255f / (levels - 1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = (y * step) + x;
                    byte pixelValue = Marshal.ReadByte(dataPtr, offset);

                    int levelIndex = (int)Math.Round(pixelValue / levelStep);
                    byte posterizedValue = (byte)(levelIndex * levelStep);

                    Marshal.WriteByte(dataPtr, offset, posterizedValue);
                }
            }

            return image;
        }

    }
}
