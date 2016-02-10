using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Aquarium.Life.Spec;

namespace Aquarium.Life.Phenotypes
{
    public class BodyPartPhenotype : ComponentPheno, IBodyPartPhenotype
    {
        public int BodyPartGeometryIndex { get; set; }
        public Color Color { get; set; }

        public IChanneledSignalPhenotype ChanneledSignalGenome { get; set; }

        public IInstancePointer AnchorPart { get; set; }
        public IInstancePointer PlacementPartSocket { get; set; }

        public BodyPartPhenotype(BodyPartSpec header)
        {
            Color = header.Color;
            BodyPartGeometryIndex = header.Type;
            AnchorPart = new InstancePointer(header.AnchorNodeId);
            PlacementPartSocket = new InstancePointer(header.PlacementPartSocket);
            Scale = new Vector3(header.ScaleX, header.ScaleY, header.ScaleZ);
        }

        public Vector3 Scale
        {
            get;
            set;
        }
    }
}
