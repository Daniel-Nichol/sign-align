using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SignAlign
{
    class SignClassifier
    {
        private List<SignModel> signModels;
        private string dataPath; //The location on disk of the data files.
        private double acceptanceThreshold;

        public SignClassifier(string dataPath, double acceptanceThreshold)
        {
            this.dataPath = dataPath;
            this.acceptanceThreshold = acceptanceThreshold;
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
                return null;
            }
        }

        /// <summary>
        /// Instantiates a new signModel for each folder in ~/dataPath/Training/, which should contain a folder of training data for each sign.
        /// 
        /// Note: when SignModel is instantiated it will attempt to load from parameters, if non exists then it 
        /// will train on the training data. We only create sign models for which training data exists.
        /// </summary>
        public void buildClassifier()
        {
            string trainingPath = dataPath + "Training/";
            string[] trainingSetLocs = Directory.GetDirectories(trainingPath); //Get the list of training sets
            signModels = new List<SignModel>();
            SignModel sm;
             
            //foreach training set (files are named by sign)
            foreach (string setLoc in trainingSetLocs)
            {
                //Create a new sign model with that name and add it
                string signName = setLoc.Split('/').Last();
                sm = new SignModel(dataPath, signName);
                signModels.Add(sm);
            }
            


        }



    }
}
