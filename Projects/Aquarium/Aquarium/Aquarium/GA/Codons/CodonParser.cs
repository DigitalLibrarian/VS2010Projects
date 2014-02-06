using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Genomes;
using Aquarium.GA.Codons;

namespace Aquarium.GA.Codons
{
    public abstract class CodonParser
    {
        List<Codon> Testers { get; set; }
        public CodonParser(List<Codon> testers)
        {
            Testers = testers;
        }

        protected List<double> ReadUntilOrEnd(BodyGenome genome, GenomeTemplate<double> template, Codon tester, int startIndex)
        {
            return ReadUntilOrEnd( genome, template, new Codon[] { tester },  startIndex);
        }
        protected List<double> ReadUntilOrEnd(BodyGenome genome, GenomeTemplate<double> template, Codon[] testers, int startIndex)
        //protected List<double> ReadUntilOrEnd(BodyGenome genome, GenomeTemplate<double> template, Codon[] codons, int startIndex)
        {
            List<double> dataRead = new List<double>();
            Traversal(genome, template, startIndex, (name) =>
            {
                foreach (var tester in testers)
                {
                    if (RecognizeCodonDefinition(genome, template, startIndex, tester))
                    {
                        return false;
                    }
                }
                var data = genome.ByName(name, template).Value;
                dataRead.Add(data);

                return true;
            });

            return dataRead;
        }

        protected int Traversal(BodyGenome genome, GenomeTemplate<double> template, int startIndex, Predicate<int> nameVisitor)
        {
            // we know the entire number of genes in the genome.
            // so we will go until we are a couple multiples of that to allow for it to reuse existing genetic material 
            int index = startIndex;
            int traversed = 0;
            var maxTraversal = genome.Size * 20;

            while (traversed < maxTraversal)
            {
                if (!nameVisitor(index + traversed))
                {
                    return traversed;
                }

                traversed++;
            }
            return traversed;
        }

        public bool RecognizeCodonDefinition(BodyGenome genome, GenomeTemplate<double> template, int index, Codon defn)
        {
             var sequence = 
                    genome
                        .CondenseSequence(index, defn.FrameSize, template)
                        .Select(x => x.Value)
                        .ToList();
             return defn.Recognize(sequence);
        }

        
         
    }
}
