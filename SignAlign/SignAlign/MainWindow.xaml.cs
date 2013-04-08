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
            InitializeComponent();   
        }

        //Open the training data window
        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            var newWindow = new recordingWindow("test");
            newWindow.ShowDialog();
        }
    }
}
