using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using Microsoft.Kinect;
namespace SignAlign
{

    /// <summary>
    /// Given an input stream, identifies the associated sign
    /// </summary>
    class SignModel
    {
        private string dataPath;
        public string name { get; private set; }
        private KMeansClassifier KMClassifier; //A K-means classifier to determine clusters in the training sets;
        public int clusters; //The number of clusters the K-M class will divide the input data into (determines the number of symbols)

        /* We store one DHMM and one weight for each of the joints as a hashmap taking joint names (taken from file)
         * to a pair of weighting and D_HMM. This approach ensures that we can associate memebers new joint observation collections
         * to their appropriate DHMM.
         * */

        private Dictionary<string, D_HMM> jointHMMs = new Dictionary<string, D_HMM>();
        private Dictionary<string, double> weights = new Dictionary<string, double>();

        /// <summary>
        /// Creates a signModel object by specifying the data path and name
        /// </summary>
        /// <param name="path">The location on disk of the signAlign data folder</param>
        /// <param name="name">The name of the sign this model corresponds to</param>
        /// <param name="train">Set true if the model should be trained from the training data<param>
        public SignModel(string path, string name, int clusters)
        {
            dataPath = path;
            this.name = name;
            this.clusters = clusters;
            KMClassifier = new KMeansClassifier(clusters);

            
            //If there is a parameters file corresponding to this sign model
            if (Directory.Exists(dataPath + "Parameters/" + name + "/"))
            {
                loadParameters(); //load them
            }
            //Otherwise, train the model from the training file
            else
            {
                trainModel(true);
            }
            setWeights();
        }
        private void setWeights()
        {
            foreach (JointType j in GestureRecording.trackedJoints)
            {
                string jname = j.ToString();
                if (jname == "HandRight" || jname == "HandLeft")
                {
                    weights.Add(jname, 1.0f);
                }
                else
                {
                    weights.Add(jname, 0.0f);
                }
            }
        }

        /// <summary>
        /// Uses the recordings files in the ~/Data/Training/ folder to parametrise a collection of HMMs.
        /// </summary>
        public void trainModel(bool absolute)
        {
            string datType = absolute ? "Absolute/" : "Relative/";
            string trainingPath = dataPath + "Training/"+datType+name+"/";

            //Create a directory in which to store the hmm parameters
            Directory.CreateDirectory(dataPath + "Parameters/" + name + "/");

            D_HMM hmm;

            //Gets the file names
            string[] fileNames = Directory.GetFiles(trainingPath);

            //For each set of three (x,y and z files)
            for (int i = 0; i < fileNames.Length; i += 3) //HOLY HELL THIS IS AWFUL FIX IT
            {
                hmm = trainNewHMM(fileNames[i], fileNames[i + 1], fileNames[i + 2]);
                hmm.saveParameters(dataPath + "Parameters/"+name+"/");
                jointHMMs.Add(hmm.name, hmm);
            }
        }
          

        /// <summary>
        /// Returns a newly initialized HMM. This method is where we encode our initial markov chain topology
        /// </summary>
        /// <param name="name">The name of the HMM (the sign corresponding to this HMM)</param>
        /// <param name="centroids">The centroids of the clusters of the training data, used to convert input streams to HMM  symbol streams</param>
        /// <returns>a new HMM which is initialized with a predetermined topology and ready to be trained</returns>
        private D_HMM initializeNewHMM(string name, double[][] centroids)
        {
            //Create A
            double[,] A = new double[12, 12];
            for (int i = 0; i < 12; i++)
            {
                A[i, i] = 0.5;
                if (i < 11)
                {
                    A[i, i + 1] = 0.5;
                }
                A[11, 11] = 1;
            }
            //Create B
            double[,] B = new double[12, centroids.Length+1];

            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < centroids.Length+1; j++)
                {
                    B[i, j] = (1.0 / (centroids.Length+1));
                }
            }
            //Pi
            double[] pi = new double[12];
            pi[0] = 1;

