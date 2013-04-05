using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
namespace SignAlign
{

    /// <summary>
    /// Given an input stream, identifies the associated sign
    /// </summary>
    class SignClassifier
    {
        string dataPath;
        KMeansClassifier KMClassifier; //A K-means classifier to determined clusters in the training sets;
        public const int CLUSTERS = 8; //The number of clusters the K-M class will divide the input data into (determines the number of symbols)

        //We store one D-HMM for each sign. The parameters are stored on file to prevent re-estimation
        LinkedList<D_HMM> HMMs = new LinkedList<D_HMM>();


        public SignClassifier(string path)
        {
            dataPath = path;
            KMeansClassifier classifier = new KMeansClassifier(CLUSTERS);

            //loadParameters();
        }

        /// <summary>
        /// Uses the recordings files in the ~/Data/Training/ folder to parametrise a collection of HMMs.
        /// </summary>
        public void trainClassifier()
        {
            string trainingPath = dataPath + "Training/";

            D_HMM hmm;

            //Gets the file names
            string[] fileNames = Directory.GetFiles(trainingPath);

            //For each set of three (x,y and z files)
            for (int i = 0; i < fileNames.Length; i += 3) //HOLY HELL THIS IS AWFUL FIX IT
            {
                hmm = trainNewHMM(fileNames[i], fileNames[i + 1], fileNames[i + 2]);
                hmm.saveParameters(dataPath + "Parameters/");
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
            double[,] B = new double[12, 9];

            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    B[i, j] = (1.0 / 9.0);
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
            int numberOfSeqs;
            using (StreamReader srx = new StreamReader(x_fileLoc))
            {
                numberOfSeqs = srx.ReadLine().Split(',').Length;
            }

            //Construct from the data files the (x,y,z) sequences
            List<double[]>[] obsSeqList = new List<double[]>[numberOfSeqs];
            //Also amalgamate them all in one list for use in clustering
            List<double[]> allObservations = new List<double[]>();
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

                            for (int j = 0; j < xrow.Length; j++)
                            {
                                obsSeqList[j].Add(new double[3] { Convert.ToDouble(xrow[j]), Convert.ToDouble(yrow[j]), Convert.ToDouble(zrow[j]) });
                                allObservations.Add(new double[3] { Convert.ToDouble(xrow[j]), Convert.ToDouble(yrow[j]), Convert.ToDouble(zrow[j]) });
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
            double[][] centroids;
            KMClassifier.computeClusters(data, 0, out centroids);

            //Name the hmm from the training data names



            //D_HMM dhmm = initializeNewHMM(name, centroids);

            return null;
        }

        /// <summary>
        /// Uses the path variable to parametrise a HMM for each file at that path (each file contains the parameters of a trained HMM)
        /// </summary>
        private void loadParameters()
        {
            string parametersFile = dataPath + "Parameters/";
            string[] fileNames = Directory.GetFiles(parametersFile);

            D_HMM dhmm;

            foreach (string fileName in fileNames)
            {
                string name = fileName.Split('.')[0];
                name = name.Split('/').Last();
                dhmm = new D_HMM(name, parametersFile);
                dhmm.loadParameters(parametersFile);
                HMMs.AddLast(dhmm);
            }
        }

        /// <summary>
        /// Returns the most likely sign from a sequence of observations
        /// </summary>
        /// <param name="observations">A sequence of observations</param>
        /// <returns>The name of the (most probable) associated sign, or if no sign is sufficiently probable then null</returns>
        public string findMostProbableSign(double[][] observations)
        {
            string signName = null;
            double bestLogProb = double.MinValue;
            foreach (D_HMM hmm in HMMs)
            {
                double tempLogProb = hmm.Evaluate(observations, true);
                if ( tempLogProb  > bestLogProb)
                {
                    bestLogProb = tempLogProb;
                    signName = hmm.name;
                }
            }

            if (bestLogProb > -100)
            {
                return signName;
            }
            return null;
        }

    }
}
