using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;

namespace SignAlign
{   
    /// <summary>
    /// A class to encapsulate a Hidden Markov Model H=(A,B,pi)
    /// 
    /// We will instantiate and train a HMM for EACH SIGN in our dictionary
    /// </summary>
    class DiscreteHiddenMarkovModel : HiddenMarkovModel
    {
        private DiscreteObservationProbabilityMeasure B; //An MxN Observation probability matrix
        private DenseVector pi; //An N-vector, the initial distribution 

        //Instantiate a HMM with known A,B and pi
        public DiscreteHiddenMarkovModel(double[,] stochMat, double[,] B, double[] pi)
        {
            A = new MarkovChain(new DenseMatrix(stochMat));
            this.B = new DiscreteObservationProbabilityMeasure(A, new DenseMatrix(B));
            this.pi = pi;
        }

        /// <summary>
        /// Returns the probability that a given sequence of observation was created by this Markov model
        ///
        /// Uses the forward/backward algorithm. No scaling is used here.
        /// </summary>
        /* public double probObservations(int[] observations)
         {
             int T = observations.Length; //We have T observations: O_0,...,O_T-1  from the set {0,1,...,M-1}
             double[,] alphas = new double[T,A.numberOfStates]; // alphas[t][i] = alpha_t(i) = P[O_0 & O_1 & ... & O_t & x_t = q_i]

             for (int i = 0; i < observations.Length; i++)
             {
                 if ((observations[i] > M - 1) || (observations[i] < 0))
                 {
                     //The observation list is impossible
                     return 0;
                 }
             }
             //Initialize the alpha_0(i)
             for (int i = 0; i < A.numberOfStates; i++)
             {
                 alphas[0, i] = pi.Values[i] * B.ToArray()[i, observations[0]];
             }
             //Recursively compute the remaining alphas
             for (int t = 1; t < T; t++)
             {
                 for (int i = 0; i < A.numberOfStates; i++)
                 {
                     alphas[t, i] = 0;
                     for (int j = 0; j < A.numberOfStates; j++)
                     {
                         alphas[t, i] += alphas[t - 1, j] * A.getTransitionProb(i, j);
                     }
                     alphas[t, i] *= B.ToArray()[i, observations[t]];
                 }
             }

             double prob = 0;
             for (int i = 0; i < A.numberOfStates; i++)
             {
                 prob += alphas[T-1, i];
             }
             return prob;
         }

         /*
         /// <summary>
         /// Returns the probability that a given sequence of observation was created by this Markov model
         ///
         /// Uses the forward/backward algorithm, we scale the alphas and return a log-probability to avoid underflow.
         /// </summary>
         public double logProbObservations(int[] observations)
         {
             int T = observations.Length; //We have T observations: O_0,...,O_T-1  from the set {0,1,...,M-1}
             double[,] alphas = new double[T,A.numberOfStates]; // alphas[t][i] = alpha_t(i) = P[O_0 & O_1 & ... & O_t & x_t = q_i]
             double[] scales = new double[T];
             for (int i = 0; i < observations.Length; i++)
             {
                 if ((observations[i] > M - 1) || (observations[i] < 0))
                 {
                     //The observation list is impossible
                     return 0;
                 }
             }
             scales[0] = 0;
             //Initialize the alpha_0(i)
             for (int i = 0; i < A.numberOfStates; i++)
             {
                 alphas[0, i] = pi.Values[i] * B.ToArray()[i, observations[0]];
                 scales[0] += alphas[0, i];
             }
             //Scale the alpha_0(i)
             scales[0] = 1/scales[0];
             for (int i = 0; i < A.numberOfStates; i++)
             {
                 alphas[0, i] *= scales[0];
             }

             //Recursively compute the remaining alphas
             for (int t = 1; t < T; t++)
             {
                 scales[t] = 0;
                 for (int i = 0; i < A.numberOfStates; i++)
                 {
                     alphas[t, i] = 0;
                     for (int j = 0; j < A.numberOfStates; j++)
                     {
                         alphas[t, i] += alphas[t - 1, j] * A.getTransitionProb(i, j);
                     }
                     alphas[t, i] *= B.ToArray()[i, observations[t]];
                     scales[t] += alphas[t, i];
                 }
                 //Scale the alpha_t(i)
                 scales[t] = 1 / scales[t];
                 for (int i = 0; i < A.numberOfStates; i++)
                 {
                     alphas[t, i] *= scales[t];
                 }
             }

             double logprob = 0;
             for (int i = 0; i < A.numberOfStates; i++)
             {
                 logprob += Math.Log(scales[i]);
             }
             return -logprob;
         }*/

