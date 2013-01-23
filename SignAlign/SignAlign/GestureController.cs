using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace WpfApplication1
{
    /// <summary>
    /// Handles the kinect sensor - gets a skeleton for gesture recognition
    /// </summary>
    class GestureController
    {
        private KinectSensor kinectSensor; //The sensor used for skeletal tracking
        private Skeleton[] skeletonData = new Skeleton[6]; //An array of skeletons given by the sensor
        private SkeletonFrame skeletonFrame;
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

        /// <summary>
        /// Called each time new frames are ready
        /// </summary>
        private void KinectAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            skeletonData = new Skeleton[kinectSensor.SkeletonStream.FrameSkeletonArrayLength];
            skeletonFrame = e.OpenSkeletonFrame();
            if (skeletonFrame != null)
            {
                skeletonFrame.CopySkeletonDataTo(skeletonData);
                if (skeletonData[0].TrackingState == SkeletonTrackingState.Tracked)
                {
                    //DO SOMETHING WITH THE SKELETON.
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
