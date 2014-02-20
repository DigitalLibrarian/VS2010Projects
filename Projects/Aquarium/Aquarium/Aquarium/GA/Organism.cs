using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Forever.Neural;

using Aquarium.GA.Bodies;
using Microsoft.Xna.Framework;

namespace Aquarium.GA
{
    public class Organism
    {
        public Body Body { get; private set; }

        public Organism(Body b)
        {
            Body = b;
        }

        float rot = 0f;
        public void Update(float duration)
        {

            Body.World = Matrix.CreateRotationY(rot += 0.01f)
                * Matrix.CreateRotationX(rot);

            Body.Update(duration);
        }
    }




    
    
    
}
