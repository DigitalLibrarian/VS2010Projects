using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Aquarium.GA.Organs;
using Forever.Neural;
using Aquarium.GA.Signals;

namespace Aquarium.GA.Bodies
{
    public class NervousSystem
    {
        List<Organ> _organs = new List<Organ>();
        public Organism Organism { get; private set; }

        public NervousSystem(Organism body)
        {
            Organism = body;
        }


        public void Update()
        {
            foreach (var part in Organism.Body.Parts)
            {
                foreach (var organ in part.Organs)
                {
                    organ.Update(this);
                }
            }
        }

        
        
        
    }
}