        public override double probObservations(List<IObersvation> observations)
        {
            int T = observations.Count;
            double[,] alphas = computeAlphas(observations);

            double prob = 0;
            for (int i = 0; i < A.numberOfStates; i++)
            {
                prob += alphas[T - 1, i];
            }
            return prob;
        }

        public override void reestimateParameters(List<IObersvation> observations)
        {
            int T = observations.Count;

            double[,] newStocMat =  new double[A.numberOfStates, A.numberOfStates];
            double[,] newEmissMat = new double[A.numberOfStates, B.range];
            double[] newPi = new double[A.numberOfStates];

            double[,] alphas = computeAlphas(observations);
            double[,] betas  = computeBetas(observations);
            double[,,] digammas = computeDiGammas(observations, alphas,betas);
            double[,] gammas = computeGammas(observations, digammas);

            for(int i = 0; i<A.numberOfStates;i++)
            {
                newPi[i] = gammas[0,i];
            }
            for(int i = 0; i<A.numberOfStates;i++)
            {
                 for(int j = 0; j<A.numberOfStates;j++)
                 {
                     double sumDiGams = 0;
                     double sumGams = 0;
                     for(int t=0; t<T-1; t++)
                     {
                         sumDiGams+= digammas[t,i,j];
                     }
                     for(int t=0; t<T-1; t++)
                     {
                         sumGams+= gammas[t,i];
                     }
                     newStocMat[i,j] = sumDiGams / sumGams;
                 }
            }


            for(int j=0; j<A.numberOfStates; j++)
            {
                for(int k = 0; k<B.range;k++)
                {
                    double sumGamObs = 0;
                    double sumGams = 0;
                    for (int t = 0; t<T-1; t++)
                    {
                        DiscreteObservation dob = (DiscreteObservation) observations[t];
                        if(dob.obsVal == k)
                        {
                            sumGamObs+= gammas[t,j];
                        }
                        sumGams += gammas[t,j];
                    }

                    newEmissMat[j,k] = sumGamObs / sumGams;
                }
            }

            A = new MarkovChain(new DenseMatrix(newStocMat));
            B = new DiscreteObservationProbabilityMeasure(A, new DenseMatrix(newEmissMat));
           // pi = new DenseVector(newPi); 
            
        }

