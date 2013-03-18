using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Microsoft.Kinect;


namespace WpfApplication1
{


    /// <summary>
    /// Holds a series of joint positions through time from the sensor.
    /// Will give an instance of a given gesture
    /// </summary>
    class GestureRecording
    {
        private long totalTime; //We store the total time of the recording
        private Stopwatch sw = new Stopwatch(); //We use the stopwatch object to do this

        //We record the positions of the upper body joints given by kinect
        public List<Tuple<float, float, float>> hand_right        {get; private set;}
        public List<Tuple<float, float, float>> wrist_right       {get; private set;}
        public List<Tuple<float, float, float>> elbow_right       {get; private set;}
        public List<Tuple<float, float, float>> shoulder_right    {get; private set;}
        public List<Tuple<float, float, float>> hand_left         {get; private set;}
        public List<Tuple<float, float, float>> wrist_left        {get; private set;}
        public List<Tuple<float, float, float>> elbow_left        {get; private set;}
        public List<Tuple<float, float, float>> shoulder_left     {get; private set;}
        public List<Tuple<float, float, float>> shoulder_centre   {get; private set;}
        public List<Tuple<float, float, float>> head              { get; private set; }

        //given a joint and a dimension (0=x, 1=y, 2=z) returns as a string the position sequence of that joint in that dimension
        public String asString(List<Tuple<float, float, float>> jointlist, int dimension)
        {
            
            StringBuilder builder = new StringBuilder();
            bool firstColumn = true;
            if (dimension == 0)
            {
                foreach (Tuple<float, float, float> pos in jointlist)
                {
                    if (firstColumn)
                    {
                        builder.Append(pos.Item1.ToString());
                        firstColumn = false;
                    }
                    else
                    {
                        builder.Append(",");
                        builder.Append(pos.Item1.ToString());
                    }
                }
            }

            return builder.ToString();
        }
        
        public GestureRecording()
        {
            hand_right         = new List<Tuple<float, float, float>>();
            wrist_right        = new List<Tuple<float, float, float>>();
            elbow_right        = new List<Tuple<float, float, float>>();
            shoulder_right     = new List<Tuple<float, float, float>>();
            hand_left          = new List<Tuple<float, float, float>>();
            wrist_left         = new List<Tuple<float, float, float>>();
            elbow_left         = new List<Tuple<float, float, float>>();
            shoulder_left      = new List<Tuple<float, float, float>>();
            shoulder_centre    = new List<Tuple<float, float, float>>();
            head               = new List<Tuple<float, float, float>>();
            sw.Stop(); //Start timing
        }

        //Takes a skeleton and adds it's position to the current recording
        public void addReading(Skeleton skeleton)
        {
            hand_right.Add(makeTuple(skeleton.Joints[JointType.HandRight].Position));
            wrist_right.Add(makeTuple(skeleton.Joints[JointType.WristRight].Position));
            elbow_right.Add(makeTuple(skeleton.Joints[JointType.ElbowRight].Position));
            shoulder_right.Add(makeTuple(skeleton.Joints[JointType.ShoulderRight].Position));
            hand_left.Add(makeTuple(skeleton.Joints[JointType.HandLeft].Position));
            wrist_left.Add(makeTuple(skeleton.Joints[JointType.WristLeft].Position));
            elbow_left.Add(makeTuple(skeleton.Joints[JointType.ElbowLeft].Position));
            shoulder_left.Add(makeTuple(skeleton.Joints[JointType.ShoulderLeft].Position));
            shoulder_centre.Add(makeTuple(skeleton.Joints[JointType.ShoulderCenter].Position));
            head.Add(makeTuple(skeleton.Joints[JointType.Head].Position));
        }

        private Tuple<float, float, float> makeTuple(SkeletonPoint sp)
        {
            return new Tuple<float, float, float>(sp.X, sp.Y, sp.Z); 
        }

        public void finish()
        {
            sw.Stop();
            totalTime = sw.ElapsedMilliseconds;
        }

    }
}
