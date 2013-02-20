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

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GestureController controller;
        public MainWindow()
        {
            controller = new GestureController();
            InitializeComponent();
            
        }


        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            /**
             * TESTING THE HMM
             * double[,] A = new double[2,2];
            double[,] B = new double[2, 3];
            double[] pi = new double[2];
            A[0, 0] = 0.7; A[0, 1] = 0.3; A[1, 0] = 0.4; A[1, 1] = 0.6;
            B[0, 0] = 0.1; B[0, 1] = 0.4; B[0, 2] = 0.5; B[1, 0] = 0.7; B[1, 1] = 0.2; B[1, 2] = 0.1;
            pi[0] = 0.6; pi[1] = 0.4;
            
            HiddenMarkovModel HMM = new HiddenMarkovModel(A,B,pi);

            int[] observations = new int[2]; 
            observations[0] = 1;
            observations[1] = 1;
            double prob = HMM.logProbObservations(observations);
            button1.Content = prob.ToString();   **/
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {

        }
        private void button5_Click(object sender, RoutedEventArgs e)
        {

        }
        private void button3_Click(object sender, RoutedEventArgs e)
        {

        }
        private void button2_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