        public void scaledReestimateParams(List<IObersvation> observations, out double[] scales)
        {
            int T = observations.Count;

            scales = new double[T];

            double[,] newStocMat = new double[A.numberOfStates, A.numberOfStates];
            double[,] newEmissMat = new double[A.numberOfStates, B.range];
            double[] newPi = new double[A.numberOfStates];

            double[,] alphas = computeScaledAlphas(observations, out scales);
            double[,] betas = computeScaledBetas(observations, scales);
            double[, ,] digammas = new double[T, A.numberOfStates, A.numberOfStates];
            double[,] gammas = new double[T, A.numberOfStates];

            double denom;

            for (int t = 0; t < T - 1; t++)
            {
                denom = 0;
                for (int i = 0; i < A.numberOfStates; i++)
                {
                    for (int j = 0; j < A.numberOfStates; j++)
                    {
                        denom += alphas[t, i] * A.getTransitionProb(i, j) * B.emissionProb(j, observations[t + 1]) * betas[t + 1, j];
                    }
                }
                for (int i = 0; i < A.numberOfStates; i++)
                {
                    gammas[t, i] = 0;
                    for (int j = 0; j < A.numberOfStates; j++)
                    {
                        digammas[t, i, j] = (alphas[t, i] * A.getTransitionProb(i, j) * B.emissionProb(j, observations[t + 1]) * betas[t + 1, j]) / denom;
                        gammas[t, i] += digammas[t, i, j];
                    }
                }
            }


            for(int i = 0; i<A.numberOfStates;i++)
            {
                newPi[i] = gammas[0,i];
            }
            for(int i = 0; i<A.numberOfStates;i++)
            {
                 for(int j = 0; j<A.numberOfStates;j++)
                 {
                     double sumDiGams = 0;
                     double sumGams = 0;
                     for(int t=0; t<T-1; t++)
                     {
                         sumDiGams+= digammas[t,i,j];
                         sumGams+= gammas[t,i];
                     }
                     newStocMat[i,j] = sumDiGams / sumGams;
                 }
            }

            for (int j = 0; j < A.numberOfStates; j++)
            {
                for (int k = 0; k < B.range; k++)
                {
                    double sumGamObs = 0;
                    double sumGams = 0;
                    for (int t = 0; t < T - 1; t++)
                    {
                        DiscreteObservation dob = (DiscreteObservation)observations[t];
                        if (dob.obsVal == k)
                        {
                            sumGamObs += gammas[t, j];
                        }
                        sumGams += gammas[t, j];
                    }

                    newEmissMat[j, k] = sumGamObs / sumGams;
                }
            }
            A = new MarkovChain(new DenseMatrix(newStocMat));
            B = new DiscreteObservationProbabilityMeasure(A, new DenseMatrix(newEmissMat));
            pi = new DenseVector(newPi); 
        }

        //Trains the model using scaled/log probs. - BROKEN - B DOES NOT RE-EVALUATE PROPERLY
        public override void trainModel(List<IObersvation> observations)
        {
            double[] scales;
            double oldLogProb = -100000;
            double logProb = 0;
            int maxIters = 2;
            int iters = 1;

            scaledReestimateParams(observations, out scales);
            logProb = computeLogProb(scales);
          
            while (iters < maxIters && logProb > oldLogProb)
            {
                oldLogProb = logProb;
                scaledReestimateParams(observations, out scales);
                logProb = computeLogProb(scales);
                iters++;
            }

        }

        private double computeLogProb(double[] scales)
        {
            double logProb = 0;

            foreach(double c in scales)
            {
                logProb += c;
            }

            return logProb;
        }

        private double[,] computeAlphas(List<IObersvation> observations)
        {
            int T = observations.Count;
            double[,] alphas = new double[T, A.numberOfStates];
            //Initialize the alphas
            for (int i = 0; i < A.numberOfStates; i++)
            {
                alphas[0, i] = pi.Values[i] * B.emissionProb(i, observations[0]);
            }

            //Inductively compute the remaining alphas
            for (int t = 1; t < T; t++)
            {
                for (int i = 0; i < A.numberOfStates; i++)
                {
                    alphas[t, i] = 0;
                    for (int j = 0; j < A.numberOfStates; j++)
                    {
                        alphas[t, i] += alphas[t - 1, j] * A.getTransitionProb(j, i);
                    }
                    alphas[t, i] *= B.emissionProb(i, observations[t]);
                }

            }
            return alphas;
        }

