using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Aquarium.GA.SpacePartitions;

namespace Aquarium.GA.Environments
{
    public interface ISurroundings
    {
        IEnumerable<IFood> ClosestFoods(Vector3 pos, float radius);
    }
}
