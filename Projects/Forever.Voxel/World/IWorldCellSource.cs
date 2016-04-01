using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Forever.SpacePartitions;

namespace Forever.Voxel.World
{
    public interface IWorldCellSource
    {
        WorldCell Get(SpaceCoord coord);
    }
}