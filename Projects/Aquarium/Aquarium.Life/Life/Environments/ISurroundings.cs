using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Aquarium.Life.Environments
{
    public interface ISurroundings
    {
        IEnumerable<IFood> ClosestFoods(Vector3 pos, float radius);

        //TODO
        //IFood FirstRayCastFood(Ray ray);
    }
}
