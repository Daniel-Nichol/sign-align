using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;

namespace WpfApplication1
{
    class MarkovChain
    {
        public int numberOfStates {get; private set; } //The number of states in the Markov Chain
        private DenseMatrix stochMatrix { get; set; } //An NxN matrix, the transition probabilities for the MC

        /// <summary>
        /// Initialize a Markov chain from a specific stochastic matrix
        /// </summary>
        /// <param name="A">The stochastic matrix</param>
        public MarkovChain(DenseMatrix A)
        {
            stochMatrix = A;
            numberOfStates = A.RowCount;
        }

        /// <summary>
        /// Returns the probability a_ij of transtition to state S_j from S_i
        /// </summary>
        public double getTransitionProb(int i, int j)
        {
            return stochMatrix.ToArray()[i, j];
        }


    }
}
