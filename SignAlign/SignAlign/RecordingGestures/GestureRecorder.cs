using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace SignAlign
{
    class GestureRecorder : GestureController
    {
        private List<GestureRecording> recordings = new List<GestureRecording>(); //A list of recordings
        private GestureRecording currentRecording;
        private Skeleton[] skeletonData = new Skeleton[6]; //An array of skeletons given by the sensor
        private SkeletonFrame skeletonFrame;
        private bool areRecording;

        public GestureRecorder()
        {
            kinectSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(KinectAllFramesReady);
        }

        //Update current recording with kinect readings when frame ready
        protected override void KinectAllFramesReady(object sender, AllFramesReadyEventArgs e)
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
        public void startRecording()
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
        public void saveRecordings(String filename)
        {
            using (var writer = new StreamWriter("C:/Users/user/Desktop/signAlign/"+filename+".csv"))
            {
                foreach(GestureRecording g in recordings)
                {
                    writer.WriteLine(g.asString(g.hand_left, 0));
                }
                writer.Flush();
                writer.Dispose();
            }
        }


    }
}
