using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SignAlign
{
    //Another attempt at a discrete HMM.
    class D_HMM
    {
        private double[]  pi;
        private double[,] A;
        private double[,] B;
        private int N, M;
        public string name{get; set;}

        private double[][] centroids; //The centroids of the clusters, used to determine the symbols from 3D readings.

        public D_HMM(double[] pi, double[,] A, double[,] B, double[][] centroids, string name)
        {
            this.pi = pi;
            this.A = A;
            this.B = B;
            N = A.GetLength(0);
            M = B.GetLength(1);
            this.centroids = centroids;
            this.name = name;
        }

        public D_HMM(string name, string parametersFile)
        {
            this.name = name;
            loadParameters(parametersFile);
        }

        public double Evaluate(double[][] observations, bool log)
        {
            int[] symbolSeq = new int[observations.Length];
            for (int i = 0; i < symbolSeq.Length; i++)
            {
                symbolSeq[i] = convertToSymbol(observations[i]);
            }

            double prob = Evaluate(symbolSeq, log);
            return prob;

        }

        private double Evaluate(int[] observationSymbols, bool log)
        {
            double prob = 0;
            double[] scales;

            // Compute the alphas and take the scales
            computeAlphas(observationSymbols, out scales);

            for (int i = 0; i < scales.Length; i++)
            {
                prob += Math.Log(scales[i]);
            }

            if (log)
            {
                return prob;
            }
            else
            {
                return Math.Exp(prob);
            }
        }

        //Given a sequence of observations, returns the corresponding list of symbols
        private int convertToSymbol(double[] observation)
        {
            int nearestLabel = 0;
            double[] nearest = centroids[0];
            double dist;

            dist = EuclideanDist(nearest, observation);

            for (int i = 1; i < centroids.Length; i++)
            {
                if (EuclideanDist(centroids[i], observation) < dist)
                {
                    nearestLabel = i;
                    dist = EuclideanDist(centroids[i], observation);
                }
            }

            //If the distance is too far, then return the non-centroid symbol.
            if (dist > 0.3)
            {
                nearestLabel = centroids.Length;
            }
            return nearestLabel;
        }
        //Helper function. Computes the Euclidean distance between two points.
        private double EuclideanDist(double[] v1, double[] v2)
        {
            double sum = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                sum += (v1[i] - v2[i]) * (v1[i] - v2[i]);
            }
            sum = Math.Sqrt(sum);
            return sum;
        }

        //Given an observation sequence, computes the forward variables using scaling (and the scale factors)
        private double[,] computeAlphas(int[] observations, out double[] scales)
        {
            int T = observations.Length;

            double[,] alphas = new double[T, N];
            scales = new double[T];


            //Compute the alpha_0(i)
            for (int i = 0; i < N; i++)
            {
                scales[0] += alphas[0, i] = pi[i] * B[i, observations[0]];
            }
            //Scales the alpha_0(i)
            if (scales[0] != 0) 
            {
                for (int i = 0; i < N; i++)
                {
                    alphas[0, i] = alphas[0, i] / scales[0];
                }
            }


            //Compute and scale the remaining alphas
            for (int t = 1; t < T; t++)
            {
                for (int i = 0; i < N; i++)
                {
                    double p = B[i, observations[t]];
                    double sum = 0.0;

                    for (int j = 0; j < N; j++)
                    {
                        sum += alphas[t - 1, j] * A[j, i];
                    }
                    alphas[t, i] = sum * p;

                    scales[t] += alphas[t, i]; 
                }
                // Scale the alphas
                if (scales[t] != 0) 
                {
                    for (int i = 0; i < N; i++)
                    {
                        alphas[t, i] = alphas[t, i] / scales[t];
                    }
                }
            }
            return alphas;
        }

        private double[,] computeBetas(int[] observations, double[] scales)
        {
            int T = observations.Length;

            double[,] betas = new double[T, N];

            for (int i = 0; i < N; i++)
            {
                if(scales[T-1]!=0)
                {
                    betas[T - 1, i] = (1 / scales[T - 1]);
                }
            }
            for (int t = T - 2; t >= 0; t--)
            {
                for (int i = 0; i < N; i++)
                {
                    betas[t, i] = 0;
                    for (int j = 0; j < N; j++)
                    {
                        betas[t, i] += A[i, j] * B[j, observations[t + 1]] * betas[t + 1, j];
                    }
                    if (scales[t] != 0)
                    {
                        betas[t, i] = betas[t, i] / scales[t];
                    }
                }
            }

            return betas;
        }

        public void Reestimate(double[][][] observationSequences, int iterations, double threshold)
        {
            int[][] observationsSymbols = new int[observationSequences.Length][];
            for (int i = 0; i < observationSequences.Length; i++)
            {
                observationsSymbols[i] = new int[observationSequences[i].Length];
                for (int j = 0; j < observationSequences[i].Length; j++)
                {
                    observationsSymbols[i][j] = convertToSymbol(observationSequences[i][j]);
                }
            }

            Reestimate(observationsSymbols, iterations, threshold);
        }

        private void Reestimate(int[][] observations, int iterations, double threshold)
        {
            int K = observations.Length;
            int currentIteration = 1;
            bool stop = false;

            // Initialization
            double[][, ,] digammas = new double[K][, ,];
            double[][,] gammas = new double[K][,];

            for (int i = 0; i < K; i++)
            {
                int T = observations[i].Length;
                digammas[i] = new double[T, N, N];
                gammas[i] = new double[T, N];
            }

            //Used to determine if we've finished (by comparison with threshold).
            double oldProb = Double.MinValue;
            double newProb = 0;

            do //Repeat until our model is sufficiently well trained
            {
                // For each sequence in the observations input
                for (int i = 0; i < K; i++)
                {
                    var obsSeq = observations[i];
                    int T = obsSeq.Length;
                    double[] scales;

                    double[,] alphas = computeAlphas(observations[i], out scales);
                    double[,] betas = computeBetas(observations[i], scales);

                    for (int t = 0; t < T; t++)
                    {
                        double s = 0;

                        for (int k = 0; k < N; k++)
                        {
                            s += gammas[i][t, k] = alphas[t, k] * betas[t, k];
                        }

                        if (s != 0) 
                        {
                            for (int k = 0; k < N; k++)
                            {
                                gammas[i][t, k] /= s;
                            }
                        }
                    }

                    // Calculate digammas
                    for (int t = 0; t < T - 1; t++)
                    {
                        double s = 0;

                        for (int k = 0; k < N; k++)
                        {
                            for (int l = 0; l < N; l++)
                            {
                                s += digammas[i][t, k, l] = alphas[t, k] * A[k, l] * betas[t + 1, l] * B[l, obsSeq[t + 1]];
                            }
                        }
                        if (s != 0)
                        {
                            for (int k = 0; k < N; k++)
                                for (int l = 0; l < N; l++)
                                    digammas[i][t, k, l] /= s;
                        }
                    }

                    for (int t = 0; t < scales.Length; t++)
                    {
                        newProb += Math.Log(scales[t]);
                    }
                }

                newProb /= observations.Length;


                //If we should stop
                if (converged(oldProb, newProb, currentIteration, iterations, threshold))
                {
                    stop = true; //Say so
                }
                //Otherwise we shouldn't stop. So re-estimate.
                else
                {
                    //Update parameters which decide when we stop
                    currentIteration++;
                    oldProb = newProb;
                    newProb = 0.0;


                    //Re-estimate pi
                    for (int k = 0; k < N; k++)
                    {
                        double sum = 0;
                        for (int i = 0; i < K; i++)
                            sum += gammas[i][0, k];
                        pi[k] = sum / N;
                    }

                    //Re-estimate A.
                    for (int i = 0; i < N; i++)
                    {
                        for (int j = 0; j < N; j++)
                        {
                            double den = 0, num = 0;

                            for (int k = 0; k < K; k++)
                            {
                                int T = observations[k].Length;

                                for (int t = 0; t < T - 1; t++)
                                    num += digammas[k][t, i, j];

                                for (int t = 0; t < T - 1; t++)
                                    den += gammas[k][t, i];
                            }

                            A[i, j] = (den != 0) ? num / den : 0.0;
                        }
                    }

                    //Re-estimate B
                    for (int i = 0; i < N; i++)
                    {
                        for (int j = 0; j < M; j++)
                        {
                            double den = 0, num = 0;

                            for (int k = 0; k < K; k++)
                            {
                                int T = observations[k].Length;

                                for (int t = 0; t < T; t++)
                                {
                                    if (observations[k][t] == j)
                                        num += gammas[k][t, i];
                                }

                                for (int t = 0; t < T; t++)
                                    den += gammas[k][t, i];
                            }
                                                        
                            B[i, j] = (num == 0) ? 1e-10 : num / den;
                        }
                    }

                }

            } while (!stop);
        }

        //Saves the HMM parameters to file
        //N,M
        //pi
        //A
        //B
        //Centroids
        public void saveParameters(string path)
        {
            using(var writer = new StreamWriter(path+name+".csv"))
            {
                writer.WriteLine(N.ToString() + "," + M.ToString()); //Line 1 stores N,M
                string piString = "";
                for (int i = 0; i < N; i++)
                {
                    piString += pi[i].ToString();
                    if (i < N - 1)
                    {
                        piString += ",";
                    }
                }
                writer.WriteLine(piString);

                string ARowString = "";
                for (int i = 0; i < N; i++)
                {
                    ARowString = "";
                    for (int j = 0; j < N; j++)
                    {
                        ARowString += A[i, j].ToString();
                        if (j < N - 1)
                        {
                            ARowString += ",";
                        }
                    }
                    writer.WriteLine(ARowString);
                }

                string BRowString = "";
                for (int i = 0; i < N; i++)
                {
                    BRowString = "";
                    for (int j = 0; j < M; j++)
                    {
                        BRowString += B[i, j].ToString();
                        if (j < M - 1)
                        {
                            BRowString += ",";
                        }
                    }
                    writer.WriteLine(BRowString);
                }
                string centString = "";
                for (int i = 0; i < centroids.Length; i++)
                {
                    centString = "";
                    for (int j = 0; j < centroids[0].Length; j++)
                    {
                        centString += centroids[i][j].ToString();
                        if (j < centroids[0].Length - 1)
                        {
                            centString += ",";
                        }
                    }
                    writer.WriteLine(centString);
                }
                writer.Flush();
                writer.Dispose();
            }
        }

        public void loadParameters(string path)
        {
            string line;
            string[] row;
            using (StreamReader sr = new StreamReader(path + name + ".csv"))
            {
                line = sr.ReadLine();
                if (line != null)
                {
                    row = line.Split(',');
                    N = Convert.ToInt32(row[0]);
                    M = Convert.ToInt32(row[1]);
                    pi = new double[N];
                    A = new double[N, N];
                    B = new double[N, M];
                }
                line = sr.ReadLine();
                if (line != null)
                {
                    row = line.Split(',');
                    for (int i = 0; i < N; i++)
                    {
                        if(row[i]!=null)
                            pi[i] = Convert.ToDouble(row[i]);
                    }
                }

                for (int i = 0; i < N; i++)
                {
                    line = sr.ReadLine();
                    if(line!=null)
                    {
                        row = line.Split(',');
                        for(int j=0; j<N;j++)
                        {
                            if(row[j]!=null)
                            {
                                A[i,j] = Convert.ToDouble(row[j]);
                            }
                        }
                    }
                }

                for (int i = 0; i < N; i++)
                {
                    line = sr.ReadLine();
                    if (line != null)
                    {
                        row = line.Split(',');
                        for (int j = 0; j < M; j++)
                        {
                            if (row[j] != null)
                            {
                                B[i, j] = Convert.ToDouble(row[j]);
                            }
                        }
                    }
                }
                List<double[]> centList = new List<double[]>();
                while ((line = sr.ReadLine()) != null)
                {
                    row = line.Split(',');
                    double[] cent = new double[row.Length];
                    for (int i = 0; i < cent.Length; i++)
                    {
                        cent[i] = Convert.ToDouble(row[i]);
                    }
                    centList.Add(cent);
                }
                centroids = centList.ToArray();
            }
        }

        private bool converged(double oldProb, double newProb,
                    int currIter, int maxIters, double threshold)
        {
            return (currIter > maxIters);
        }
    }
}
