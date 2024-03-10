﻿using Emgu.CV;
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

        public static List<(Mat image, string channelName)> SplitChannels(Mat image)
        {
            List<(Mat, string)> channelsWithNames = new List<(Mat, string)>();

            VectorOfMat vector = new VectorOfMat();
            CvInvoke.Split(image, vector);

            for (int i = 0; i < vector.Size; i++)
            {
                Mat channel = vector[i];
                Mat grayChannel = channel.Clone();

                string name = i switch
                {
                    0 => "(Blue channel)",
                    1 => "(Green channel)",
                    2 => "(Red channel)",
                    _ => $"({i} channel)"
                };

                channelsWithNames.Add((grayChannel, name));
            }

            return channelsWithNames;
        }
    }
}
