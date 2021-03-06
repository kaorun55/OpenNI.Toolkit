﻿using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OpenNI.Toolkit.WPF
{
    public static class ImageExtensions
    {
        public static BitmapSource ToBitmapSource( this ImageGenerator image )
        {
            ImageMetaData imageMD = image.GetMetaData();
            return BitmapSource.Create( imageMD.XRes, imageMD.YRes,
                               96, 96, PixelFormats.Rgb24, null, imageMD.ImageMapPtr,
                               imageMD.DataSize, imageMD.XRes * imageMD.BytesPerPixel );
        }
    }
}
