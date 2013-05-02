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
        public MainWindow()
        {
        }

        //Open the training data window
        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            var newWindow = new recordingWindow(textBox1.Text, true);
            newWindow.ShowDialog();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            var newWindow = new recordingWindow(textBox1.Text, false);
            newWindow.ShowDialog();
        }


        private void button3_Click(object sender, RoutedEventArgs e)
        {
            SignClassifier sc = new SignClassifier("C:/Users/user/Desktop/signAlign/Data/", Double.MinValue, true);
            int fn; int fp; double per;
            //per = sc.runTests("C:/Users/user/Desktop/signAlign/Data/Training/Absolute/", out fn, out fp);
            sc.clustersTest();
            //sc.clustersRocTest("Hello", "C:/Users/user/Desktop/signAlign/Data/", 3,10, -1000, 50);
            //double correctPercent = sc.runTests("C:/Users/user/Desktop/signAlign/Data/Test/Absolute/");

            button3.Content = "done";
        }

    }
}
