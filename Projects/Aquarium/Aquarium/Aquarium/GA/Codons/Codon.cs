using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.GA.Codons
{
    public abstract class Codon
    {
        public abstract int FrameSize { get; }
        public abstract bool Recognize(List<double> genes);
        public abstract List<double> Example(); 
    }
}
