using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication1
{
    /// <summary>
    /// A base class for Hidden Markov Models
    /// </summary>
    abstract class HiddenMarkovModel
    {
        protected MarkovChain A; 
        /// <summary>
        /// Returns the probability that a given sequence of observation was created by this Markov model
        ///
        /// Uses the forward/backward algorithm. No scaling is used here.
        /// </summary>
        public abstract double probObservations(List<IObersvation> observations);

        /// <summary>
        /// Re-estimate the model parameters given a list of obersvations
        /// </summary>
        public abstract void reestimateParameters(List<IObersvation> observations);
        /// <summary>
        /// Iteratively improves the models until it is (locally) optimal
        /// </summary>
        public abstract void trainModel(List<IObersvation> observations);

    }

}
