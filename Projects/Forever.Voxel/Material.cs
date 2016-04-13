using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Forever.Voxel
{
    // TODO - this might benefit from the "many singletons" pattern from the millington book that he uses with state machines
    //      - The bottom line is that there should only be one immutable instance of say "copper" or "mud" instead of
    //         duplicating all that data in memory.
    public class Material
    {
        public Color Color { get; private set; }

        public Material(Color color)
        {
            Color = color;
        }
    }
}
