using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

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

    public class Xtion
    {
        private Context context;

        public event EventHandler<VideoFrameReadyEventArgs> VideoFrameReady;
        public event EventHandler<DepthFrameReadyEventArgs> DepthFrameReady;
        public event EventHandler<SkeletonFrameReadyEventArgs> SkeletonFrameReady;

        private Thread readerThread;
        private bool shouldRun = true;

        public Xtion( string xmlFile )
        {
            ScriptNode script;
            context = Context.CreateFromXmlFile( xmlFile, out script );

            foreach ( var node in context.EnumerateExistingNodes() ) {
                if ( node.Instance is ImageGenerator ) {
                    ImageGenrator = node.Instance as ImageGenerator;
                    ImageGenrator.NewDataAvailable += new EventHandler( ImageGenrator_NewDataAvailable );
                }
                else if ( node.Instance is DepthGenerator ) {
                    DepthGenerator = node.Instance as DepthGenerator;
                    DepthGenerator.NewDataAvailable += new EventHandler( DepthGenerator_NewDataAvailable );
                }
                else if ( node.Instance is UserGenerator ) {
                    UserGenerator = node.Instance as UserGenerator;
                    UserGenerator.NewUser += new EventHandler<NewUserEventArgs>( UserGenerator_NewUser );
                    UserGenerator.LostUser += new EventHandler<UserLostEventArgs>( UserGenerator_LostUser );
                    UserGenerator.UserExit += new EventHandler<UserExitEventArgs>( UserGenerator_UserExit );
                    UserGenerator.UserReEnter += new EventHandler<UserReEnterEventArgs>( UserGenerator_UserReEnter );

                    UserGenerator.NewDataAvailable += new EventHandler( UserGenerator_NewDataAvailable );

                    Skeleton = UserGenerator.SkeletonCapability;
                    if ( Skeleton.DoesNeedPoseForCalibration ) {
                        throw new Exception( "OpenNI 1.4.0.2 以降をインストールしてください" );
                    }

                    Skeleton.CalibrationComplete += new EventHandler<CalibrationProgressEventArgs>( Skeleton_CalibrationComplete );
                    Skeleton.SetSkeletonProfile( SkeletonProfile.HeadAndHands );
                    Skeleton.SetSmoothing( 0.7f );
                }
            }

            // 画像更新のためのスレッドを作成
            shouldRun = true;
            readerThread = new Thread( new ThreadStart( () =>
            {
                while ( shouldRun ) {
                    context.WaitAndUpdateAll();
                }
            } ) );
            readerThread.Start();
        }

        ~Xtion()
        {
            Shutdown();
        }

        void DepthGenerator_NewDataAvailable( object sender, EventArgs e )
        {
            if ( (DepthFrameReady != null) && (DepthGenerator.IsDataNew) ) {
                DepthFrameReady( this, new DepthFrameReadyEventArgs()
                {
                    DepthGenerator = DepthGenerator
                } );
            }
        }

        void ImageGenrator_NewDataAvailable( object sender, EventArgs e )
        {
            if ( VideoFrameReady != null ) {
                VideoFrameReady( this, new VideoFrameReadyEventArgs()
                {
                    ImageGenerator = ImageGenrator
                } );
            }
        }

        void UserGenerator_NewDataAvailable( object sender, EventArgs e )
        {
            // 骨格の描画
            var users = UserGenerator.GetUsers();
            if ( (SkeletonFrameReady != null) && (users.Length != 0) ) {
                SkeletonFrameReady( this, new SkeletonFrameReadyEventArgs()
                {
                    Skeleton = Skeleton,
                    Users = users,
                    DepthGenerator = DepthGenerator,
                } );
            }
        }

        void UserGenerator_UserReEnter( object sender, UserReEnterEventArgs e )
        {
            Trace.WriteLine( System.Reflection.MethodBase.GetCurrentMethod().Name );
        }

        void UserGenerator_UserExit( object sender, UserExitEventArgs e )
        {
            Trace.WriteLine( System.Reflection.MethodBase.GetCurrentMethod().Name );
        }

        void UserGenerator_LostUser( object sender, UserLostEventArgs e )
        {
            Trace.WriteLine( System.Reflection.MethodBase.GetCurrentMethod().Name );
        }

        void UserGenerator_NewUser( object sender, NewUserEventArgs e )
        {
            Trace.WriteLine( System.Reflection.MethodBase.GetCurrentMethod().Name );
            if ( Skeleton.IsCalibrating( e.ID ) ) {
                Skeleton.AbortCalibration( e.ID );
            }

            Skeleton.RequestCalibration( e.ID, true );
        }

        void Skeleton_CalibrationComplete( object sender, CalibrationProgressEventArgs e )
        {
            if ( e.Status == CalibrationStatus.OK ) {
                Trace.WriteLine( "Calibration complete success." );
                Skeleton.StartTracking( e.ID );
            }
            else {
                Trace.WriteLine( "Calibration complete failed." );
                Skeleton.RequestCalibration( e.ID, true );
            }
        }

        public void Shutdown()
        {
            if ( shouldRun ) {
                shouldRun = false;
                readerThread.Join();
            }
        }

        public ImageGenerator ImageGenrator
        {
            get;
            private set;
        }

        public DepthGenerator DepthGenerator
        {
            get;
            private set;
        }

        public UserGenerator UserGenerator
        {
            get;
            private set;
        }

        public SkeletonCapability Skeleton
        {
            get;
            private set;
        }
    }
}
