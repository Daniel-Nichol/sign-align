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
        public recordingWindow(string gestureName, bool training)
        {
            recorder = new GestureRecorder(gestureName, training);
            InitializeComponent();
            recorder.kinectSensor.AllFramesReady+=new EventHandler<AllFramesReadyEventArgs>(updateVis);
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
            if (e.OpenSkeletonFrame() != null)
            {
                Skeleton[] skelData = new Skeleton[6];
                e.OpenSkeletonFrame().CopySkeletonDataTo(skelData);
                label1.Content = (skelData[0].Joints[JointType.HandLeft].Position.X - skelData[0].Joints[JointType.HandRight].Position.X).ToString();
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            recorder.saveRecordings("C:/Users/user/Desktop/signAlign/Data/", true);
            recorder.saveRecordings("C:/Users/user/Desktop/signAlign/Data/", false);
            button1.IsEnabled = false;
        }
    }


}
