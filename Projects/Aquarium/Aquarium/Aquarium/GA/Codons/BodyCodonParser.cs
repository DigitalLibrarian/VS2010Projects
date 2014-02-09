using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Genomes;
using Aquarium.GA.Phenotypes;
using Aquarium.GA.GeneParsers;

namespace Aquarium.GA.Codons
{
    public abstract class AqParser : CodonParser<int>
    {
        public List<int> Extract(BodyGenome genome, GenomeTemplate<int> template, int startIndex, Codon<int> recognizer, Codon<int>[] terminals)
        {
            var iterator = startIndex;
            int maxRead = genome.Size;
            List<int> clump = new List<int>();

            bool endRecognized = false;
            do
            {
                var scan = iterator % maxRead;
                var seq = genome.CondenseSequence(scan, recognizer.FrameSize, template);
                var data = seq.Select(gene => gene.Value).ToList();
                if (recognizer.Recognize(data))
                {
                    iterator += recognizer.FrameSize;
                    scan = iterator % maxRead;

                    clump = ReadUntilOrEnd(genome, template, terminals, scan);

                    return clump;
                }
                else
                {
                    foreach (var term in terminals)
                    {
                        if (term.Recognize(data))
                        {
                            return clump;
                        }
                    }
                    iterator++;
                }
            } while (iterator < maxRead && !endRecognized);

            return clump;
        }
    }

    public class BodyCodonParser : AqParser
    {
        

        

        public IBodyPhenotype ParseBodyPhenotype(BodyGenome g, GenomeTemplate<int> t)
        {
            BodyPhenotype bodyP = null;
            if (!g.Genes.Any()) return bodyP;
            bodyP = new BodyPhenotype();

            var iterator = 0;
            int maxRead = g.Size ;
            List<int> clump;
            var bodyPartStart = new BodyPartStartCodon();
            var bodyEnd = new BodyEndCodon();
            bool endRecognized = false;
            do
            {
                var scan = iterator % maxRead;
                var seq = g.CondenseSequence(scan, bodyPartStart.FrameSize, t);
                var data = seq.Select(gene => gene.Value).ToList();
                if (bodyPartStart.Recognize(data))
                {
                    iterator += bodyPartStart.FrameSize;
                    scan = iterator % maxRead;

                    clump = ReadUntilOrEnd(
                        g, t,
                        new Codon<int>[]  {
                            new BodyPartEndCodon(),
                            new BodyPartStartCodon(),
                            new BodyEndCodon()
                        },
                        scan);


                    iterator += clump.Count();
                    scan = iterator % maxRead;

                    if (clump.Count() >= BodyPartHeader.Size)
                    {
                        var header = BodyPartHeader.FromGenes(clump);
                        bodyP.BodyPartPhenos.Add(new BodyPartPhenotype(header));

                        
                        var organClump = Extract(g, t, scan, new OrganStartCodon(), new Codon<int>[] { new OrganEndCodon() });
                        if (organClump.Any())
                        {
                            throw new NotImplementedException();
                        }

                    }

                }
                else if (bodyEnd.Recognize(data))
                {
                    endRecognized = true;
                }
                else
                {
                    iterator++;
                }

            } while (iterator < maxRead && !endRecognized);
            
            return bodyP;
        }
    }
}
