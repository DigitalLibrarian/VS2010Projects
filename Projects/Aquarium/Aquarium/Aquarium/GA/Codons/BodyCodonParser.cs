using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Genomes;
using Aquarium.GA.Phenotypes;
using Aquarium.GA.GeneParsers;

namespace Aquarium.GA.Codons
{

    public class BodyCodonParser : CodonParser<int>
    {


        protected List<int> ParseBodyPartClump(BodyGenome genome, GenomeTemplate<int> template, int startIndex)
        {
            return ReadUntilOrEnd(genome, template, new BodyPartEndCodon(), startIndex);
        }

        

        public IBodyPhenotype ParseBodyPhenotype(BodyGenome g, GenomeTemplate<int> t)
        {
            BodyPhenotype bodyP = null;
            if (!g.Genes.Any()) return bodyP;
            bodyP = new BodyPhenotype();

            var iterator = 0;
            int maxRead = g.Size * 2;
            List<int> clump;
            var bodyPartStart = new BodyPartStartCodon();
            var bodyPartEnd = new BodyPartEndCodon();
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

                    clump = ParseBodyPartClump(g, t, scan);

                    if (clump.Count() >= BodyPartHeader.Size)
                    {
                        var header = BodyPartHeader.FromGenes(clump);
                        bodyP.BodyPartPhenos.Add(new BodyPartPhenotype(header));
                    }

                    iterator += clump.Count();
                    scan = iterator % maxRead;
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
