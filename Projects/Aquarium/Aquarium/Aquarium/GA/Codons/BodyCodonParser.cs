using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Genomes;
using Aquarium.GA.Phenotypes;
using Aquarium.GA.GeneParsers;

namespace Aquarium.GA.Codons
{
    public class BodyCodonParser : CodonParser<double>
    {
        protected List<double> ParseBodyPartClump(BodyGenome genome, GenomeTemplate<double> template, int startIndex)
        {
            return ReadUntilOrEnd(genome, template, new BodyPartEndCodon(), startIndex);
        }

        public IBodyPhenotype ParseBodyPhenotype(BodyGenome g, GenomeTemplate<double> t)
        {
            
            var p = new BodyPartGeneParser();
            var headers = p.ReadBodyPartHeaders(g, t);


            BodyPhenotype bodyP = new BodyPhenotype();
            headers.ForEach(header =>
            {
                var partOne = new BodyPartPhenotype();
                partOne.Color = header.Color;
                partOne.BodyPartGeometryIndex = header.GeomIndex;
                partOne.AnchorPart = new InstancePointer(header.AnchorInstance);
                partOne.PlacementPartSocket = new InstancePointer(header.PlacementSocket);
                partOne.Scale = header.Scale;

                bodyP.BodyPartPhenos.Add(partOne);

            });


            return bodyP;
        }
    }
}
