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
        public bool areRecording { get; private set; }
        private string gestureName;
        private bool training; //If true record training data, else record test data
        //private bool handsUp = true; //Do we record for hands above the waistw?
        public bool handsMet = false;
        private int minRecordingLength = 5;

        public GestureRecorder(string gestureName, bool training)
        {
            this.gestureName = gestureName;
            this.training = training;
            //kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(KinectAllFramesReady);
        }

        public void setHandsUpTraining(bool handsUp)
        {
            //this.handsUp = handsUp;
        }

        //Update current recording with kinect readings when frame ready
        protected override void KinectAllFramesReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            checkRecordingPos(e);
            
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

        private void checkRecordingPos(SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skelData = new Skeleton[kinectSensor.SkeletonStream.FrameSkeletonArrayLength];
            SkeletonFrame skelFrame = e.OpenSkeletonFrame();
                if (skelFrame != null)
                {
                    skelFrame.CopySkeletonDataTo(skelData);
                    if (skelData[0].TrackingState == SkeletonTrackingState.Tracked)
                    {

                        double[] leftHand = { 
                                        skelData[0].Joints[JointType.HandLeft].Position.X, 
                                        skelData[0].Joints[JointType.HandLeft].Position.Y, 
                                        skelData[0].Joints[JointType.HandLeft].Position.Z 
                                    };
                        double[] rightHand = { 
                                        skelData[0].Joints[JointType.HandRight].Position.X, 
                                        skelData[0].Joints[JointType.HandRight].Position.Y, 
                                        skelData[0].Joints[JointType.HandRight].Position.Z 
                                    };
                        double dist = Math.Sqrt(
                            Math.Pow((rightHand[0] - leftHand[0]), 2)
                            + Math.Pow((rightHand[1] - leftHand[1]), 2)
                            + Math.Pow((rightHand[2] - leftHand[2]), 2));

                        if (skelData[0].Joints[JointType.HandLeft].Position.Y < skelData[0].Joints[JointType.HipLeft].Position.Y)
                        {
                            if (areRecording)
                                stopRecording();
                        }
                        else
                        {
                            if (!areRecording)
                                startRecording();
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
            if (!currentRecording.lengthIsLessThan(minRecordingLength))
            {
                recordings.Add(currentRecording);
            }
        }

        public int getNumberOfFrames()
        {
            if (currentRecording != null)
            {
                return currentRecording.getLength();
            }
            else
            {
                return 0;
            }
        }

        //Saves the current recordings as .csv with joint annotations
        public void saveRecordings(string dataFile, bool absolute)
        {
            string datLoc, datType;
            datType = absolute ? "Absolute/" : "Relative/";
            datLoc = training ? "Training/" : "Test/";

            if (!Directory.Exists(dataFile + datLoc + datType + "/" + gestureName + "/"))
            {
                Directory.CreateDirectory(dataFile + datLoc + datType + "/" + gestureName + "/");
            }

            string saveLoc = dataFile + datLoc + datType;

            foreach (JointType j in GestureRecording.trackedJoints)
            {
                
                using (var writer = new StreamWriter(saveLoc + "/"+gestureName+"/"+j.ToString("G")+"_x"+".csv",true))
                {
                    foreach (GestureRecording g in recordings)
                    {
                        writer.WriteLine(g.asString(j, 0, absolute));
                    }
                    writer.Flush();
                    writer.Dispose();
                }
                using (var writer = new StreamWriter(saveLoc + "/" + gestureName + "/" + j.ToString("G") + "_y" + ".csv", true))
                {
                    foreach (GestureRecording g in recordings)
                    {
                        writer.WriteLine(g.asString(j, 1, absolute));
                    }
                    writer.Flush();
                    writer.Dispose();
                }
                using (var writer = new StreamWriter(saveLoc + "/" + gestureName + "/" + j.ToString("G") + "_z" + ".csv",true))
                {
                    foreach (GestureRecording g in recordings)
                    {
                        writer.WriteLine(g.asString(j, 2, absolute));
                    }
                    writer.Flush();
                    writer.Dispose();
                }
            }
        }

        public void Uninitialize()
        {
            kinectSensor.Stop();
        }


    }
}
