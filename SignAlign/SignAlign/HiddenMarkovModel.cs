using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;

namespace WpfApplication1
{   
    /// <summary>
    /// A class to encapsulate a Hidden Markov Model H=(A,B,pi)
    /// 
    /// We will instantiate and train a HMM for EACH SIGN in our dictionary
    /// </summary>
    class HiddenMarkovModel
    {
        private int N; //The number of states in the Markov Chain
        private int M; //The number of possible emissions of a given state in the Markov chain
        private DenseMatrix A; //An NxN matrix, the transition probabilities for the MC
        private DenseMatrix B; //An MxN Observation probability matrix
        private DenseVector pi; //An N-vector, the initial distribution 

        //Instantiate a HMM without a known A or B
        public HiddenMarkovModel(int N, int M)
        {
            this.N = N;
            this.M = M;
            A = new DenseMatrix(N);
            B = new DenseMatrix(N, M);
        }
        //Instantiate a HMM with known A,B and pi
        public HiddenMarkovModel(double[,] A, double[,] B, double[] pi)
        {
            this.A = new DenseMatrix(A);
            this.B = new DenseMatrix(B);
            this.pi = pi;
            N = this.A.RowCount;
            M = this.B.ColumnCount;
            //CHECK THAT THE DIMENSIONS LINE UP
        }

        /// <summary>
        /// Returns the probability that a given sequence of observation was created by this Markov model
        ///
        /// Uses the forward/backward algorithm. No scaling is used here.
        /// </summary>
        public double probObservations(int[] observations)
        {
            int T = observations.Length; //We have T observations: O_0,...,O_T-1  from the set {0,1,...,M-1}
            double[,] alphas = new double[T,N]; // alphas[t][i] = alpha_t(i) = P[O_0 & O_1 & ... & O_t & x_t = q_i]

            for (int i = 0; i < observations.Length; i++)
            {
                if ((observations[i] > M - 1) || (observations[i] < 0))
                {
                    //The observation list is impossible
                    return 0;
                }
            }
            //Initialize the alpha_0(i)
            for (int i = 0; i < N; i++)
            {
                alphas[0, i] = pi.Values[i] * B.ToArray()[i, observations[0]];
            }
            //Recursively compute the remaining alphas
            for (int t = 1; t < T; t++)
            {
                for (int i = 0; i < N; i++)
                {
                    alphas[t, i] = 0;
                    for (int j = 0; j < N; j++)
                    {
                        alphas[t, i] += alphas[t - 1, j] * A.ToArray()[i, j];
                    }
                    alphas[t, i] *= B.ToArray()[i, observations[t]];
                }
            }

            double prob = 0;
            for (int i = 0; i < N; i++)
            {
                prob += alphas[T-1, i];
            }
            return prob;
        }

         /// <summary>
        /// Returns the probability that a given sequence of observation was created by this Markov model
        ///
        /// Uses the forward/backward algorithm, we scale the alphas and return a log-probability to avoid underflow.
        /// </summary>
        public double logProbObservations(int[] observations)
        {
            int T = observations.Length; //We have T observations: O_0,...,O_T-1  from the set {0,1,...,M-1}
            double[,] alphas = new double[T,N]; // alphas[t][i] = alpha_t(i) = P[O_0 & O_1 & ... & O_t & x_t = q_i]
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
            for (int i = 0; i < N; i++)
            {
                alphas[0, i] = pi.Values[i] * B.ToArray()[i, observations[0]];
                scales[0] += alphas[0, i];
            }
            //Scale the alpha_0(i)
            scales[0] = 1/scales[0];
            for (int i = 0; i < N; i++)
            {
                alphas[0, i] *= scales[0];
            }

            //Recursively compute the remaining alphas
            for (int t = 1; t < T; t++)
            {
                scales[t] = 0;
                for (int i = 0; i < N; i++)
                {
                    alphas[t, i] = 0;
                    for (int j = 0; j < N; j++)
                    {
                        alphas[t, i] += alphas[t - 1, j] * A.ToArray()[i, j];
                    }
                    alphas[t, i] *= B.ToArray()[i, observations[t]];
                    scales[t] += alphas[t, i];
                }
                //Scale the alpha_t(i)
                scales[t] = 1 / scales[t];
                for (int i = 0; i < N; i++)
                {
                    alphas[t, i] *= scales[t];
                }
            }

            double logprob = 0;
            for (int i = 0; i < N; i++)
            {
                logprob += Math.Log(scales[i]);
            }
            return -logprob;
        }
    }
    

}
