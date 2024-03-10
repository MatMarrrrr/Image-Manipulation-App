using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APO_Mateusz_Marek_20456
{
    internal static class ImageOperarions
    {
        public static Mat ConvertToGrayScale(Mat image)
        {
            Mat grayImage = new Mat();
            CvInvoke.CvtColor(image, grayImage, ColorConversion.Bgr2Gray);

            return grayImage;
        }

        public static Mat NegateImage(Mat image)
        {
            unsafe
            {
                byte* dataPtr = (byte*)image.DataPointer;
                int width = image.Width;
                int height = image.Height;
                int step = image.Step;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        byte* pixelPtr = dataPtr + (y * step) + x;
                        *pixelPtr = (byte)(255 - *pixelPtr);
                    }
                }
            }
            return image;
        }

        public static Mat StretchContrast(Mat image)
        {
            double minVal = 0, maxVal = 0;
            int[]? minIdx = null;
            int[]? maxIdx = null;
            CvInvoke.MinMaxIdx(image, out minVal, out maxVal, minIdx, maxIdx);

            byte p1 = (byte)minVal;
            byte p2 = (byte)maxVal;
            byte q3 = 0;
            byte q4 = 255;

            unsafe
            {
                byte* dataPtr = (byte*)image.DataPointer;
                int width = image.Width;
                int height = image.Height;
                int step = image.Step;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        byte* pixelPtr = dataPtr + (y * step) + x;
                        byte pixelValue = *pixelPtr;

                        if (pixelValue < p1)
                        {
                            *pixelPtr = q3;
                        }
                        else if (pixelValue > p2)
                        {
                            *pixelPtr = q4;
                        }
                        else
                        {
                            *pixelPtr = (byte)(((pixelValue - p1) * (q4 - q3)) / (p2 - p1) + q3);
                        }
                    }
                }
            }

            return image;
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


    }
}
