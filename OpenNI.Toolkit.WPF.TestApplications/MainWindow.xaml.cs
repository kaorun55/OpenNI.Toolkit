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

namespace OpenNI.Toolkit.WPF.TestApplications
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private Thread readerThread;
        private bool shouldRun;

        public MainWindow()
        {
            InitializeComponent();
            try {
                ScriptNode node;
                Context context = Context.CreateFromXmlFile( "SamplesConfig.xml", out node );
                ImageGenerator image = context.FindExistingNode( NodeType.Image ) as ImageGenerator;

                // 画像更新のためのスレッドを作成
                shouldRun = true;
                readerThread = new Thread( new ThreadStart( () =>
                {
                    while ( shouldRun ) {
                        context.WaitAndUpdateAll();

                        // ImageMetaDataをBitmapSourceに変換する(unsafeにしなくてもOK!!)
                        this.Dispatcher.BeginInvoke( DispatcherPriority.Background, new Action( () =>
                        {
                            imageRgb.Source = image.ToBitmapSource();
                        } ) );
                    }
                } ) );
                readerThread.Start();
            }
            catch ( Exception ex ) {
            }
        }

        private void Window_Closing( object sender, System.ComponentModel.CancelEventArgs e )
        {
            shouldRun = false;
        }
    }
}
