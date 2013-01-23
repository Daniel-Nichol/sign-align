using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;

namespace WpfApplication1
{   
    /// <summary>
    /// A class to encapsulate a Hidden Markov Model
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
        public HiddenMarkovModel()
        {

        }
        /// <summary>
        /// Returns the probability that a given sequence of observation was created by this Markov model
        /// 
        /// Uses the forward/backward algorithm
        /// </summary>
        public double probObservations()
        {
            return 0;
        }
    }
}