            D_HMM hmm = new D_HMM(pi, A, B, centroids, name);
            return hmm;
        }

        /// <summary>
        /// Initializes and trains a hmm from a given set of training data
        /// </summary>
        /// <param name="x_fileLoc">The x locations data file</param>
        /// <param name="y_fileLoc">The y locations data file</param>
        /// <param name="z_fileLoc">The z locations data file</param>
        /// <returns>A HMM trained on the (x,y,z) observation sequences specified by the file locations</returns>
        private D_HMM trainNewHMM(string x_fileLoc, string y_fileLoc, string z_fileLoc)
        {
            //Compute the number of different training sequences there are in the training data files
            int numberOfSeqs = 0;
            using (StreamReader srx = new StreamReader(x_fileLoc))
            {
                while (srx.ReadLine() != null)
                    numberOfSeqs++;
            }

            //Construct from the data files the (x,y,z) sequences
            List<double[]>[] obsSeqList = new List<double[]>[numberOfSeqs];
            for (int i = 0; i < numberOfSeqs; i++)
            {
                obsSeqList[i] = new List<double[]>();
            }
            //Also amalgamate them all in one list for use in clustering
            List<double[]> allObservations = new List<double[]>();
            int currentObs = 0;
            using (StreamReader srx = new StreamReader(x_fileLoc))
            {
                using (StreamReader sry = new StreamReader(y_fileLoc))
                {
                    using (StreamReader srz = new StreamReader(z_fileLoc))
                    {
                        string linex, liney, linez; string[] xrow, yrow, zrow;
                        while ((linex = srx.ReadLine()) != null)
                        {
                            liney = sry.ReadLine();
                            linez = srz.ReadLine();

                            xrow = linex.Split(',');
                            yrow = liney.Split(',');
                            zrow = linez.Split(',');
                            if (linex != "")
                            {
                                for (int i = 0; i < xrow.Length; i++)
                                {
                                    obsSeqList[currentObs].Add(new double[] {Convert.ToDouble(xrow[i]), 
                                Convert.ToDouble(yrow[i]), Convert.ToDouble(zrow[i])});
                                    allObservations.Add(new double[] {Convert.ToDouble(xrow[i]), 
                                Convert.ToDouble(yrow[i]), Convert.ToDouble(zrow[i])});
                                }
                                currentObs++;
                            }
                        }
                    }
                }
            }
            //Convert to an array for use in the training algorithm
            double[][][] obsSeqsArray = new double[numberOfSeqs][][];
            for (int i = 0; i < numberOfSeqs; i++)
            {
                obsSeqsArray[i] = obsSeqList[i].ToArray();
            }
            //Compute the clusters for the hmm
            double[][] data = allObservations.ToArray();
            double[][] centroids = new double[clusters][];
            KMClassifier.computeClusters(data, 0, out centroids);

            //Name the hmm from the training data names
            string hmmname = x_fileLoc.Split('.')[0];
            hmmname = (hmmname.Split('/').Last()).Split('_').First(); //Also the joint name
            
            D_HMM dhmm = initializeNewHMM(hmmname, centroids);
            dhmm.Reestimate(obsSeqsArray, 50, 0.03f);

            return dhmm;
        }

        /// <summary>
        /// Uses the path variable to parametrise a HMM for each file at that path (each file contains the parameters of a trained HMM)
        /// </summary>
        private void loadParameters()
        {
            string parametersFile = dataPath + "Parameters/" + name + "/";
            string[] fileNames = Directory.GetFiles(parametersFile);

            D_HMM dhmm;

            foreach (string fileName in fileNames)
            {
                string hmmname = fileName.Split('.')[0];
                hmmname = hmmname.Split('/').Last();
                
                dhmm = new D_HMM(hmmname, parametersFile);
                jointHMMs.Add(hmmname, dhmm);
            }
        }
        /// <summary>
        /// Given list of observation sequences, one for each HMM of the signModel, returns the probability that this list corresponds to the 
        /// training sign for this signModel
        /// </summary>
        /// <param name="obsSeq"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public double Evaluate(Dictionary<string, double[][]> jointObsSeqs, bool log)
        {
            /*
             * If the list joint observation collection does not specify the same
             * joints as the jointsHMMs then we return 0
             */

            //Else compute the weighted sum. Here we assume the joints observation list and joints HMM lists are in the same order.
            double logProbSum = 0;
                        
            foreach (string joint in jointObsSeqs.Keys)
            {
                D_HMM tempHmm;
                double[][] tempObsSeq;
                double weight;
                jointHMMs.TryGetValue(joint, out tempHmm);
                jointObsSeqs.TryGetValue(joint, out tempObsSeq);
                weights.TryGetValue(joint, out weight);
                logProbSum += weight*tempHmm.Evaluate(tempObsSeq, true);
            }
            if (log)
            {
                return logProbSum;
            }
            else
            {
                return Math.Exp(logProbSum);
            }

        }
        
    }
}
