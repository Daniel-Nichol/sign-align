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

            KMeansClassifier classifier = new KMeansClassifier(8);

            List<double[]> observations = new List<double[]>();
            List<double[]>[] observationSeqs = new List<double[]>[10];
            List<double[]>[] testSeqs = new List<double[]>[10];

            for (int i = 0; i < 10; i++)
            {
                observationSeqs[i] = new List<double[]>();
                testSeqs[i] = new List<double[]>();
            }

            using (StreamReader srx = new StreamReader("C:/Users/user/Desktop/signAlign/circle_x.csv"))
            {
                using (StreamReader sry = new StreamReader("C:/Users/user/Desktop/signAlign/circle_y.csv"))
                {
                    using (StreamReader srz = new StreamReader("C:/Users/user/Desktop/signAlign/circle_z.csv"))
                    {
                        string linex, liney, linez; string[] xrow, yrow, zrow;
                        int count = 0;
                        while (count < 60)
                        {
                            linex = srx.ReadLine();
                            liney = sry.ReadLine();
                            linez = srz.ReadLine();

                            xrow = linex.Split(',');
                            yrow = liney.Split(',');
                            zrow = linez.Split(',');

                            for (int i = 0; i < xrow.Length; i++)
                            {
                                observations.Add(new double[3] { Convert.ToDouble(xrow[i]), Convert.ToDouble(yrow[i]), Convert.ToDouble(zrow[i]) });
                                observationSeqs[i].Add(new double[3] { Convert.ToDouble(xrow[i]), Convert.ToDouble(yrow[i]), Convert.ToDouble(zrow[i]) });
                            }
                            count++;
                        }
                    }
                }
            }
            using (StreamReader srx = new StreamReader("C:/Users/user/Desktop/signAlign/m_x.csv"))
            {
                using (StreamReader sry = new StreamReader("C:/Users/user/Desktop/signAlign/m_y.csv"))
                {
                    using (StreamReader srz = new StreamReader("C:/Users/user/Desktop/signAlign/m_z.csv"))
                    {
                        string linex, liney, linez; string[] xrow, yrow, zrow;
                        int count = 0;
                        while (count < 60)
                        {
                            linex = srx.ReadLine();
                            liney = sry.ReadLine();
                            linez = srz.ReadLine();

                            xrow = linex.Split(',');
                            yrow = liney.Split(',');
                            zrow = linez.Split(',');

                            for (int i = 0; i < xrow.Length; i++)
                            {
                                testSeqs[i].Add(new double[3] { Convert.ToDouble(xrow[i]), Convert.ToDouble(yrow[i]), Convert.ToDouble(zrow[i]) });
                            }
                            count++;
                        }
                    }
                }
            }

            double[][] data = observations.ToArray<double[]>();
            double[][][] obsSeqs = new double[10][][];
            double[][][] tSeqs = new double[10][][];
            for (int i = 0; i < 10; i++)
            {
                obsSeqs[i] = observationSeqs[i].ToArray<double[]>();
                tSeqs[i] = testSeqs[i].ToArray<double[]>();
            }
            double[][] centroids;
            
            int[] labels = classifier.computeClusters(data, 0, out centroids);
            
            //Create A
            double[,] A = new double[12,12];
            for(int i = 0;i<12;i++)
            {
                A[i, i] = 0.5;
                if (i < 11)
                {
                    A[i, i + 1] = 0.5;
                }
                A[11, 11] = 1;
            }
            double[,] B = new double[12, 9];

            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    B[i, j] = (1.0 / 9.0);
                }
            }
            double[] pi = new double[12];
            pi[0] = 1;

            D_HMM dhmm = new D_HMM(pi, A, B, centroids, "test");

            dhmm.Reestimate(obsSeqs, 100, 0);

            double prob = dhmm.Evaluate(tSeqs[1], true);

            dhmm.saveParameters("C:/Users/user/Desktop/signAlign/");

            button1.Content = prob.ToString();

                
         
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
