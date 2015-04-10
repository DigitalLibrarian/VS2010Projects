using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Aquarium.Sim.Agents;

namespace Aquarium.Life.Environments
{
    public interface ISurroundings
    {
        void Track(Organism org);

        IEnumerable<IFood> ClosestFoods(Vector3 pos, float radius);
        IEnumerable<OrganismAgent> ClosestOrganisms(Vector3 pos, float radius);

        //TODO
        //IFood FirstRayCastFood(Ray ray);
    }
}
