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
            #region Without scales
            /*double[,] alphas = new double[T, N];
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

            return prob;*/
            
            #endregion

            double[,] alphabars = new double[T, N];
            double[,] alphahats = new double[T, N];
            double[] scales = new double[T];

            for (int i = 0; i < N; i++)
            {
                alphabars[0, i] = pi[i] * queryGuassian(observations[0], mus[i], sigmas[i]);
                scales[0] += alphabars[0, i];
            }
            scales[0] = 1 / scales[0];

            for (int i = 0; i < N; i++)
            {
                alphahats[0, i] = scales[0] * alphabars[0, i];
            }

            for (int t = 1; t < observations.Length; t++)
            {
                for (int i = 0; i < N; i++)
                {
                    alphabars[t, i] = 0;
                    for (int j = 0; j < N; j++)
                    {
                        alphabars[t, i] += alphahats[t - 1, i] * A.getTransitionProb(i, j);
                    }
                    alphabars[t, i] *= queryGuassian(observations[t], mus[i], sigmas[i]);
                    scales[t] += alphabars[t, i];
                }
                scales[t] = (1 / scales[t]);
                for (int i = 0; i < N; i++)
                {
                    alphahats[t,i] = scales[t] * alphabars[t, i];
                }
            }

            double logProb = 0;
            for (int t = 0; t < observations.Length; t++)
            {
                logProb += Math.Log(scales[t]);
            }
            logProb = -logProb;
            return logProb;

        }

        public double calculateMultiplePosterior(DenseVector[][] observationsSequence)
        {
            double prob = 0;

            for (int k = 0; k < observationsSequence.Length;k++ )
            {
                prob += calculatePosterior(observationsSequence[k]);
            }
            return prob;
        }


        public void reestimateParameters(DenseVector[][] observationsCollection)
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

            double[][,,] digammas = new double[K][,,];
            double[][,] gammas = new double[K][,];

            //Initialize arrays to proper sizes.
            for (int k = 0; k < K; k++)
            {
                alphahats[k]    = new double[times[k], N];
                alphabars[k]    = new double[times[k], N];
                scales[k]       = new double[times[k]];
                betahats[k]     = new double[times[k], N];
                digammas[k]     = new double[times[k],N,N];
                gammas[k]       = new double[times[k],N];
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
                    alphahats[k][0, i] = scales[k][0]*alphabars[k][0,i];
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
                        alphahats[k][t, i] = scales[k][t]*alphabars[k][t,i];
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

                #region The gamma pass
                for (int t = 0; t < times[k] - 1; t++)
                {
                    for (int i = 0; i < N; i++)
                    {
                        for (int j = 0; j < N; j++)
                        {
                            digammas[k][t, i, j] = alphahats[k][t, i] * A.getTransitionProb(i, j) *
                                queryGuassian(observationsCollection[k][t + 1], mus[j], sigmas[j]) * betahats[k][t + 1, j];
                        }
                        gammas[k][t, i] = alphahats[k][t, i] * betahats[k][t, i] * (1 / scales[k][t]);
                    }
                } 
                #endregion
            }

            #region re-estimate pi
            DenseVector newPi = new DenseVector(N);
            double numer, denom, piComp;
            for (int i = 0; i < N; i++)
            {
                numer = 0; denom = 0; piComp = 0;
                for (int k = 0; k < K; k++)
                {
                    numer += gammas[k][0, i];
                }
                for (int j = 0; j < numer; j++)
                {
                    for (int k = 0; k < K; k++)
                    {
                        denom += gammas[k][0, j];
                    }
                }
                piComp = numer / denom;
                newPi[i] = piComp;
            }
            
            #endregion

            #region re-estimate A
            DenseMatrix newA;
            double[,] newAarray = new double[N, N];
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    numer = 0; denom = 0;

                    //Can these be simplified with the gammas?
                    for (int k = 0; k < K; k++)
                    {
                        for (int t = 0; t < times[k] - 1; t++)
                        {
                            numer += alphahats[k][t, i] * A.getTransitionProb(i, j) *
                                queryGuassian(observationsCollection[k][t + 1], mus[j], sigmas[j]) * betahats[k][t + 1, j];
                        }
                    }

                    for (int k = 0; k < K; k++)
                    {
                        for (int t = 0; t < times[k] - 1; t++)
                        {
                            denom += alphahats[k][t, i] * betahats[k][t, i] * (1 / scales[k][t]);
                        }
                    }
                    newAarray[i, j] = (numer / denom);
                }

            }
            newA = new DenseMatrix(newAarray);
            
            #endregion

            #region re-estimate mus
            DenseVector[] newMus = new DenseVector[N];
            //Initialize and zero the new mus
            for (int j = 0; j < N; j++)
            {
                newMus[j] = new DenseVector(observationsCollection[0][0].Count);
                for (int l = 0; l < observationsCollection[0][0].Count; l++)
                {
                    newMus[j][l] = 0;
                }

            }
            for (int j = 0; j < N; j++)
            {
                denom = 0;
                for (int k = 0; k < K; k++)
                {
                    for (int t = 0; t < times[k]; t++)
                    {
                        newMus[j] += gammas[k][t, j] * observationsCollection[k][t];
                        denom += gammas[k][t, j];
                    }
                }
                newMus[j] = (newMus[j] / denom);
            }
            #endregion

            #region re-estimate Sigmas
            DenseMatrix[] newSigmas = new DenseMatrix[N];
            DenseMatrix temp = new DenseMatrix(observationsCollection[0][0].Count);
            DenseVector tempvec = new DenseVector(observationsCollection[0][0].Count);
            for (int j = 0; j < N; j++)
            {
                newSigmas[j] = new DenseMatrix(observationsCollection[0][0].Count);
                for (int l = 0; l < observationsCollection[0][0].Count; l++)
                {
                    for (int m = 0; m < observationsCollection[0][0].Count; m++)
                    {
                        newSigmas[j][l, m] = 0;
                    }
                }

            }

            for (int j = 0; j < N; j++)
            {
                denom = 0;
                for (int k = 0; k < K; k++)
                {
                    for (int t = 0; t < times[k]; t++)
                    {
                        tempvec = observationsCollection[k][t] - newMus[j];
                        temp = DenseVector.OuterProduct(tempvec, tempvec);
                        newSigmas[j] += temp;

                        denom += gammas[k][t, j];
                    }
                }
                newSigmas[j] = newSigmas[j] * (1 / denom);
            } 
            #endregion

            pi = newPi;
            A = new MarkovChain(newA);
            mus = newMus;
            sigmas = newSigmas;
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
