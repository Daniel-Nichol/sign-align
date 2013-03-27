using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignAlign
{
    class KMeansClassifier
    {

        int K; //The number of clusters

        double[][] centroids; //The Centroids of the k clusters

        public KMeansClassifier(int K)
        {
            this.K = K;
            centroids = new double[K][];
        }

        //Chooses K random centroids from the given observations
        private double[][] chooseRandomCentroids(double[][] observations)
        {
            Random rs = new Random();
            double[][] cents = new double[K][];
            int[] centroidsChosen = new int[K];
            for (int i = 0; i < K; i++)
            {
                centroidsChosen[i] = -1;
            }

            int j = 0;
            int randomCentroid;

            while (centroidsChosen[K - 1] == -1)
            {
                randomCentroid = rs.Next(0, observations.Length - 1);
                if (!centroidsChosen.Contains<int>(randomCentroid))
                {
                    centroidsChosen[j] = randomCentroid;
                    cents[j] = observations[j];
                    j++;
                }
            }

            return cents;
        }

        //Computes the clusters via a greedy algorithm
        public int[] computeClusters(double[][] observations, double threshold)
        {
            int T = observations.Length;
            int dims = observations[0].Length;
            bool finished = false;

            //Choose K initial centroids at random
            centroids = chooseRandomCentroids(observations);

            int[] count = new int[K];
            int[] labels = new int[T];
            double[][] newCentroids;


            while (!finished)
            {
                newCentroids = new double[K][];
                for (int i = 0; i < K; i++)
                {
                    newCentroids[i] = new double[dims];
                    count[i] = 0;
                }

                //Sorts the observations into clusters based on their nearest centroid
                for (int i = 0; i < observations.Length; i++)
                {
                    double[] point = observations[i];

                    labels[i] = nearestCentroidLabel(observations[i]);
                    count[labels[i]]++;

                    for (int j = 0; j < point.Length; j++)
                    {
                        newCentroids[labels[i]][j] += point[j];
                    }
                }

                //Normalize the new centroid
                for (int i = 0; i < K; i++)
                {
                    for (int j = 0; j < dims; j++)
                    {
                        newCentroids[i][j] /= count[i];
                    }
                }

                finished = hasFinished(centroids, newCentroids, threshold);
                if (!finished)
                {
                    centroids = newCentroids;
                }
            }
            return labels;
        }

        //Determines the label of the nearest centroid to a point
        private int nearestCentroidLabel(double[] point)
        {
            int nearestLabel = 0;
            double[] nearest = centroids[0];
            double dist = EuclideanDist(nearest, point);

            for (int i = 1; i < centroids.Length; i++)
            {
                if (EuclideanDist(centroids[i], point) < dist)
                {
                    nearestLabel = i;
                    dist = EuclideanDist(centroids[i], point);
                }
            }

            return nearestLabel;
        }

        //Helper function computes the Euclidean distance between two vectors.
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

        //Determines when the k-means has finished, this occurs when the centroids change by less than the threshold
        private bool hasFinished(double[][] centroids, double[][] newCentroids, double threshold)
        {
            bool finished = true;

            for (int i = 0; i < centroids.Length; i++)
            {
                if (EuclideanDist(centroids[i], newCentroids[i]) > threshold)
                {
                    finished = false;
                }
            }
            return finished;
        }
    }

}
