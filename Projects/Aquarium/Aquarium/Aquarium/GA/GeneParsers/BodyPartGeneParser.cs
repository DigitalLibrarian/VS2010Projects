using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Genomes;
using Aquarium.GA.Codons;

namespace Aquarium.GA.GeneParsers
{

    public class BodyPartGeneParser
    {
        public List<BodyPartHeader> ReadBodyPartHeaders(List<int> genes)
        {

            var list = new List<BodyPartHeader>();
            int pageSize = 9; // number of doubles to describe BodyPartHeader
            
            // TODO - replace this "use all the data \o/" approach with something that iterates through the string and recognizes
            // start, stop, and goto codons

            int totalGenes = genes.Count;
            int totalPages = totalGenes / pageSize;

            for (int page = 0; page < totalPages; page++)
            {
                //var partGene = genes.CondenseSequence(page * pageSize, pageSize, template).Select(x => x.Value).ToList<double>();
                var partGene = genes.GetRange(page * pageSize, pageSize);
                var header = BodyPartHeader.FromGenes(partGene);
                list.Add(header);
            }


            



            return list;
        }



    }

}
