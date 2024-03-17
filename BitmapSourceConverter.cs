﻿using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Windows;

namespace Image_Manipulation_App
{
    internal static class BitmapSourceConverter
    {
        public static BitmapSource ToBitmapSource(Mat mat)
        {
            return Imaging.CreateBitmapSourceFromHBitmap(
                mat.ToBitmap().GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
    }
}
