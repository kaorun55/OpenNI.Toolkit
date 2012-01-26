
namespace OpenNI.Toolkit.WPF
{
    public static class SkeletonExtension
    {
        public static Point3D ScaleTo( this Point3D point, DepthGenerator depth, int width, int height )
        {
            return new Point3D( (point.X * width) / depth.MapOutputMode.XRes, (point.Y * height) / depth.MapOutputMode.YRes, point.Z );
        }
    }

}
