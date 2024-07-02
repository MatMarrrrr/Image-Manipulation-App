using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Microsoft.Win32;
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
            var (paddedImage1, paddedImage2, size) = PaddingImages(image1, image2);
            Mat result = new Mat(size, DepthType.Cv8U, 1);
            CvInvoke.Add(paddedImage1, paddedImage2, result);

            return result;
        }

        public static Mat SubtractImages(Mat image1, Mat image2)
        {
            var (paddedImage1, paddedImage2, size) = PaddingImages(image1, image2);
            Mat result = new Mat(size, DepthType.Cv8U, paddedImage1.NumberOfChannels);

            CvInvoke.Subtract(paddedImage1, paddedImage2, result);
            return result;
        }

        public static Mat BlendImages(Mat image1, Mat image2, double alpha)
        {
            var (paddedImage1, paddedImage2, size) = PaddingImages(image1, image2);
            Mat result = new Mat(size, DepthType.Cv8U, paddedImage1.NumberOfChannels);
            double beta = 1.0 - alpha;

            CvInvoke.AddWeighted(paddedImage1, alpha, paddedImage2, beta, 0, result);
            return result;
        }

        public static Mat AndImages(Mat image1, Mat image2)
        {
            var (paddedImage1, paddedImage2, size) = PaddingImages(image1, image2);
            Mat result = new Mat(size, DepthType.Cv8U, paddedImage1.NumberOfChannels);

            CvInvoke.BitwiseAnd(paddedImage1, paddedImage2, result);
            return result;
        }

        public static Mat OrImages(Mat image1, Mat image2)
        {
            var (paddedImage1, paddedImage2, size) = PaddingImages(image1, image2);
            Mat result = new Mat(size, DepthType.Cv8U, paddedImage1.NumberOfChannels);

            CvInvoke.BitwiseOr(paddedImage1, paddedImage2, result);
            return result;
        }

        public static Mat XorImages(Mat image1, Mat image2)
        {
            var (paddedImage1, paddedImage2, size) = PaddingImages(image1, image2);
            Mat result = new Mat(size, DepthType.Cv8U, paddedImage1.NumberOfChannels);

            CvInvoke.BitwiseXor(paddedImage1, paddedImage2, result);
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


        public static (Mat paddedImage1, Mat paddedImage2, Size size) PaddingImages(Mat image1, Mat image2)
        {
            int width = Math.Max(image1.Width, image2.Width);
            int height = Math.Max(image1.Height, image2.Height);

            Mat paddedImage1 = new Mat();
            Mat paddedImage2 = new Mat();

            int image1TopPadding = (height - image1.Height) / 2;
            int image1BottomPadding = height - image1.Height - image1TopPadding;
            int image1LeftPadding = (width - image1.Width) / 2;
            int image1RightPadding = width - image1.Width - image1LeftPadding;

            int image2TopPadding = (height - image2.Height) / 2;
            int image2BottomPadding = height - image2.Height - image2TopPadding;
            int image2LeftPadding = (width - image2.Width) / 2;
            int image2RightPadding = width - image2.Width - image2LeftPadding;

            CvInvoke.CopyMakeBorder(image1, paddedImage1, image1TopPadding, image1BottomPadding, image1LeftPadding, image1RightPadding, BorderType.Constant, new MCvScalar(0));
            CvInvoke.CopyMakeBorder(image2, paddedImage2, image2TopPadding, image2BottomPadding, image2LeftPadding, image2RightPadding, BorderType.Constant, new MCvScalar(0));

            return (paddedImage1, paddedImage2, paddedImage1.Size);
        }

        public static Matrix<float> ConvolveKernels(Matrix<float> kernel1, Matrix<float> kernel2)
        {
            int finalSize = 5;
            Matrix<float> result = new Matrix<float>(finalSize, finalSize);

            for (int x = 0; x < finalSize; x++)
            {
                for (int y = 0; y < finalSize; y++)
                {
                    float sum = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            int ni = x - i;
                            int nj = y - j;
                            if (ni >= 0 && ni < 3 && nj >= 0 && nj < 3)
                            {
                                sum += kernel1[i, j] * kernel2[ni, nj];
                            }
                        }
                    }
                    result[x, y] = sum;
                }
            }

            return result;
        }

        public static (Mat squareElement, Mat diamondElement) CreateStructuringElements(int squareSize, int diamondSize)
        {
            Mat squareElement = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(squareSize, squareSize), new Point(-1, -1));

            Mat diamondElement = new Mat(diamondSize, diamondSize, DepthType.Cv8U, 1);
            diamondElement.SetTo(new MCvScalar(0));
            int center = diamondSize / 2;

            for (int i = 0; i <= center; i++)
            {
                int offset = center - i;
                diamondElement.GetData().SetValue((byte)255, center + i, center - offset);
                diamondElement.GetData().SetValue((byte)255, center - i, center + offset);
                diamondElement.GetData().SetValue((byte)255, center - i, center - offset);
                diamondElement.GetData().SetValue((byte)255, center + i, center + offset);
            }

            return (squareElement, diamondElement);
        }

        public static Mat CreateDiamondElement(int size)
        {
            int center = size / 2;
            Mat diamond = new Mat(size, size, DepthType.Cv8U, 1);
            diamond.SetTo(new MCvScalar(0));

            IntPtr dataPointer = diamond.DataPointer;
            int step = diamond.Step;

            for (int i = 0; i <= center; i++)
            {
                for (int j = center - i; j <= center + i; j++)
                {
                    Marshal.WriteByte(dataPointer, i * step + j, 1);
                    Marshal.WriteByte(dataPointer, (size - i - 1) * step + j, 1);
                }
            }

            return diamond;
        }

        public static Mat GetStructuralElement(string shape, int size)
        {
            Mat element;

            switch (shape.ToLower())
            {
                case "diamond":
                    element = CreateDiamondElement(size);
                    break;
                case "square":
                    element = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(size, size), new Point(-1, -1));
                    break;
                default:
                    element = CreateDiamondElement(size);
                    break;
            }

            return element;
        }
        public static Mat Erode(Mat image, Mat structuringElement)
        {
            Mat result = new Mat();
            CvInvoke.Erode(image, result, structuringElement, new Point(-1, -1), 1, BorderType.Constant, new MCvScalar(0));
            return result;
        }
        public static Mat Dilate(Mat image, Mat structuringElement)
        {
            Mat result = new Mat();
            CvInvoke.Dilate(image, result, structuringElement, new Point(-1, -1), 1, BorderType.Constant, new MCvScalar(0));
            return result;
        }
        public static Mat Open(Mat image, Mat structuringElement)
        {
            Mat result = new Mat();
            CvInvoke.MorphologyEx(image, result, MorphOp.Open, structuringElement, new Point(-1, -1), 1, BorderType.Constant, new MCvScalar(0));
            return result;
        }
        public static Mat Close(Mat image, Mat structuringElement)
        {
            Mat result = new Mat();
            CvInvoke.MorphologyEx(image, result, MorphOp.Close, structuringElement, new Point(-1, -1), 1, BorderType.Constant, new MCvScalar(0));
            return result;
        }

        public static Mat Skeletonize(Mat image, Mat structuringElement)
        {
            Mat skel = new Mat(image.Size, DepthType.Cv8U, 1);
            skel.SetTo(new MCvScalar(0));
            Mat temp = new Mat();
            Mat eroded = new Mat();

            do
            {
                eroded = Erode(image, structuringElement);
                temp = Open(eroded, structuringElement);
                CvInvoke.Subtract(eroded, temp, temp);
                CvInvoke.BitwiseOr(skel, temp, skel);
                eroded.CopyTo(image);
            } while (CvInvoke.CountNonZero(eroded) != 0);

            return skel;
        }

        public static Mat ImagePyramidUp(Mat image)
        {
            Mat result = new Mat();
            CvInvoke.PyrUp(image, result);
            return result;
        }

        public static Mat ImagePyramidDown(Mat image)
        {
            Mat result = new Mat();
            CvInvoke.PyrDown(image, result);
            return result;
        }

        public static void SaveImage(Mat image)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JPEG Image|*.jpg|PNG Image|*.png|Bitmap Image|*.bmp";
            saveFileDialog.Title = "Zapisz obraz jako";
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName != "")
            {
                CvInvoke.Imwrite(saveFileDialog.FileName, image);
            }
        }

        public static void SaveCompressedImage(Mat image)
        {
            if (image == null || image.IsEmpty)
                throw new ArgumentException("Obraz jest pusty.");

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Binary files|*.bin";
            saveFileDialog.Title = "Zapisz skompresowany obraz jako plik binarny";
            saveFileDialog.ShowDialog();

            if (!string.IsNullOrEmpty(saveFileDialog.FileName))
            {
                byte[] data = new byte[image.Cols]; 
                Marshal.Copy(image.DataPointer, data, 0, data.Length);
                System.IO.File.WriteAllBytes(saveFileDialog.FileName, data);
            }
        }

        public static Mat ApplyHoughTransform(Mat image)
        {
            CvInvoke.GaussianBlur(image, image, new Size(3, 3), 1);

            Mat edges = new Mat();
            CvInvoke.Canny(image, edges, 50, 150);

            LineSegment2D[] lines = CvInvoke.HoughLinesP(edges, 1, Math.PI / 180, 50, 30, 10);

            foreach (var line in lines)
            {
                CvInvoke.Line(image, line.P1, line.P2, new MCvScalar(0, 255, 0), 2, LineType.EightConnected);
            }

            return image;
        }

        public static Mat ManualThreshold(Mat inputImage, double thresholdValue)
        {
            Mat outputImage = new Mat();
            CvInvoke.Threshold(inputImage, outputImage, thresholdValue, 255, ThresholdType.Binary);
            return outputImage;
        }

        public static Mat AdaptiveThreshold(Mat inputImage)
        {
            Mat outputImage = new Mat();
            CvInvoke.AdaptiveThreshold(inputImage, outputImage, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 11, 2);
            return outputImage;
        }

        public static (Mat, double) OtsuThreshold(Mat inputImage)
        {
            Mat outputImage = new Mat();
            double otsuThreshold = CvInvoke.Threshold(inputImage, outputImage, 0, 255, ThresholdType.Otsu | ThresholdType.Binary);
            return (outputImage, otsuThreshold);
        }

        public static Mat PerformInpainting(Mat originalImage, Mat mask)
        {
            Mat inpaintedImage = new Mat();
            CvInvoke.Inpaint(originalImage, mask, inpaintedImage, 3, InpaintType.NS);
            return inpaintedImage;
        }

        public static Mat PerformGrabCutWithMask(Mat originalImage, Mat maskImage)
        {
            Mat formattedImage = EnsureImageIsCv8Uc3(originalImage);

            Mat grabCutMask = ConvertToGrabCutMask(maskImage);

            Mat bgModel = new Mat();
            Mat fgModel = new Mat();
            Rectangle roi = DetectROI(grabCutMask);

            CvInvoke.GrabCut(formattedImage, grabCutMask, roi, bgModel, fgModel, 5, GrabcutInitType.InitWithMask);

            Mat foreground = new Mat(formattedImage.Size, DepthType.Cv8U, 3);
            foreground.SetTo(new MCvScalar(255, 255, 255));
            formattedImage.CopyTo(foreground, grabCutMask);

            return foreground;
        }

        private static Mat EnsureImageIsCv8Uc3(Mat image)
        {
            if (image.NumberOfChannels != 3 || image.Depth != DepthType.Cv8U)
            {
                Mat result = new Mat();
                if (image.NumberOfChannels == 1)
                {
                    CvInvoke.CvtColor(image, result, ColorConversion.Gray2Bgr);
                }
                else
                {
                    image.ConvertTo(result, DepthType.Cv8U);
                    if (result.NumberOfChannels == 1)
                    {
                        CvInvoke.CvtColor(result, result, ColorConversion.Gray2Bgr);
                    }
                }
                return result;
            }
            return image;
        }

        private static Mat ConvertToGrabCutMask(Mat inputMask)
        {
            Mat grabCutMask = new Mat();
            if (inputMask.Depth != DepthType.Cv8U)
            {
                inputMask.ConvertTo(grabCutMask, DepthType.Cv8U);
            }
            else
            {
                grabCutMask = inputMask.Clone();
            }

            unsafe
            {
                byte* dataPtr = (byte*)grabCutMask.DataPointer;
                int step = grabCutMask.Step;

                for (int y = 0; y < grabCutMask.Rows; y++)
                {
                    for (int x = 0; x < grabCutMask.Cols; x++)
                    {
                        byte* rowPtr = dataPtr + (y * step);
                        byte pixelValue = rowPtr[x];

                        if (pixelValue == 0)
                        {
                            rowPtr[x] = 0;
                        }
                        else if (pixelValue == 255)
                        {
                            rowPtr[x] = 1;
                        }
                    }
                }
            }

            return grabCutMask;
        }

        private static Rectangle DetectROI(Mat mask)
        {
            Rectangle roi = Rectangle.Empty;
            var handle = GCHandle.Alloc(mask.GetData(), GCHandleType.Pinned);
            try
            {
                IntPtr scan0 = handle.AddrOfPinnedObject();
                byte depth = mask.Depth == DepthType.Cv8U ? (byte)1 :
                            mask.Depth == DepthType.Cv16U || mask.Depth == DepthType.Cv16S ? (byte)2 : (byte)4;

                int stride = mask.Step;
                for (int y = 0; y < mask.Rows; y++)
                {
                    for (int x = 0; x < mask.Cols; x++)
                    {
                        byte value = Marshal.ReadByte(scan0, y * stride + x * depth);
                        if (value != 0)
                        {
                            if (roi == Rectangle.Empty)
                            {
                                roi = new Rectangle(x, y, 1, 1);
                            }
                            else
                            {
                                roi = Rectangle.Union(roi, new Rectangle(x, y, 1, 1));
                            }
                        }
                    }
                }
            }
            finally
            {
                if (handle.IsAllocated)
                    handle.Free();
            }

            return roi;
        }

        public static (Mat, double) CompressImage(Mat image)
        {
            List<byte> compressed = new List<byte>();
            byte[] imageData = new byte[image.Step * image.Rows];
            image.CopyTo(imageData);

            byte currentPixel = imageData[0];
            int count = 1;

            for (int i = 1; i < imageData.Length; i++)
            {
                if (imageData[i] == currentPixel)
                {
                    count++;
                }
                else
                {
                    compressed.Add(currentPixel);
                    compressed.AddRange(BitConverter.GetBytes(count));
                    currentPixel = imageData[i];
                    count = 1;
                }
            }

            compressed.Add(currentPixel);
            compressed.AddRange(BitConverter.GetBytes(count));

            Mat compressedMat = new Mat(new Size(compressed.Count, 1), DepthType.Cv8U, 1);
            compressedMat.SetTo(compressed.ToArray());

            double compressionRatio = (double)compressed.Count / imageData.Length;
            return (compressedMat, compressionRatio);
        }




        public static Mat ApplyWatershed(Mat image)
        {
            Mat coloredImage = new Mat();
            if (image.NumberOfChannels == 1)
            {
                CvInvoke.CvtColor(image, coloredImage, ColorConversion.Gray2Bgr);
            }
            else
            {
                coloredImage = image.Clone();
            }

            Mat gray = new Mat();
            CvInvoke.CvtColor(coloredImage, gray, ColorConversion.Bgr2Gray);

            Mat binary = new Mat();
            CvInvoke.Threshold(gray, binary, 0, 255, ThresholdType.BinaryInv | ThresholdType.Otsu);

            Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));
            Mat opening = new Mat();
            CvInvoke.MorphologyEx(binary, opening, MorphOp.Open, kernel, new Point(-1, -1), 2, BorderType.Default, new MCvScalar());

            Mat sureBg = new Mat();
            CvInvoke.Dilate(opening, sureBg, kernel, new Point(-1, -1), 3, BorderType.Default, new MCvScalar());

            Mat distTransform = new Mat();
            CvInvoke.DistanceTransform(opening, distTransform, null, DistType.L2, 5);
            double minVal = 0;
            double maxVal = 0;
            Point minLoc = new Point();
            Point maxLoc = new Point();
            CvInvoke.MinMaxLoc(distTransform, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

            Mat sureFg = new Mat();
            CvInvoke.Threshold(distTransform, sureFg, 0.7 * maxVal, 255, ThresholdType.Binary);
            sureFg.ConvertTo(sureFg, DepthType.Cv8U);

            Mat unknown = new Mat();
            CvInvoke.Subtract(sureBg, sureFg, unknown);

            Mat markers = new Mat();
            CvInvoke.ConnectedComponents(sureFg, markers);
            markers += 1;

            Mat zeros = new Mat();
            markers.CopyTo(zeros);
            zeros.SetTo(new MCvScalar(0));
            zeros.CopyTo(markers, unknown);

            CvInvoke.Watershed(coloredImage, markers);

            Mat result = new Mat(markers.Size, DepthType.Cv8U, 1);
            result.SetTo(new MCvScalar(0));

            Mat mask = new Mat();
            zeros.SetTo(new MCvScalar(-1));
            CvInvoke.Compare(markers, zeros, mask, CmpType.Equal);
            mask.ConvertTo(mask, DepthType.Cv8U);

            return mask;
        }

        public static Tuple<Mat, List<AnalysisResult>> AnalyzeImage(Mat inputImage)
        {
            List<AnalysisResult> results = new List<AnalysisResult>();
            Mat img = inputImage.Clone();
            Mat cntImg = new Mat(img.Size, DepthType.Cv8U, 3);

            Mat grayImage = new Mat();
            if (img.NumberOfChannels != 1)
            {
                CvInvoke.CvtColor(img, grayImage, ColorConversion.Bgr2Gray);
            }
            else
            {
                grayImage = img;
            }

            Mat binary = new Mat();
            CvInvoke.Threshold(grayImage, binary, 127, 255, ThresholdType.Binary);

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(binary, contours, null, RetrType.List, ChainApproxMethod.ChainApproxNone);

            for (int i = 0; i < contours.Size; i++)
            {
                Random r = new Random();
                CvInvoke.DrawContours(cntImg, new VectorOfVectorOfPoint(contours[i]), -1, new MCvScalar(r.Next(255), r.Next(255), r.Next(255)), 1, LineType.EightConnected, null, -1, default);
                double area = CvInvoke.ContourArea(contours[i]);
                double perimeter = CvInvoke.ArcLength(contours[i], true);
                Rectangle rect = CvInvoke.BoundingRectangle(contours[i]);
                double aspectRatio = (double)rect.Width / (double)rect.Height;
                double extent = (double)area / (double)(rect.Width * rect.Height);

                var vf = new PointF[contours[i].Size];
                for (int ii = 0; ii < contours[i].Size; ii++) vf[ii] = contours[i][ii];
                var hull = CvInvoke.ConvexHull(vf);
                double hull_area = CvInvoke.ContourArea(new VectorOfPointF(hull), true);
                double solidity = area / hull_area;
                double equivalentDiameter = Math.Sqrt(4 * area / Math.PI);

                results.Add(new AnalysisResult
                {
                    No = i,
                    Moments = contours[i].Size,
                    Area = area,
                    Perimeter = perimeter,
                    AspectRatio = aspectRatio,
                    Extent = extent,
                    Solidity = solidity,
                    EquivalentDiameter = equivalentDiameter
                });
            }

            return new Tuple<Mat, List<AnalysisResult>>(cntImg, results);
        }


        public static int CountObjectsFromBinaryMap(Mat binaryImage)
        {
            Mat componentLabels = new Mat();
            int numberOfLabels = CvInvoke.ConnectedComponents(binaryImage, componentLabels);

            return numberOfLabels - 1;
        }
    }
}
