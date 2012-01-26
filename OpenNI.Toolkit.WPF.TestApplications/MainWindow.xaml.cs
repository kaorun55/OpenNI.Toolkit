using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace OpenNI.Toolkit.WPF.TestApplications
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        Xtion xtion;

        public MainWindow()
        {
            InitializeComponent();

            try 
            {
                xtion = new Xtion();
                xtion.VideoFrameReady += new EventHandler<VideoFrameReadyEventArgs>( xtion_VideoFrameReady );
                xtion.DepthFrameReady += new EventHandler<DepthFrameReadyEventArgs>( xtion_DepthFrameReady );
                xtion.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>( xtion_SkeletonFrameReady );
            }
            catch ( Exception ex ) {
                MessageBox.Show( ex.Message );
            }
        }

        void xtion_SkeletonFrameReady( object sender, SkeletonFrameReadyEventArgs e )
        {
            this.Dispatcher.BeginInvoke( DispatcherPriority.Background, new Action( () =>
            {
                canvas1.Children.Clear();

                foreach ( var u in e.Users ) {
                    if ( !e.Skeleton.IsTracking( u ) ) {
                        continue;
                    }

                    foreach ( SkeletonJoint s in Enum.GetValues( typeof( SkeletonJoint ) ) ) {
                        if ( !e.Skeleton.IsJointAvailable( s ) || !e.Skeleton.IsJointActive( s ) ) {
                            continue;
                        }

                        var joint = e.Skeleton.GetSkeletonJoint( u, s );
                        var point = e.DepthGenerator.ConvertRealWorldToProjective( joint.Position.Position );
                        point  = point.ScaleTo( e.DepthGenerator, 320, 240 );

                        const int dimeter = 10;
                        const int r = dimeter / 2;
                        canvas1.Children.Add( new Ellipse()
                        {
                            Fill = Brushes.Red,
                            Margin = new Thickness( point.X - r, point.Y - r, point.X + r, point.Y + r ),
                            Height = dimeter,
                            Width = dimeter,
                        } );
                    }
                }
            } ) );
        }

        void xtion_DepthFrameReady( object sender, DepthFrameReadyEventArgs e )
        {
            this.Dispatcher.BeginInvoke( DispatcherPriority.Background, new Action( () =>
            {
                imageDepth.Source = e.DepthGenerator.ToBitmapSource();
            } ) );
        }

        void xtion_VideoFrameReady( object sender, VideoFrameReadyEventArgs e )
        {
            this.Dispatcher.BeginInvoke( DispatcherPriority.Background, new Action( () =>
            {
                imageRgb.Source = e.ImageGenerator.ToBitmapSource();
            } ) );
        }

        private void Window_Closing( object sender, System.ComponentModel.CancelEventArgs e )
        {
            xtion.Shutdown();
        }

        private void buttonSave_Click( object sender, RoutedEventArgs e )
        {
            xtion.Image.ToBitmapSource().Save( "image.png", ImageFormat.Png );
        }
    }
}
