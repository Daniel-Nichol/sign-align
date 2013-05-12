using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Microsoft.Kinect;



namespace SignAlign
{


    /// <summary>
    /// Holds a series of joint positions through time from the sensor.
    /// Will give an instance of a given gesture
    /// </summary>
    class GestureRecording
    {
        private long totalTime; //We store the total time of the recording
        private Stopwatch sw = new Stopwatch(); //We use the stopwatch object to do this

        //This is a constanst array which contains those JointTypes which we track (the upper body)
        public static readonly JointType[] trackedJoints = 
        {
            JointType.HandRight, 
            JointType.HandLeft, 
            JointType.WristRight, 
            JointType.WristLeft,
            JointType.ElbowLeft,
            JointType.ElbowRight,
            JointType.ShoulderLeft,
            JointType.ShoulderRight,
            JointType.ShoulderCenter,
            JointType.Head
        };

        private Dictionary<JointType, List<double[]>> jointReadingsAbsolute 
            = new Dictionary<JointType,List<double[]>>();

        private Dictionary<JointType, List<double[]>> jointReadingsHeadRelative
            = new Dictionary<JointType, List<double[]>>();

        public GestureRecording()
        {
            jointReadingsAbsolute = new Dictionary<JointType, List<double[]>>(trackedJoints.Count());
            foreach (JointType j in trackedJoints)
            {
                jointReadingsAbsolute.Add(j, new List<double[]>());
                jointReadingsHeadRelative.Add(j, new List<double[]>());
            }
            sw.Stop(); //Start timing
                                  
            
        }
        //given a joint and a dimension (0=x, 1=y, 2=z) returns as a string the position sequence of that joint in that dimension
        public string asString(JointType j, int dimension, bool absolute)
        {

            StringBuilder builder = new StringBuilder();

            List<double[]> jStream;
            if (absolute)
            {
                jointReadingsAbsolute.TryGetValue(j, out jStream);
            }
            else
            {
                jointReadingsHeadRelative.TryGetValue(j, out jStream);
            }
            
            bool firstColumn = true;
            foreach (double[] pos in jStream)
            {
                if (firstColumn)
                {
                    builder.Append(pos[dimension].ToString());
                    firstColumn = false;
                }
                else
                {
                    builder.Append(",");
                    builder.Append(pos[dimension].ToString());
                }
            }
          
            return builder.ToString();
        }

        public bool lengthIsLessThan(int length)
        {
            List<double[]> reads;
            jointReadingsAbsolute.TryGetValue(trackedJoints[0], out reads);
            return reads.Count < length;
        }

        public int getLength()
        {
            List<double[]> reads;
            jointReadingsAbsolute.TryGetValue(trackedJoints[0], out reads);
            if (reads != null)
            {
                return reads.Count;
            }
            else
            {
                return 0;
            }
        }

        //Takes a skeleton and adds it's position to the current recording
        public void addReading(Skeleton skeleton)
        {
            foreach (JointType j in trackedJoints)
            {
                //Note: the following works as jReadings is 
                //passed by reference. We really do update the value in the hash table.
                List<double[]> jReadings, jReadingsRel;
                jointReadingsAbsolute.TryGetValue(j, out jReadings);
                jointReadingsHeadRelative.TryGetValue(j, out jReadingsRel);
                jReadings.Add(asDoubleArray(skeleton.Joints[j].Position));
                double[] rel = {
                                   skeleton.Joints[JointType.Head].Position.X - skeleton.Joints[j].Position.X,
                                   skeleton.Joints[JointType.Head].Position.Y - skeleton.Joints[j].Position.Y,
                                   skeleton.Joints[JointType.Head].Position.Z - skeleton.Joints[j].Position.Z
                               };
                jReadingsRel.Add(rel);
            }
        }

        private double[] asDoubleArray(SkeletonPoint sp)
        {
            double[] sparr = {sp.X, sp.Y, sp.Z};
            return sparr;
        }

        public void finish()
        {
            sw.Stop();
            totalTime = sw.ElapsedMilliseconds;
        }

    }
}
