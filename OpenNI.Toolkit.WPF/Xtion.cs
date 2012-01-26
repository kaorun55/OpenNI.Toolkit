using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace OpenNI.Toolkit.WPF
{
    public class Xtion
    {
        private Context context;

        public event EventHandler<VideoFrameReadyEventArgs> VideoFrameReady;
        public event EventHandler<DepthFrameReadyEventArgs> DepthFrameReady;
        public event EventHandler<SkeletonFrameReadyEventArgs> SkeletonFrameReady;

        private Thread readerThread;
        private bool shouldRun = true;

        private const int Width = 640;
        private const int Height = 480;
        private const int FPS = 30;

        public Xtion()
        {
            Trace.WriteLine( Assembly.GetExecutingAssembly().FullName );

            context = new Context();
            context.GlobalMirror = true;

            Image = new ImageGenerator( context );
            Image.NewDataAvailable += new EventHandler( ImageGenrator_NewDataAvailable );
            Image.MapOutputMode = new MapOutputMode()
            {
                XRes = Width,
                YRes = Height,
                FPS = FPS
            };

            Depth = new DepthGenerator( context );
            Depth.NewDataAvailable += new EventHandler( DepthGenerator_NewDataAvailable );
            Depth.MapOutputMode = new MapOutputMode()
            {
                XRes = Width,
                YRes = Height,
                FPS = FPS
            };

            User = new UserGenerator( context );
            User.NewUser += new EventHandler<NewUserEventArgs>( UserGenerator_NewUser );
            User.LostUser += new EventHandler<UserLostEventArgs>( UserGenerator_LostUser );
            User.UserExit += new EventHandler<UserExitEventArgs>( UserGenerator_UserExit );
            User.UserReEnter += new EventHandler<UserReEnterEventArgs>( UserGenerator_UserReEnter );

            User.NewDataAvailable += new EventHandler( UserGenerator_NewDataAvailable );

            Skeleton = User.SkeletonCapability;
            if ( Skeleton.DoesNeedPoseForCalibration ) {
                throw new Exception( "OpenNI 1.4.0.2 以降をインストールしてください" );
            }

            Skeleton.CalibrationComplete += new EventHandler<CalibrationProgressEventArgs>( Skeleton_CalibrationComplete );
            Skeleton.SetSkeletonProfile( SkeletonProfile.HeadAndHands );
            Skeleton.SetSmoothing( 0.7f );

            // 画像更新のためのスレッドを作成
            shouldRun = true;
            readerThread = new Thread( new ThreadStart( () =>
            {
                while ( shouldRun ) {
                    context.WaitAndUpdateAll();
                }
            } ) );
            readerThread.Start();

            context.StartGeneratingAll();
        }

        ~Xtion()
        {
            Shutdown();
        }

        void DepthGenerator_NewDataAvailable( object sender, EventArgs e )
        {
            if ( (DepthFrameReady != null) && (Depth.IsDataNew) ) {
                DepthFrameReady( this, new DepthFrameReadyEventArgs()
                {
                    DepthGenerator = Depth
                } );
            }
        }

        void ImageGenrator_NewDataAvailable( object sender, EventArgs e )
        {
            if ( VideoFrameReady != null ) {
                VideoFrameReady( this, new VideoFrameReadyEventArgs()
                {
                    ImageGenerator = Image
                } );
            }
        }

        void UserGenerator_NewDataAvailable( object sender, EventArgs e )
        {
            var users = User.GetUsers();
            if ( (SkeletonFrameReady != null) && (users.Length != 0) ) {
                SkeletonFrameReady( this, new SkeletonFrameReadyEventArgs()
                {
                    Skeleton = Skeleton,
                    Users = users,
                    DepthGenerator = Depth,
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
            Trace.WriteLine( System.Reflection.MethodBase.GetCurrentMethod().Name + " New!!" );
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

        public ImageGenerator Image
        {
            get;
            private set;
        }

        public DepthGenerator Depth
        {
            get;
            private set;
        }

        public UserGenerator User
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
