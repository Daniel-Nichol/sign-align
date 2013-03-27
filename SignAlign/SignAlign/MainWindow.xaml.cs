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
           
            /*double[,] A = new double[2,2];
            double[,] B = new double[2, 3];
            double[] pi = new double[2];
            A[0, 0] = 0.7; A[0, 1] = 0.3; A[1, 0] = 0.4; A[1, 1] = 0.6;
            B[0, 0] = 0.1; B[0, 1] = 0.4; B[0, 2] = 0.5; B[1, 0] = 0.7; B[1, 1] = 0.2; B[1, 2] = 0.1;
            pi[0] = 0.6; pi[1] = 0.4;

            int[] obsSeq = new int[4] { 0, 1, 2, 1 };

            int[][] obsSeqs = new int[1][];
            obsSeqs[0] = obsSeq;

            D_HMM HMM = new D_HMM(pi, A, B);

            double prob = HMM.Evaluate(obsSeq, false);
            HMM.Learn(obsSeqs, 100, 0.0);
            double prob2 = HMM.Evaluate(obsSeq, false);
            button1.Content = prob.ToString()+" "+prob2.ToString();*/

            KMeansClassifier classifier = new KMeansClassifier(4);

            //int[] labels = classifier.computeClusters(data, 0);

            //button1.Content = labels.ToString();

         
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
