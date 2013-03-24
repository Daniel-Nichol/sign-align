using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace SignAlign
{
    /// <summary>
    /// Handles the kinect sensor - gets a skeleton for gesture recognition
    /// </summary>
    public class GestureController
    {
        public KinectSensor kinectSensor { get; private set; } //The sensor used for skeletal tracking
        protected bool tracking;
        public GestureController() //
        {
            // Walk through KinectSensors to find the first one with a Connected status
            var firstKinect = (from k in KinectSensor.KinectSensors
                               where k.Status == KinectStatus.Connected
                               select k).FirstOrDefault();
            if (firstKinect != null)
            {
                kinectSensor = firstKinect;
            }
            //Enable the available streams
            kinectSensor.SkeletonStream.Enable();
            //kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            //kinectSensor.DepthStream.Enable();
            //Wait for the frames of all 3 streams to be read. Then we will call AllFramesReady
            kinectSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(KinectAllFramesReady);
            kinectSensor.Start();
        }

        public bool isTracking()
        {
            return tracking;
        }

        /// <summary>
        /// Called each time new frames are ready
        /// </summary>
        protected virtual void KinectAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            Skeleton[] skeletonData;
            SkeletonFrame skeletonFrame;

            skeletonData = new Skeleton[kinectSensor.SkeletonStream.FrameSkeletonArrayLength];
            skeletonFrame = e.OpenSkeletonFrame();
            if (skeletonFrame != null)
            {
                skeletonFrame.CopySkeletonDataTo(skeletonData);
                if (skeletonData[0].TrackingState == SkeletonTrackingState.Tracked)
                {
                    tracking = true;
                }
                else
                {
                    tracking = false;
                }
            }
        }


        /// <summary>
        /// Stops the kinect.
        /// </summary>
        /// <param name="kinectSensor">The kinect sensor.</param>
        private void StopKinect(KinectSensor kinectSensor)
        {
            kinectSensor.Stop();
        }
    }
}
