using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Aquarium.GA.Population;
using Aquarium.GA.Environments;
using Forever.SpacePartitions;

namespace Aquarium.GA.SpacePartitions
{


    public class EnvironmentSpace : Space<IEnvMember> 
    {

        int FoodGridSize;
        public EnvironmentSpace(int gridSize, int foodGridSize) : base(gridSize) 
        {
            FoodGridSize = foodGridSize;
        }

        protected override IPartition<IEnvMember> CreateNewPartition(Microsoft.Xna.Framework.BoundingBox box)
        {
            return new EnvPartition(box, FoodGridSize);
        }
    }
}
