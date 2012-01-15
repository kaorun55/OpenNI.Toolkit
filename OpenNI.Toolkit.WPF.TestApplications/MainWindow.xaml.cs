using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Threading;
using OpenNI.Toolkit.WPF;

namespace OpenNI.Toolkit.WPF.TestApplications
{
    public sealed class VideoFrameReadyEventArgs : EventArgs
    {
        public ImageGenerator ImageGenerator
        {
            get;
            set;
        }
    }

    public class Xtion
    {
        private Context context;
        private DepthGenerator depth;
        private UserGenerator user;

        public event EventHandler<VideoFrameReadyEventArgs> VideoFrameReady;


        private Thread readerThread;
        private bool shouldRun = true;

        public Xtion( string xmlFile )
        {
            ScriptNode script;
            context = Context.CreateFromXmlFile( "SamplesConfig.xml", out script );

            foreach ( var node in context.EnumerateExistingNodes() ) {
                if ( node.Instance is ImageGenerator ) {
                    ImageGenrator = node.Instance as ImageGenerator;
                }
                else if ( node.Instance is DepthGenerator ) {
                    depth = node.Instance as DepthGenerator;
                }
            }

            // 画像更新のためのスレッドを作成
            shouldRun = true;
            readerThread = new Thread( new ThreadStart( () =>
            {
                while ( shouldRun ) {
                    context.WaitAndUpdateAll();

                    if ( VideoFrameReady != null ) {
                        VideoFrameReady( this, new VideoFrameReadyEventArgs()
                        {
                            ImageGenerator = ImageGenrator
                        } );
                    }
                }
            } ) );
            readerThread.Start();
        }

        ~Xtion()
        {
            shouldRun = false;
            readerThread.Join();
        }

        public ImageGenerator ImageGenrator
        {
            get;
            private set;
        }
    }

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
                xtion = new Xtion( "SamplesConfig.xml" );
                xtion.VideoFrameReady += new EventHandler<VideoFrameReadyEventArgs>( xtion_VideoFrameReady );
            }
            catch ( Exception ex ) {
                MessageBox.Show( ex.Message );
            }
        }

        void xtion_VideoFrameReady( object sender, VideoFrameReadyEventArgs e )
        {
            // ImageMetaDataをBitmapSourceに変換する(unsafeにしなくてもOK!!)
            this.Dispatcher.BeginInvoke( DispatcherPriority.Background, new Action( () =>
            {
                imageRgb.Source = e.ImageGenerator.ToBitmapSource();
            } ) );
        }

        private void Window_Closing( object sender, System.ComponentModel.CancelEventArgs e )
        {
        }

        private void buttonSave_Click( object sender, RoutedEventArgs e )
        {
            xtion.ImageGenrator.ToBitmapSource().Save( "image.png", ImageFormat.Png );
        }
    }
}
