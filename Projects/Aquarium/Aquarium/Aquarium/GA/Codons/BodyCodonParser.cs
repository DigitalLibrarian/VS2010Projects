using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Genomes;

namespace Aquarium.GA.Codons
{
    class BodyCodonParser : CodonParser
    {
        public BodyCodonParser(List<CodonDefinition> codonDefs)
            : base(codonDefs)
        {
        }

        protected List<double> ParseBodyPartClump(BodyGenome genome, GenomeTemplate<double> template, int startIndex)
        {
            return ReadUntilOrEnd(genome, template, Codon.BodyPartEnd, startIndex);
        }
    }
}
