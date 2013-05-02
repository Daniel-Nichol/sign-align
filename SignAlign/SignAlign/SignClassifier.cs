using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Kinect;

namespace SignAlign
{
    class SignClassifier
    {
        private List<SignModel> signModels;
        private int clusters = 4;
        private string dataPath; //The location on disk of the data files.
        private double acceptanceThreshold;
        private bool absolute;
        private readonly string[] ignoreList = 
        {
           /* "Blue", "Bus","Cat","Circle","Coffee","Computer",
            "Dark","Day","Green","House","My","Orange","Red",
            "Snow","Swan","Tea","Where","Your" */
        };
        List<string> ignore; 

        public SignClassifier(string dataPath, double acceptanceThreshold, bool absolute)
        {
            this.dataPath = dataPath;
            this.acceptanceThreshold = acceptanceThreshold;
            this.absolute = absolute;
            ignore = new List<string>(ignoreList);
            buildClassifier();
            
        }
        /// <summary>
        /// Given a list of observations sequences, one for each joint, returns the name of the most likely sign
        /// </summary>
        /// <param name="jointObsSeq">a collection of observation sequences, one for each joint.</param>
        /// <returns>The name of the best fitting sign (if one exists) else null</returns>
        public string getSign(Dictionary<string ,double[][]> jointObsSeq)
        {
            string bestSignMatch = "";
            double bestLogProb = double.MinValue;
            double tempLogProb;
            foreach (SignModel sm in signModels)
            {
                tempLogProb = sm.Evaluate(jointObsSeq, true);
                if (tempLogProb > bestLogProb)
                {
                    bestLogProb = tempLogProb;
                    bestSignMatch = sm.name;
                }
            }
            if (bestLogProb > acceptanceThreshold)
            {
                return bestSignMatch;
            }
            else
            {
                return "none";
            }
        }
        
     
        private double[][] buildObsSeq(string x_fileLoc, string y_fileLoc, string z_fileLoc, int testNum)
        {
            List<double[]> obsSeq = new List<double[]>();
            using (StreamReader srx = new StreamReader(x_fileLoc))
            {
                using (StreamReader sry = new StreamReader(y_fileLoc))
                {
                    using (StreamReader srz = new StreamReader(z_fileLoc))
                    {
                        for (int j = 0; j < testNum; j++)
                        {
                            srx.ReadLine(); sry.ReadLine(); srz.ReadLine();
                        }
                        string[] xrow, yrow, zrow;
                        xrow = srx.ReadLine().Split(',');
                        yrow = sry.ReadLine().Split(',');
                        zrow = srz.ReadLine().Split(',');
                        for (int i = 0; i < xrow.Length; i++)
                        {
                           obsSeq.Add(new double[] {Convert.ToDouble(xrow[i]), 
                                Convert.ToDouble(yrow[i]), Convert.ToDouble(zrow[i])});
                        }
                    }
                }
            }
            return obsSeq.ToArray<double[]>();
        }


        private List<double[][]> buildObservationSequence(string folder, JointType j)
        {
            List<double[][]> obsSeqs = new List<double[][]>();
            
            using (StreamReader srx = new StreamReader(folder +"/" + j.ToString() + "_x.csv"))
            {
                using (StreamReader sry = new StreamReader(folder + "/" + j.ToString() + "_y.csv"))
                {
                    using (StreamReader srz = new StreamReader(folder + "/" + j.ToString() + "_z.csv"))
                    {
                        string linex;
                        string[] xrow, yrow, zrow;
                        while ((linex = srx.ReadLine()) != null)
                        {
                            double[][] obsSeq = null;
                            xrow = linex.Split(',');
                            yrow = sry.ReadLine().Split(',');
                            zrow = srz.ReadLine().Split(',');
                            obsSeq = new double[xrow.Length][];
                            for (int i = 0; i < xrow.Length; i++)
                            {
                                obsSeq[i] = new double[] {Convert.ToDouble(xrow[i]), 
                                Convert.ToDouble(yrow[i]), Convert.ToDouble(zrow[i])};
                            }
                            obsSeqs.Add(obsSeq);
                        }
                       
                    }
                }
            }
            return obsSeqs;
        }

