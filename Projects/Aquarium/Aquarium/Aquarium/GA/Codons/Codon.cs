using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.GA.Codons
{
    public abstract class Codon<TCGene>
    {
        public abstract int FrameSize { get; }
        public abstract bool Recognize(List<TCGene> genes);
        public abstract List<TCGene> Example(); 
    }

    public abstract class BodyCodon : Codon<double>  {}
}
