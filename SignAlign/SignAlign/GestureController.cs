using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Samples.Kinect.WpfViewers;

namespace WpfApplication1
{
    /// <summary>
    /// Handles the kinect sensor - gets a skeleton for gesture recognition
    /// </summary>
    class GestureController
    {
        private KinectSensor kinectSensor; //The sensor used for skeletal tracking
        private Skeleton[] skeletonData = new Skeleton[6]; //An array of skeletons given by the sensor
        public GestureController()
        {
            // Walk through KinectSensors to find the first one with a Connected status
            var firstKinect = (from k in KinectSensor.KinectSensors
                               where k.Status == KinectStatus.Connected
                               select k).FirstOrDefault();
            if (firstKinect != null)
            {
                kinectSensor = firstKinect;
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

        /// <summary>
        /// Opens a new kinect sensor for skeletal tracking
        /// </summary>
        /// <param name="newKinect">the new sensor</param>
        private void openKinect(KinectSensor newKinect)
        {
            kinectSensor = newKinect;
            newKinect.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            // Activate the SkeletonStream with default smoothing.
            newKinect.SkeletonStream.Enable();
            // Subscribe to the SkeletonFrameReady event to know when data is available
            newKinect.SkeletonFrameReady += Kinect_SkeletonFrameReady;
            // Starts the sensor
            newKinect.Start();
        }

        /// <summary>
        /// Handles the SkeletonFrameReady event of the newKinect control - called for each new available skeleton
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Microsoft.Kinect.SkeletonFrameReadyEventArgs"/> instance containing the event data.</param>
        void Kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            // Opens the received SkeletonFrame
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                // Skeleton frame might be null if we are too late or if the Kinect has been stopped
                if (skeletonFrame == null)
                    return;

                // Copies the data in a Skeleton array (6 items) 
                skeletonFrame.CopySkeletonDataTo(skeletonData)

                // Retrieves Skeleton objects with Tracked state
                var trackedSkeletons = skeletonData.Where(s => s.TrackingState == SkeletonTrackingState.Tracked);
            }
        }

    }
}