        /// <summary>
        /// Instantiates a new signModel for each folder in ~/dataPath/Training/, which should contain a folder of training data for each sign.
        /// 
        /// Note: when SignModel is instantiated it will attempt to load from parameters, if non exists then it 
        /// will train on the training data. We only create sign models for which training data exists.
        /// </summary>
        public void buildClassifier()
        {
            string trainingPath = absolute ? dataPath + "Training/Absolute" : dataPath + "Training/Relative";

            string[] trainingSetLocs = Directory.GetDirectories(trainingPath); //Get the list of training sets
            signModels = new List<SignModel>();
            SignModel sm;
             
            //foreach training set (files are named by sign)
            foreach (string setLoc in trainingSetLocs)
            {
                //Create a new sign model with that name and add it
                string signName = setLoc.Split('/').Last().Split('\\').Last();
                
                if (!ignore.Contains(signName))
                {
                    sm = new SignModel(dataPath, signName,clusters);
                    signModels.Add(sm);
                }
            }
        }

        #region Testing procedures

        public double runTests(string folder, out int falsePositives, out int falseNegatives, out int fpRate, out int tpRate)
        {
            int totalAttempts = 0;
            int correct = 0;
            string[] setLocs = Directory.GetDirectories(folder);
            falsePositives = 0; falseNegatives = 0; fpRate = 0; tpRate = 0;
            foreach (string loc in setLocs)
            {
                int numOfTests = 0;
                string signName = loc.Split('.')[0].Split('/').Last();
                if (!ignore.Contains(signName))
                {
                    int t1, t2;
                    correct += testFile(loc, signName, out numOfTests, out t1, out t2);
                    falsePositives += t1;
                    falseNegatives += t2;
                }
                totalAttempts += numOfTests;
            }
            return (double)correct / (double)totalAttempts;
        }

        private int testFile(string file, string signName, out int numOfTests, out int falsePos, out int falseNeg)
        {
            numOfTests = 0;
            int correct = 0;
            falseNeg = 0; falsePos = 0;
            using (StreamReader sr = new StreamReader(file + "/HandRight_x.csv"))
            {
                while (sr.ReadLine() != null)
                    numOfTests++;
            }

            List<Dictionary<string, double[][]>> tests = new List<Dictionary<string, double[][]>>(numOfTests);

            for (int i = 0; i < numOfTests; i++)
            {
                Dictionary<string, double[][]> test = new Dictionary<string, double[][]>();
                foreach (JointType j in GestureRecording.trackedJoints)
                {
                    string jname = j.ToString();
                    double[][] jointObsSeq = buildObsSeq(file + "/" + jname + "_x.csv",
                        file + "/" + jname + "_y.csv", file + "/" + jname + "_z.csv", i);
                    test.Add(j.ToString(), jointObsSeq);
                }
                tests.Add(test);
            }
            foreach (Dictionary<string, double[][]> test in tests)
            {
                string sign = getSign(test);
                if (sign == signName)
                    correct++;
                if (sign == "none" && signName != "none")
                    falseNeg++;
                if (sign != "none" && signName == "none")
                    falsePos++;
            }
            return correct;
        }


        public void acceptanceThreshTest(double minTresh, double increment)
        {
            if(minTresh>0)
            {
                return;
            }
            using (StreamWriter sr = new StreamWriter("C:/Users/user/Desktop/signAlign/Data/Meta/acceptanceThreshTest.csv"))
            {
                for (double tempThresh = minTresh; tempThresh < 0; tempThresh += increment)
                {
                    acceptanceThreshold = tempThresh;
                    int falsePos = 0; int falseNeg = 0; int fprate = 0; int tprate = 0;
                    runTests("C:/Users/user/Desktop/signAlign/Data/Test/Absolute/", out falsePos, out falseNeg, out fprate, out tprate);
                    sr.WriteLine(tempThresh.ToString() + "," + falsePos.ToString() + "," + falseNeg.ToString());
                }

            }
        }

        public void clustersRocTest(string sign, string folder, int minclusts, int maxclusts, double minThresh, double increment)
        {
            for (int i = minclusts; i < maxclusts; i+=3 )
            {
                clusters = i;
                delParams();
                buildClassifier();
                makeRocCurve(sign, folder, minThresh, increment);
            }
        }

