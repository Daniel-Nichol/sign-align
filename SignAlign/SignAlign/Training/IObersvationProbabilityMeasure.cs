using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication1
{
    /// <summary>
    /// An interface which is a base for the funtions that determine
    /// the probability b_j(O) of an observation O at state S_j of a Markov chain
    /// </summary>
    interface ObersvationProbabilityMeasure
    {
        double emissionProb(int state, IObersvation O);
    }
}
