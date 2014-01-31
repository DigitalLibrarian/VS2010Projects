using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Forever.Neural;

using Aquarium.GA.Bodies;

namespace Aquarium.GA
{
    public class Organism
    {
        public Body Body { get; private set; }
        
        public void Update(float duration)
        {
            Body.Update(duration);
        }
    }




    
    
    
}
