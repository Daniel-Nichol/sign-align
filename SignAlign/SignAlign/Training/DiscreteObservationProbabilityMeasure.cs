using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;

namespace SignAlign
{
    class DiscreteObservationProbabilityMeasure : ObersvationProbabilityMeasure
    {
        private MarkovChain A;
        private DenseMatrix B; //The probability matrix 
        public int range { get; private set; }
        public DiscreteObservationProbabilityMeasure(MarkovChain A, DenseMatrix B)
        {
            this.A = A;
            this.B = B;
            this.range = B.ColumnCount;
        }

        public double emissionProb(int state, IObersvation o)
        {
            //If the observation is of the wrong type the the probability is 0;
            if (o.GetType() == typeof(DiscreteObservation))
            {
                DiscreteObservation ob = (DiscreteObservation)o;
                if (ob.obsVal < 0 || ob.obsVal >= range)
                {
                    return 0;
                }
                else
                {
                    double prob = B.ToArray()[state, ob.obsVal];
                    return prob;
                }
            }
            else
            {
                return 0;
            }
        }
    }
}
