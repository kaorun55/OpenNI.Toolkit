using System;

namespace OpenNI.Toolkit.WPF
{
    public sealed class VideoFrameReadyEventArgs : EventArgs
    {
        public ImageGenerator ImageGenerator
        {
            get;
            set;
        }
    }

    public sealed class DepthFrameReadyEventArgs : EventArgs
    {
        public DepthGenerator DepthGenerator
        {
            get;
            set;
        }
    }

    public sealed class SkeletonFrameReadyEventArgs : EventArgs
    {
        public SkeletonCapability Skeleton
        {
            get;
            set;
        }

        public int[] Users
        {
            get;
            set;
        }

        public DepthGenerator DepthGenerator
        {
            get;
            set;
        }
    }
}
