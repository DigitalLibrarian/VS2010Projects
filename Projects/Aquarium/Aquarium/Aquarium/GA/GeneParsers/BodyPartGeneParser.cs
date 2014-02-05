using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Genomes;

namespace Aquarium.GA.GeneParsers
{

    public class BodyPartGeneParser
    {
        public List<BodyPartHeader> ReadBodyPartHeaders(BodyGenome genes, GenomeTemplate<double> template)
        {
            int pageSize = 9; // number of doubles to describe BodyPartHeader

            var list = new List<BodyPartHeader>();

            // TODO - replace this "use all the data \o/" approach with something that iterates through the string and recognizes
            // start, stop, and goto codons

            int totalGenes = genes.Genes.Count;
            int totalPages = totalGenes / pageSize;

            for (int page = 0; page < totalPages; page++)
            {
                var partGene = genes.CondenseSequence(page * pageSize, pageSize, template).Select(x => x.Value).ToList<double>();
                list.Add(BodyPartHeader.FromGenes(partGene));
            }

            return list;
        }



    }

}
