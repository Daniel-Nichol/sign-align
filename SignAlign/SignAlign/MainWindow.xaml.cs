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
            
            HiddenMarkovModel HMM = new DiscreteHiddenMarkovModel(A,B,pi);

            List<IObersvation> observations = new List<IObersvation>();
            observations.Add(new DiscreteObservation(0));
            observations.Add(new DiscreteObservation(0));
            observations.Add(new DiscreteObservation(1));
            observations.Add(new DiscreteObservation(1));

            HMM.reestimateParameters(observations);
            HMM.reestimateParameters(observations);
            HMM.reestimateParameters(observations);
            HMM.reestimateParameters(observations);
            HMM.reestimateParameters(observations);
            double prob = HMM.probObservations(observations);*/


            double[,] sig = new double[2,2];
            sig[0,0] = 0.2; sig[0,1] = 0.3; sig[1,0] = 0.30; sig[1,1] = 1.00;
            DenseMatrix sigMat = new DenseMatrix(sig);

            double[] obs = new double[2];
            double[] mean = new double[2];
            obs[0] = 0; obs[1] = 0;
            mean[0] = 0; mean[1] = 0;

            //CD_HMM cdhmm = new CD_HMM();

            //double prob = cdhmm.queryGuassian(new DenseVector(obs),new DenseVector(mean), sigMat);

           // button1.Content = prob.ToString();
           
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
