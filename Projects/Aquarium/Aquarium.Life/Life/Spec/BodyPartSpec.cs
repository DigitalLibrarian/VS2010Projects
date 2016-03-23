using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Aquarium.Life.Spec
{
    public class BodyPartSpec
    {
        public int Type { get; set; }
        public int AnchorNodeId { get; set; }
        public int PlacementPartSocket { get; set; }
        public int ChanneledSignal { get; set; }
        public float ScaleX { get; set; }
        public float ScaleY { get; set; }
        public float ScaleZ { get; set; }
        public Color Color { get; set; }

        public static BodyPartSpec FromGenome(IEnumerator<int> g)
        {
            return new BodyPartSpec
            {
                Type = Fuzzy.PositiveInteger(g.Next()),
                AnchorNodeId = Fuzzy.PositiveInteger(g.Next()),
                PlacementPartSocket = Fuzzy.PositiveInteger(g.Next()),
                ChanneledSignal = Fuzzy.PositiveInteger(g.Next()),
                ScaleX = Fuzzy.CircleClamp(Fuzzy.PositiveFloat(g.Next()), 0.01f, 10f),
                ScaleY = Fuzzy.CircleClamp(Fuzzy.PositiveFloat(g.Next()), 0.01f, 10f),
                ScaleZ = Fuzzy.CircleClamp(Fuzzy.PositiveFloat(g.Next()), 0.01f, 10f),
                Color = new Color(
                    Fuzzy.CircleClamp(Fuzzy.PositiveInteger(g.Next()), 0, 255),
                    Fuzzy.CircleClamp(Fuzzy.PositiveInteger(g.Next()), 0, 255),
                    Fuzzy.CircleClamp(Fuzzy.PositiveInteger(g.Next()), 0, 255)
                )
            };
        }
    }
}
