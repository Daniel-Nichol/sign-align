using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;
namespace SignAlign
{
    class CD_HMM
    {

        MarkovChain A; //underlying MC
        DenseVector pi; //Start probs
        DenseVector[] mus;  //means of a simple mixture containing a single 3D gaussian distribution for each state
        DenseMatrix[] sigmas; //covariance of the 3D gaussians.

        public CD_HMM(MarkovChain A, DenseVector pi, DenseVector[] mus, DenseMatrix[] sigmas)
        {
            this.A = A;
            this.pi = pi;
            this.mus = mus;
            this.sigmas = sigmas;
        }

        //Given an observation, mu and sigma. What is N(O | mu, sigma)?
        public double queryGuassian(DenseVector observation, DenseVector mean, DenseMatrix covar)
        {
            double scaletemp, scale, exponent, prob;
            DenseMatrix v1, v2; //Temp matrices, for multiplying

            scaletemp = (Math.Pow(2 * Math.PI, mean.Count / 2));
            scale = (Math.Sqrt(covar.Determinant()));
            scale *= scaletemp;
            scale = 1 / scale;

            v1 = (DenseMatrix)(observation - mean).ToRowMatrix();
            v2 = (DenseMatrix)(observation - mean).ToColumnMatrix();
            v2 = (DenseMatrix)(covar.Inverse()) * v2;

            exponent = (-0.5) *  ((v1 * v2).ToArray()[0,0]);
            prob = scale * Math.Pow(Math.E, exponent);

            return prob;
        }

        //Given a sequence of observations, compute P(O|lambda)
        public double calculatePosterior(DenseVector[] observations)
        {
            int T = observations.Length;
            int N = A.numberOfStates;
            double[,] alphas = new double[T, N];
            double prob = 0;

            for (int i = 0; i < N; i++)
            {
                alphas[0, i] = pi.ToArray()[i] * queryGuassian(observations[0], mus[i], sigmas[i]);
            }

            for (int t = 1; t < T; t++)
            {
                for (int i = 0; i < N; i++)
                {
                    alphas[t, i] = 0;
                    for (int j = 0; j < N; j++)
                    {
                        alphas[t, i] += alphas[t - 1, j] * A.getTransitionProb(j, i);
                    }
                    alphas[t, i] *= queryGuassian(observations[t], mus[i], sigmas[i]);
                }
            }

            for (int i = 0; i < N; i++)
            {
                prob += alphas[T - 1, i];
            }

            return prob;
        }

        public double calculateMultiplePosterior(DenseVector[][] observationsSequence)
        {
            double prob = 1;

            foreach (DenseVector[] observations in observationsSequence)
            {
                prob *= calculatePosterior(observations);
            }
            return prob;
        }


        private void reestimateParameters(DenseVector[][] observationsCollection)
        {
            int K = observationsCollection.Length;
            int[] times = new int[K];
            int N = A.numberOfStates;

            for (int k = 0; k < K; k++)
            {
                times[k] = observationsCollection[k].Length; //possible error?
            }

            double[][,] alphabars = new double[K][,];
            double[][,] alphahats = new double[K][,];
            double[][] scales = new double[K][];

            double[][,] betahats = new double[K][,];

            for (int k = 0; k < K; k++)
            {
                alphahats[k] = new double[times[k], N];
                alphabars[k] = new double[times[k], N];
                scales[k] = new double[times[k]];

                betahats[k] = new double[times[k], N];
            }

            
            for (int k = 0; k < K; k++)
            {
                //The alpha pass
                scales[k][0] = 0;
                #region The alpha Pass
                for (int i = 0; i < N; i++)
                {
                    alphabars[k][0, i] = pi[i] * queryGuassian(observationsCollection[k][0], mus[i], sigmas[i]);
                    scales[k][0] += alphabars[k][0, i];
                }
                scales[k][0] = 1 / (scales[k][0]);

                for (int i = 0; i < N; i++)
                {
                    alphahats[k][0, i] = scales[k][0];
                }

                for (int t = 1; t < times[k]; t++)
                {
                    scales[k][t] = 0;
                    for (int i = 0; i < N; i++)
                    {
                        alphabars[k][t, i] = 0;
                        for (int j = 0; j < N; j++)
                        {
                            alphabars[k][t, i] += alphahats[k][t - 1, j] * A.getTransitionProb(j, i);
                        }
                        alphabars[k][t, i] *= queryGuassian(observationsCollection[k][t], mus[i], sigmas[i]);
                        scales[k][t] += alphabars[k][t, i];
                    }
                    scales[k][t] = 1 / (scales[k][t]);
                    for (int i = 0; i < N; i++)
                    {
                        alphahats[k][t, 0] *= scales[k][t];
                    }
                }
                
                #endregion

                #region The beta pass
                for (int i = 1; i < N; i++)
                {
                    betahats[k][times[k] - 1, i] = scales[k][times[k] - 1];
                }
                for (int t = times[k] - 2; t >= 0; t--)
                {
                    for (int i = 0; i < N; i++)
                    {
                        betahats[k][t, i] = 0;
                        for (int j = 0; j < N; j++)
                        {
                            betahats[k][t, i] += A.getTransitionProb(i, j) * queryGuassian(observationsCollection[k][t + 1], mus[j], sigmas[j]) * betahats[k][t + 1, j];
                        }
                        betahats[k][t, i] *= scales[k][t];
                    }
                } 
                #endregion


            }


            double[,] betas;
            double[, ,] digammas;
            double[,] gammas;





            
        }

        private void reestimateMarkovChain(double[, ,] digammas, double[] gammas)
        {
        }
        
        private void reestimatePi(double[,] gammas)
        {
        }

        private void reestimeMus(double[, ,] digammas)
        {
        }

        private void reestimateSigmas(double[, ,] digammas)
        {
        }
    }

}