        /// <summary>
        /// Saves to file a one-vs-many RoC curve using "sign" as a the one
        /// </summary>
        /// <param name="sign">the sign to be used for tru positives</param>
        /// <param name="folder">the hold out data location</param>
        /// <param name="minThresh">the minimum threshold to start the RoC curve from</param>
        /// <param name="increment">the threshold increment</param>
        public void makeRocCurve(string sign, string folder, double minThresh, double increment)
        {
            if (minThresh > 0)
            {
                return;
            }
            using (StreamWriter sr = new StreamWriter("C:/Users/user/Desktop/signAlign/Data/Meta/"+sign+"_RoC"+clusters.ToString()+".csv"))
            {
                for (double tempThresh = minThresh; tempThresh < 0; tempThresh += increment)
                {
                    acceptanceThreshold = tempThresh;
                    double fprate = 0.0f; double tprate = 0.0f;
                    oneManyTests("C:/Users/user/Desktop/signAlign/Data/Test/Absolute/", sign, out fprate, out tprate);
                    sr.WriteLine(tempThresh.ToString() + "," + fprate.ToString() + "," + tprate.ToString());
                }
            }
        }
        /// <summary>
        /// Computes the true positive and false postive rates of "sign" vs all other signs
        /// Uses the current threshold value.
        /// </summary>
        /// <param name="folder">The test data folder</param>
        /// <param name="sign">The sign used for true positives</param>
        /// <param name="fpRate">out: will return the false positive rate</param>
        public void oneManyTests(string folder, string sign,out double fpRate, out double tpRate)
        {
            int falsePositives = 0; int truePositives = 0; int positives = 0; int negatives = 0;
            string[] setLocs = Directory.GetDirectories(folder);
            fpRate = 0; tpRate = 0;
            foreach (string loc in setLocs)
            {
                int numOfTests = 0;
                string signName = loc.Split('.')[0].Split('/').Last();
                if (!ignore.Contains(signName))
                {
                    //If we are testing the true sign, update our numbers accordingly
                    if (sign == signName)
                    {
                        int t1, t2;
                        truePositives += testFile(loc, signName, out numOfTests, out t1, out t2);
                        positives += numOfTests;
                    }
                    else //We should be classifying negatives (ie other signs)
                    {
                        int negs, falses;
                        oneManyFalsePos(loc, sign, signName, out negs, out falses);
                        falsePositives += falses;
                        negatives += negs;
                    }
                }
            }
            fpRate = (double)falsePositives / (double)negatives;
            tpRate = (double)truePositives / (double) positives;
        }

        private void oneManyFalsePos(string file, string trueSign ,string signName, out int negatives, out int falsePos)
        {
            negatives = 0;
            falsePos = 0;
            using (StreamReader sr = new StreamReader(file + "/HandRight_x.csv"))
            {
                while (sr.ReadLine() != null)
                    negatives++;
            }

            List<Dictionary<string, double[][]>> tests = new List<Dictionary<string, double[][]>>(negatives);

            for (int i = 0; i < negatives; i++)
            {
                Dictionary<string, double[][]> test = new Dictionary<string, double[][]>();
                foreach (JointType j in GestureRecording.trackedJoints)
                {
                    string jname = j.ToString();
                    double[][] jointObsSeq = buildObsSeq(file + "/" + jname + "_x.csv",
                        file + "/" + jname + "_y.csv", file + "/" + jname + "_z.csv", i);
                    test.Add(j.ToString(), jointObsSeq);
                }
                tests.Add(test);
            }
            foreach (Dictionary<string, double[][]> test in tests)
            {
                string sign = getSign(test);
                if (sign == trueSign)
                    falsePos++;
            }
            return; 
        }
        
        public void clustersTest()
        {
            using (StreamWriter sr = new StreamWriter("C:/Users/user/Desktop/signAlign/Data/Meta/clusterNumberTest.csv"))
            {
                for (int i = 3; i < 10; i++)
                {
                    clusters = i;
                    delParams();
                    buildClassifier();
                    int falsePos = 0; int falseNeg = 0; int fprate = 0; int tprate = 0; double correctPC;
                    correctPC = runTests("C:/Users/user/Desktop/signAlign/Data/Test/Absolute/", out falsePos, out falseNeg, out fprate, out tprate);
                    sr.WriteLine(clusters.ToString() + "," +correctPC.ToString()+","+ falsePos.ToString() + "," + falseNeg.ToString());
                }
            }
        }

        /// <summary>
        /// Deletes the parameterisations and retrains
        /// </summary>
        public void delParams()
        {
            string[] files = Directory.GetDirectories("C:/Users/user/Desktop/signAlign/Data/Parameters/");
            foreach (string file in files)
            {
                Directory.Delete(file, true);
            }
           
        }

        #endregion





    }
}
