using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Genomes;
using Aquarium.GA.Codons;

namespace Aquarium.GA.Codons
{
    public abstract class CodonParser<TGene>
    {
        protected List<TGene> ReadUntilOrEnd(Genome<TGene> genome, GenomeTemplate<TGene> template, Codon<TGene> tester, int startIndex)
        {
            return ReadUntilOrEnd(genome, template, new Codon<TGene>[] { tester }, startIndex);
        }
        protected List<TGene> ReadUntilOrEnd(Genome<TGene> genome, GenomeTemplate<TGene> template, Codon<TGene>[] testers, int startIndex)
        {
            List<TGene> dataRead = new List<TGene>();
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

        protected int Traversal(Genome<TGene> genome, GenomeTemplate<TGene> template, int startIndex, Predicate<int> nameVisitor)
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

        protected bool RecognizeCodonDefinition(Genome<TGene> genome, GenomeTemplate<TGene> template, int index, Codon<TGene> defn)
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
