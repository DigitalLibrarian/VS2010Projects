using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA;
using Aquarium.GA.Population;
using Aquarium.GA.Genomes;

namespace Aquarium.Sim.Agents
{
    public class OrganismAgent : PopulationMember, IAgent
    {

        public delegate void OnDeathEventHandler(OrganismAgent agent);
        public event OnDeathEventHandler OnDeath;

        public OrganismAgent(BodyGenome genome, Organism organism) : base(genome, organism)
        {
        }


        public void Draw(float duration, Forever.Render.RenderContext renderContext)
        {
            Organism.Body.Render(renderContext);
        }

        public void Update(float duration)
        {
            Organism.Update(duration);

            CheckDead();
        }

        bool postMortum = false;
        private void CheckDead()
        {
            if (postMortum) return;

            postMortum = Organism.IsDead;
            if (postMortum)
            {
                if (OnDeath != null)
                {
                    OnDeath(this);
                }
            }
        }


    }
}
