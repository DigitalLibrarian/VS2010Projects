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
            return ReadUntilOrEnd( genome, template, new Codon[] { codon },  startIndex);
        }
        List<double> ReadUntilOrEnd(BodyGenome genome, GenomeTemplate<double> template, Codon[] codons, int startIndex)
        {
            List<double> dataRead = new List<double>();
            Traversal(genome, template, startIndex, (name) =>
            {
                var found = RecognizeCodons(genome, template, name);
                if (found == Codon.None)
                {
                    var data = genome.ByName(name, template).Value;
                    dataRead.Add(data);
                }

                if (codons.Contains(found))
                {
                    return false;
                }

                return true;
            });

            return dataRead;
        }

        private void Traversal(BodyGenome genome, GenomeTemplate<double> template, int startIndex, Predicate<int> nameVisitor)
        {

            // we know the entire number of genes in the genome.
            // so we will go until we are a couple multiples of that to allow for it to reuse existing genetic material 
            int index = startIndex;
            int traversed = 0;
            var maxTraversal = genome.Size * 2;
            while (traversed < maxTraversal)
            {
                var found = RecognizeCodons(genome, template, index + traversed);
                if (found == Codon.None)
                {
                    if (!nameVisitor(index + traversed))
                    {
                        return;
                    }
                }

                traversed++;
            }

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
