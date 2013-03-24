using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignAlign
{
    class DiscreteObservation : IObersvation
    {
        public int range { get; private set; } //range: [0,...,range)
        public int obsVal {get;private set;}
        public DiscreteObservation(int ob)
        {
            this.obsVal = ob;
            range = 4;
           
        }

    }
}
