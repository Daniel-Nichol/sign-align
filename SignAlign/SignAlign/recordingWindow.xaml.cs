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
using System.Windows.Shapes;
using Microsoft.Kinect;
using Microsoft.Samples.Kinect.WpfViewers;



namespace SignAlign
{
    /// <summary>
    /// Interaction logic for recordingWindow.xaml
    /// </summary>
    public partial class recordingWindow : Window
    {
        //Initialize a recorder
        GestureRecorder recorder;
        public recordingWindow(string gestureName)
        {
            recorder = new GestureRecorder("My", true);
            InitializeComponent();
            recorder.kinectSensor.AllFramesReady+=new EventHandler<AllFramesReadyEventArgs>(updateVis);
            //unsub
            //ellipse1 = new Ellipse();
        }

        private void updateVis(object sender, AllFramesReadyEventArgs e)
        {
            if (recorder.areRecording)
            {
                ellipse1.Fill = new SolidColorBrush(Colors.Red);
            }
            else if (recorder.handsMet)
            {
                ellipse1.Fill = new SolidColorBrush(Colors.Blue);
            }
            else
            {
                ellipse1.Fill = new SolidColorBrush(Colors.Green);
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            recorder.saveRecordings("C:/Users/user/Desktop/signAlign/Data/");
        }





       /* public void allFramesReadyEvent(object sender, AllFramesReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame == null)
                {
                    return;
                }
                byte[] pixels = new byte[colorFrame.PixelDataLength];
                colorFrame.CopyPixelDataTo(pixels);

                int stride = colorFrame.Width * 4;
                image1.Source =
                    BitmapSource.Create(colorFrame.Width, colorFrame.Height,
                    96, 96, PixelFormats.Bgr32, null, pixels, stride);


            }
        }*/


     

    }


}
