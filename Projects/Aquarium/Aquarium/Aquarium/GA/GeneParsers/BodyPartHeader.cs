using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Aquarium.GA.GeneParsers
{
    //TODO - catch over flows
    public struct BodyPartHeader
    {
        public static int Size { get { return 9; } }

        public int GeomIndex;
        public Color Color;

        public int AnchorInstance;
        public int PlacementSocket;

        public Vector3 Scale;

        public BodyPartHeader(int geom, int r, int g, int b, int anchor, int placement, int scaleX, int scaleY, int scaleZ)
        {
            /*
            GeomIndex = Fuzzy.PositiveInteger(geom);
            Color = Fuzzy.ToColor(r, g, b);
            AnchorInstance = Fuzzy.PositiveInteger(anchor);
            PlacementSocket = Fuzzy.PositiveInteger(placement);
            Scale = Fuzzy.ToScaleVector(scaleX, scaleY, scaleZ);
            Scale = Vector3.Max(Scale, new Vector3(0.01f, 0.01f, 0.01f)); //minimum cap
            Scale = Vector3.Min(Scale, new Vector3(10f, 100f, 10f)); //maximum cap
             * */

            GeomIndex = geom;
            Color = new Color(r, b, g);
            AnchorInstance = anchor;
            PlacementSocket = placement;
            Scale = Fuzzy.ToScaleVector(scaleX, scaleY, scaleZ);
            Scale = Vector3.Max(Scale, new Vector3(0.01f, 0.01f, 0.01f)); //minimum cap
            Scale = Vector3.Min(Scale, new Vector3(10f, 100f, 10f)); //maximum cap
        }

        public static BodyPartHeader FromGenes(List<int> partGene)
        {
            return new BodyPartHeader(
                    partGene[0], partGene[1], partGene[2],
                    partGene[3], partGene[4], partGene[5],
                    partGene[6], partGene[7], partGene[8]
                    );
        }
    }

}
