using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Kinect;

namespace SignAlign
{
    public class SignClassifier
    {
        private List<SignModel> signModels;
        private int clusters = 4;
        private string dataPath; //The location on disk of the data files.
        private double acceptanceThreshold;
        private bool absolute;

        private readonly string[] nouns = 
        {
            "Bus","Cat","Circle","Coffee","Computer",
            "Day", "Tea"
        };

        private readonly string[] questions = 
        {
            "Where", "Hello","House","Name","Snow","Swan"
        };
        private readonly string[] adjectives =
        {
            "Blue", "Dark", "Green","Orange","Red"
        };
        private readonly string[] pronouns =
        {
            "My","Your",
        };
        
        private string[] ignoreList = 
        {
            /*"Blue", "Bus","Cat","Circle","Coffee","Computer",
            "Dark","Day", "Green","Hello","House","My","Name","Orange","Red",
            "Snow","Swan","Tea","Where","Your"*/
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
        /// Given a gesture recording object gets the most likely sign
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public string getSign(GestureRecording gr)
        {
            Dictionary<JointType, List<double[]>> jointObsHM = gr.getJointReadings(false); //Fix.

            //Translate for use in the getSign method
            Dictionary<string, double[][]> jointObsSeq = new Dictionary<string, double[][]>();
            foreach (JointType j in GestureRecording.trackedJoints)
            {
                string jname = j.ToString();
                List<double[]> seqList; jointObsHM.TryGetValue(j, out seqList);
                double[][] seqArr = seqList.ToArray();
                jointObsSeq.Add(jname, seqArr);
            }

            return getSign(jointObsSeq);
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
                if (!ignoreList.Contains(sm.name)) //Only use the sign models for signs we aren't ignoring
                {
                    tempLogProb = sm.Evaluate(jointObsSeq, true);
                    if (tempLogProb > bestLogProb)
                    {
                        bestLogProb = tempLogProb;
                        bestSignMatch = sm.name;
                    }
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

        public string getSignFromRestrictedGrammar(Dictionary<string, double[][]> jointObsSeq, string[] wordClass)
        {
            string bestSignMatch = "";
            double bestLogProb = double.MinValue;
            double tempLogProb;
            foreach (SignModel sm in signModels)
            {
                if (!ignoreList.Contains(sm.name) && wordClass.Contains(sm.name)) //Only use the sign models for signs we aren't ignoring
                {
                    tempLogProb = sm.Evaluate(jointObsSeq, true);
                    if (tempLogProb > bestLogProb)
                    {
                        bestLogProb = tempLogProb;
                        bestSignMatch = sm.name;
                    }
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

                sm = new SignModel(dataPath, signName, clusters);
                signModels.Add(sm);
            }
        }

        #region Testing procedures

        public double runTests(string folder, out int totalAttempts, out int falsePositives, out int falseNegatives, out int misclasses)
        {
            totalAttempts = 0;
            int correct = 0;
            string[] setLocs = Directory.GetDirectories(folder);
            falsePositives = 0; falseNegatives = 0; misclasses = 0;
            foreach (string loc in setLocs)
            {
                int numOfTests = 0;
                string signName = loc.Split('.')[0].Split('/').Last();
                int fp, fn, mc;

                correct += testFile(loc, signName, out numOfTests, out fp, out fn, out mc);

                falsePositives += fp;
                falseNegatives += fn;
                misclasses += mc;
                totalAttempts += numOfTests;
            }
            return (double)correct / (double)totalAttempts;
        }

        private int testFile(string file, string signName, out int numOfTests, out int falsePos, out int falseNeg, out int misclasses)
        {
            numOfTests = 0;
            int correct = 0;
            falseNeg = 0; falsePos = 0; misclasses = 0;
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
                //If this is training data for a sign we have chosen to ignore, then the sign name should be "none"
                if (ignoreList.Contains(signName))
                {
                    signName = "none";
                }
                string sign = getSign(test);
                if (sign == signName)
                    correct++; //That is a true positive or a true negative
                else //We have made a mistake
                {
                    if (sign == "none" && signName != "none")
                        falseNeg++; //If we've missed a real sign then we have a false negative
                    else if (sign != "none" && signName == "none")
                        falsePos++; //We've detected a sign from nonsense input
                    else
                        misclasses++; //We've taken input for one sign and classified it as another sign
                }
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
                    int falsePos = 0; int falseNeg = 0; int mc = 0; int ta = 0;
                    runTests("C:/Users/user/Desktop/signAlign/Data/Test/Absolute/",out ta, out falsePos, out falseNeg, out mc);
                    sr.WriteLine(tempThresh.ToString() + "," + falsePos.ToString() + "," + falseNeg.ToString() + "," + mc.ToString());
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
                int fp, fn, mc, corr;
                corr = testFile(loc, signName, out numOfTests, out fp, out fn, out mc);
                if (sign == signName)
                {
                    truePositives += corr;
                    positives += numOfTests;
                }
                else
                {
                    falsePositives += fp;
                    negatives += numOfTests;
                }
                positives += numOfTests;
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
        
        /// <summary>
        /// For each parameterisation with cluster numbers between the min and max
        /// plots a threshold-accuracy curve.
        /// </summary>
        /// <param name="minClusters"></param>
        /// <param name="maxClusters"></param
        public void clustersTest(int minClusters, int maxClusters)
        {
            using (StreamWriter sr = new StreamWriter("C:/Users/user/Desktop/signAlign/Data/Meta/clusterNumberTest.csv"))
            {
                for (int i = minClusters; i < maxClusters; i++)
                {
                    clusters = i;
                    delParams();
                    buildClassifier();
                    int falsePos = 0; int falseNeg = 0; int mc = 0; int ta = 0; double accuracy;
                    sr.WriteLine(clusters.ToString());
                    string threshString, accString;
                    threshString = "Threshold:,"; accString = "Accuracy:,"; 
                    for (int thresh = -2000; thresh < 0; thresh += 50)
                    {
                        acceptanceThreshold = thresh;
                        accuracy = runTests("C:/Users/user/Desktop/signAlign/Data/Test/Absolute/",out ta, out falsePos, out falseNeg, out mc);
                        threshString += thresh.ToString() + ",";
                        accString += accuracy.ToString() + ",";
                    }
                    sr.WriteLine(threshString);
                    sr.WriteLine(accString);
                }
            }
        }
                
        private int testFileRestricted(string file, string signName, out int numOfTests, out int falsePos, out int falseNeg, out int misclasses, string[] wordclass)
        {
            numOfTests = 0;
            int correct = 0;
            falseNeg = 0; falsePos = 0; misclasses = 0;
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
                //If this is training data for a sign we have chosen to ignore, then the sign name should be "none"
                if (ignoreList.Contains(signName))
                {
                    signName = "none";
                }
                string sign = getSignFromRestrictedGrammar(test, wordclass);
                if (sign == signName)
                    correct++; //That is a true positive or a true negative
                else //We have made a mistake
                {
                    if (sign == "none" && signName != "none")
                        falseNeg++; //If we've missed a real sign then we have a false negative
                    else if (sign != "none" && signName == "none")
                        falsePos++; //We've detected a sign from nonsense input
                    else
                        misclasses++; //We've taken input for one sign and classified it as another sign
                }
            }
            return correct;
        }

        //Returns the misclassification rate 
        public double restrictedGrammarTest(string folder, out int missclasses, out int numOfTests, out double accuracy)
        {
            missclasses = 0; numOfTests = 0; int correct = 0;
            string[] setLocs = Directory.GetDirectories(folder);
            foreach(string loc in setLocs)
            {
                string signName = loc.Split('.')[0].Split('/').Last();
                string[] wordclass = getWordClass(signName);
                //Determine the word class of this sign
                int mc = 0; int ta = 0; int falsePos, falseNeg; 
                correct+= testFileRestricted(loc, signName, out ta, out falsePos,
                    out falseNeg, out mc, wordclass);
                missclasses += mc;
                numOfTests += ta;
            }
            accuracy = (double)correct / (double)numOfTests;
            return (double)missclasses / (double)numOfTests;
        }


        /// <summary>
        /// Given a sign name (ie word) returns the class of word
        /// </summary>
        /// <param name="signName">The name of the sign</param>
        /// <returns>The world class of this sign</returns>
        private string[] getWordClass(string signName)
        {
            if (nouns.Contains(signName))
            {
                return nouns;
            }
            else if (questions.Contains(signName))
            {
                return questions;
            }
            else if (adjectives.Contains(signName))
            {
                return adjectives;
            }
            else
            {
                return pronouns;
            }
        }

        public void misclassTest()
        {
            List<string> allSignsList = new List<string>();
            string[] strlist = {
                "Blue", "Bus","Cat","Circle","Coffee","Computer",
                "Dark","Day", "Green","Hello","House","My","Name","Orange","Red",
                "Snow","Swan","Tea","Where","Your"
            };
            allSignsList = strlist.ToList<string>();
            using (StreamWriter sr = new StreamWriter("C:/Users/user/Desktop/signAlign/Data/Meta/misclassTest.csv"))
            {
                int numOfSigns = allSignsList.Count;
                foreach(string sign in allSignsList)
                {
                    int falsePos = 0; int falseNeg = 0; int mc = 0; int ta = 0; double accuracy;
                    accuracy = runTests("C:/Users/user/Desktop/signAlign/Data/Test/Absolute/", out ta, out falsePos, out falseNeg, out mc);
                    sr.WriteLine(numOfSigns.ToString() +","+ mc.ToString());
                    ignore.Add(sign);
                    ignoreList = ignore.ToArray();
                    numOfSigns--;
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
