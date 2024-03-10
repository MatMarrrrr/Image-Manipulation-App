using Emgu.CV;
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

namespace APO_Mateusz_Marek_20456
{
    internal static class BitmapSourceConverter
    {
        /*
        public static BitmapSource ToBitmapSource(Mat mat)
        {
            Bitmap bitmap = mat.ToBitmap();
            MemoryStream stream = new MemoryStream();

            bitmap.Save(stream, ImageFormat.Bmp);
            stream.Position = 0;

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }
        */
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
