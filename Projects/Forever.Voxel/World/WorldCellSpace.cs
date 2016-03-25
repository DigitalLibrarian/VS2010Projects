using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.SpacePartitions;
using Microsoft.Xna.Framework;

namespace Forever.Voxel.World
{
    public class WorldCellSpace : Space<WorldCell>
    {
        IWorldCellSource Source { get; set; }
        public WorldCellSpace(int gridSize, IWorldCellSource source) : base(gridSize)
        {
            Source = source;
        }
    }
}
