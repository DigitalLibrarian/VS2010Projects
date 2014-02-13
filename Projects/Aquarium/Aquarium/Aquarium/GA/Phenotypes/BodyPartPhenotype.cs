using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Aquarium.GA.Headers;

namespace Aquarium.GA.Phenotypes
{
    public class BodyPartPhenotype : ComponentPheno, IBodyPartPhenotype
    {
        public int BodyPartGeometryIndex { get; set; }
        public Color Color { get; set; }

        public IChanneledSignalPhenotype ChanneledSignalGenome { get; set; }

        public IInstancePointer AnchorPart { get; set; }
        public IInstancePointer PlacementPartSocket { get; set; }

        public BodyPartPhenotype(BodyPartHeader header)
        {
            Scale = new Vector3(1f, 1f, 1f);

            Color = header.Color;
            BodyPartGeometryIndex = header.GeomIndex;
            AnchorPart = new InstancePointer(header.AnchorInstance);
            PlacementPartSocket = new InstancePointer(header.PlacementSocket);
            Scale = header.Scale;
        }

        public Vector3 Scale
        {
            get;
            set;
        }
    }
}
