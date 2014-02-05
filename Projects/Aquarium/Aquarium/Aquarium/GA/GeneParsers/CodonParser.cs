using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Genomes;

namespace Aquarium.GA.GeneParsers
{
    public abstract class CodonParser
    {
        List<CodonDefinition> Testers { get; set; }
        public CodonParser(List<CodonDefinition> testers)
        {
            Testers = testers;
        }

        List<double> ReadUntilOrEnd(BodyGenome genome, GenomeTemplate<double> template, Codon codon, int startIndex)
        {
            List<double> dataRead = new List<double>();

            // we know the entire number of genes in the genome.
            // so we will go until we are twice that to allow for it to reuse existing genetic material 
            int index = startIndex;
            int traversed = 0;
            var numGenes = genome.Size;
            while (traversed < numGenes)
            {
                var found = RecognizeCodons(genome, template, index);
                if(found == Codon.None)
                {
                    dataRead.Add(genome.ByName(index,  template).Value);
                }

                if (found == codon)
                {
                    return dataRead;
                }
                
                traversed++;
            }

            return null;
        }

        private List<double> ParseBodyPartClump(BodyGenome genome, GenomeTemplate<double> template, int startIndex)
        {
            return ReadUntilOrEnd(genome, template, Codon.BodyPartEnd, startIndex);
        }



        private Codon RecognizeCodons(BodyGenome genome, GenomeTemplate<double> template, int index)
        {
            foreach (var tester in Testers)
            {
                if (tester.Recognize(genome.CondenseSequence(index, tester.FrameSize, template).Select(x => x.Value).ToList()))
                {
                    return tester.Codon;
                }
            }

            return Codon.None;
        }
    }
}
