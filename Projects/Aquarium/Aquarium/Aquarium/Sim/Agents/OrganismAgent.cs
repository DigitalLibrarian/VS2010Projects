using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA;

namespace Aquarium.Sim.Agents
{
    class OrganismAgent : IAgent
    {
        Organism Organism { get; set; }

        public OrganismAgent(Organism organism)
        {
            Organism = organism;
        }


        public void Draw(float duration, Forever.Render.RenderContext renderContext)
        {
            Organism.Body.Render(renderContext);
        }

        public void Update(float duration)
        {
            Organism.Update(duration);

            if (Organism.IsDead)
            {
                throw new Exception("Uh oh. Somebody died");
            }
        }


    }
}
