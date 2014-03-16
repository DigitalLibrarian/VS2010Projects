using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Aquarium.Life.Organs;
using Forever.Neural;
using Aquarium.Life.Signals;

namespace Aquarium.Life.Bodies
{
    public class NervousSystem
    {
        List<Organ> _organs = new List<Organ>();
        public Organism Organism { get; private set; }

        public bool IsDead { get { return Organism.IsDead; } }

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
