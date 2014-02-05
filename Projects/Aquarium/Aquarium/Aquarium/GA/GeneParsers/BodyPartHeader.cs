﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Aquarium.GA.GeneParsers
{


    public struct BodyPartHeader
    {
        public int GeomIndex;
        public Color Color;

        public int AnchorInstance;
        public int PlacementSocket;

        public Vector3 Scale;

        public BodyPartHeader(double geom, double r, double g, double b, double anchor, double placement, double scaleX, double scaleY, double scaleZ)
        {
            GeomIndex = Fuzzy.PositiveInteger(geom);
            Color = Fuzzy.ToColor(r, g, b);
            AnchorInstance = Fuzzy.PositiveInteger(anchor);
            PlacementSocket = Fuzzy.PositiveInteger(placement);
            Scale = Fuzzy.ToScaleVector(scaleX, scaleY, scaleZ);
        }

        public static BodyPartHeader FromGenes(List<double> partGene)
        {
            return new BodyPartHeader(
                    partGene[0], partGene[1], partGene[2],
                    partGene[3], partGene[4], partGene[5],
                    partGene[6], partGene[7], partGene[8]
                    );
        }
    }

}
