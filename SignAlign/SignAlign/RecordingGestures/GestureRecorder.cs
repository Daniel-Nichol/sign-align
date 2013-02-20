using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace WpfApplication1.Recording
{
    class GestureRecorder
    {
        private KinectSensor kinectSensor; //The sensor to record from
        private List<GestureRecording> recordings = new List<GestureRecording>(); //A list of recording
        private GestureRecording currentRecording;
        private bool areRecording;

        public GestureRecorder(KinectSensor kinectSensor)
        {
            this.kinectSensor = kinectSensor;
            kinectSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(KinectAllFramesReady);
        }

        //Update current recording with kinect readings when frame ready
        private void KinectAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            Skeleton[] skeletonData;
            SkeletonFrame skeletonFrame;
            if (areRecording)
            {
                skeletonData = new Skeleton[kinectSensor.SkeletonStream.FrameSkeletonArrayLength];
                skeletonFrame = e.OpenSkeletonFrame();
                if (skeletonFrame != null)
                {
                    skeletonFrame.CopySkeletonDataTo(skeletonData);
                    if (skeletonData[0].TrackingState == SkeletonTrackingState.Tracked)
                    {
                        currentRecording.addReading(skeletonData[0]);
                    }
                }
            }
        }

        //Starts recording from the kinect
        public void StartRecording()
        {
            areRecording = true;
            currentRecording = new GestureRecording();
        }
        public void stopRecording()
        {
            areRecording = false;
            currentRecording.finish();
            recordings.Add(currentRecording);
        }

        //Saves the current recordings as .csv with joint annotations
        public void saveRecordings()
        {
        }


    }
}
