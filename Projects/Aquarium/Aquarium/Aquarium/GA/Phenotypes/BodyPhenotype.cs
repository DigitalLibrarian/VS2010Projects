using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Aquarium.GA.Genomes;
using Aquarium.GA.Bodies;
using Microsoft.Xna.Framework;

namespace Aquarium.GA.Phenotypes
{
    public enum GeneParsers
    {
        BodyPart,
        BodyPartSocket
    }


    public class BodyPartGeneParser
    {
        public List<BodyPartHeader> ReadBodyPartHeaders(BodyGenome genes, GenomeTemplate<double> template)
        {
            int pageSize = 6; // number of doubles to describe BodyPartHeader

            var list = new List<BodyPartHeader>();

            int totalGenes = genes.Genes.Count;
            int totalPages = totalGenes / pageSize;

            for (int page = 0; page < totalPages; page++ )
            {
                var partGene = genes.CondenseSequence(page * pageSize, pageSize, template).Select(x => x.Value).ToList<double>();
                list.Add(new BodyPartHeader(partGene[0], partGene[1], partGene[2], partGene[3], partGene[4], partGene[5]));
            }
           
            return list;
        }



    }

    public struct BodyPartHeader
    {
        public int GeomIndex;
        public Color Color;

        public int AnchorInstance;
        public int PlacementSocket;


        public BodyPartHeader(double geom, double r, double g, double b, double anchor, double placement)
        {
            GeomIndex = Fuzzy.PositiveInteger(geom);
            Color = Fuzzy.ToColor(r, g, b);
            AnchorInstance = Fuzzy.PositiveInteger(anchor);
            PlacementSocket = Fuzzy.PositiveInteger(placement);
        }
    }





}
