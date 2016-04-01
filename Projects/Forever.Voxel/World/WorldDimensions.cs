using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forever.Voxel.World
{
    public class WorldDimensions
    {
        public int VoxelsPerDimension { get; private set; }

        public WorldDimensions(int voxelsPerDimension)
        {
            VoxelsPerDimension = voxelsPerDimension;
        }
    }
}
