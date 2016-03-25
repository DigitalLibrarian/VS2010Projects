
using Forever.SpacePartitions;
using Microsoft.Xna.Framework;
using Forever.Render;
using System.Collections.Generic;
using Aquarium.Life.Environments;

using Aquarium.Agent;

namespace Aquarium.Sim
{
    public class SimSpace : DynamicSpace<IAgent>
    {
        public float UpdateRadius { get; private set; }
        int FoodSpaceGridSize;
        public SimSpace(int gridSize, int foodSpaceGridSize)
            : base(gridSize)
        {
            FoodSpaceGridSize = foodSpaceGridSize;
        }

        protected override IPartition<IAgent> CreateNewPartition(Microsoft.Xna.Framework.BoundingBox box)
        {
            return new SimSpacePartition(this, box, FoodSpaceGridSize);
        }
    }
}
