using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.GA.Codons
{

    public enum Codon { None, BodyEnd, BodyPartStart, BodyPartEnd }

    public abstract class CodonDefinition
    {
        public abstract Codon Codon { get; }
        public abstract int FrameSize { get; }
        public abstract bool Recognize(List<double> genes);
        public abstract List<double> Example(); 
    }
}