        private double[,] computeScaledAlphas(List<IObersvation> observations, out double[] scaleVals)
        {
            int T = observations.Count;
            double[,] scaledAlphas = new double[T,A.numberOfStates];
            scaleVals = new double[T];

            scaleVals[0] = 0;
            //Compute the alpha_0(i)s
            for (int i = 0; i < A.numberOfStates; i++)
            {
                scaledAlphas[0, i] = pi.Values[i] * B.emissionProb(i, observations[0]);
                scaleVals[0] += scaledAlphas[0, i];
            }
            //Scale the alpha_0(i)s
            scaleVals[0] = 1 / scaleVals[0];
            for (int i = 0; i<A.numberOfStates;i++)
            {
                scaledAlphas[0, i] *= scaleVals[0];
            }

            for (int t = 1; t < T; t++)
            {
                scaleVals[t] = 0;
                for (int i = 0; i < A.numberOfStates; i++)
                {
                    scaledAlphas[t, i] = 0;
                    for (int j = 0; j < A.numberOfStates; j++)
                    {
                        scaledAlphas[t, i] += scaledAlphas[t - 1, j] * A.getTransitionProb(j, i);
                    }
                    scaledAlphas[t, i] *= B.emissionProb(i, observations[t]);
                    scaleVals[t] += scaledAlphas[t, i];
                }

                scaleVals[t] = 1 / scaleVals[t];
                for (int i = 0; i < A.numberOfStates; i++)
                {
                    scaledAlphas[t, i] *= scaleVals[t];
                }
            }

            return scaledAlphas;
        }

        private double[,] computeBetas(List<IObersvation> observations)
        {
            int T = observations.Count;
            double[,] betas = new double[T, A.numberOfStates];

            for (int i = 0; i < A.numberOfStates; i++)
            {
                betas[T - 1, i] = 1;
            }

            for (int t = T - 2; t >= 0; t--)
            {
                for (int i = 0; i < A.numberOfStates; i++)
                {
                    betas[t, i] = 0;
                    for (int j = 0; j < A.numberOfStates; j++)
                    {
                        betas[t, i] += A.getTransitionProb(i, j) * B.emissionProb(j, observations[t + 1]) * betas[t + 1, j];
                    }
                }
            }
            return betas;
        }

        private double[,] computeScaledBetas(List<IObersvation> observations, double[] scaleVals)
        {
            int T = observations.Count;
            double[,] scaledBetas = new double[T, A.numberOfStates];

            for (int i = 0; i < A.numberOfStates; i++)
            {
                scaledBetas[T - 1, i] = scaleVals[T - 1];
            }
            for (int t = T - 2; t >= 0; t--)
            {
                for (int i = 0; i < A.numberOfStates; i++)
                {
                    scaledBetas[t, i] = 0;
                    for (int j = 0; j < A.numberOfStates; j++)
                    {
                        scaledBetas[t,i] += A.getTransitionProb(i,j)*B.emissionProb(j,observations[t+1])*scaledBetas[t+1,j];
                    }
                    scaledBetas[t, i] *= scaleVals[t];
                }
            }
            return scaledBetas;
        }

        private double[, ,] computeDiGammas(List<IObersvation> observations, double[,] alphas, double[,] betas)
        {
            int T = observations.Count;
            double[, ,] digammas = new double[T, A.numberOfStates, A.numberOfStates];

            double probObs = probObservations(observations);

            for (int t = 0; t < T - 1; t++)
            {
                for (int i = 0; i < A.numberOfStates; i++)
                {
                    for (int j = 0; j < A.numberOfStates; j++)
                    {
                        digammas[t, i, j] = alphas[t, i] * A.getTransitionProb(i, j) * B.emissionProb(j, observations[t + 1]) * betas[t + 1, j];
                        digammas[t, i, j] = digammas[t, i, j] / probObs;
                    }
                }
            }
            return digammas;
        }

        private double[,] computeGammas(List<IObersvation> observations, double[, ,] digammas)
        {
            int T = observations.Count;
            double[,] gammas = new double[T,A.numberOfStates];
            for (int t = 0; t < T; t++)
            {
                for (int i = 0; i < A.numberOfStates; i++)
                {
                    gammas[t, i] = 0;
                    for (int j = 0; j < A.numberOfStates; j++)
                    {
                        gammas[t, i] += digammas[t, i, j];
                    }
                }
            }
            return gammas;
        }
    }
    

}
