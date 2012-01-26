// thans : http://c4fkinect.codeplex.com/

using System;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OpenNI.Toolkit.WPF
{
    public static class DepthExtensions
    {
        public static BitmapSource ToBitmapSource( this DepthGenerator depth )
        {
            var md = depth.GetMetaData();

            int count = md.DataSize / md.BytesPerPixel;
            short[] depthData = new short[count];

            Marshal.Copy( md.DepthMapPtr, depthData, 0, count );
            for ( int i = 0; i < count; i++ ) {
                depthData[i] = CalculateIntensityFromDepth( depthData[i] );

            }

            return BitmapSource.Create( md.XRes, md.YRes, 96, 96, PixelFormats.Gray16, null, depthData, md.XRes * md.BytesPerPixel );
        }

        const float MaxDepthDistance = 10000;
        const float MinDepthDistance = 500; 
        const float MaxDepthDistanceOffset = MaxDepthDistance - MinDepthDistance;

        public static short CalculateIntensityFromDepth( short distance )
        {
            // realDepth is now millimeter
            // transform 13-bit depth information into an 8-bit intensity appropriate
            // for display (we disregard information in most significant bit)
            return (short)(65535 - (65535 * Math.Max( distance - MinDepthDistance, 0 ) / (MaxDepthDistanceOffset)));
        }
    }
}
