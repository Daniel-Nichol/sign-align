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
using System.IO;
using System.Windows.Shapes;
using Microsoft.Kinect;

using MathNet.Numerics.LinearAlgebra.Double;


namespace SignAlign
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GestureController controller;
        GestureRecorder recorder;
        public MainWindow()
        {
            controller = new GestureController();
            controller.kinectSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(updateStatusGraphic);
            InitializeComponent();   
        }

        //rudimentary tracking graphic
        public void updateStatusGraphic(object sender, AllFramesReadyEventArgs e)
        {
            if (controller.isTracking())
            {
                kinectStatEllipse.Fill = new SolidColorBrush(Colors.Green);
            }
            else
            {
                kinectStatEllipse.Fill = new SolidColorBrush(Colors.Red);
            }
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            
            SignClassifier sc = new SignClassifier("C:/Users/user/Desktop/signAlign/Data/", -120);



            button1.Content = "finished";

                
         
        }

        private void button2_Click_1(object sender, RoutedEventArgs e)
        {
            recorder = new GestureRecorder();
        }

        private void button3_Click_1(object sender, RoutedEventArgs e)
        {
            recorder.startRecording();
        }
        private void button4_Click(object sender, RoutedEventArgs e)
        {
            recorder.stopRecording();
        }
        private void button5_Click_1(object sender, RoutedEventArgs e)
        {
            recorder.saveRecordings("test");
        }
        
    }
}
