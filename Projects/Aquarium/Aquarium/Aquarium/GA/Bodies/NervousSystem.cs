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
        public Body Body { get; private set; }



        public NervousSystem(Body body)
        {
            Body = body;
        }


        public void Update()
        {
            foreach (var part in Body.Parts)
            {
                foreach (var organ in part.Organs)
                {
                    organ.Update(this);
                }
            }
        }

        
        
        
    }
}
