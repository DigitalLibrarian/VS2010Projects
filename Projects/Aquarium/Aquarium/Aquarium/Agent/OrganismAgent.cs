using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using Forever.Physics;
using Forever.Render;

using Aquarium.Life;
using Aquarium.Life.Genomes;
using Aquarium.Life.Population;

using Aquarium.Ui.Targets;

using Aquarium.Sim;
using Forever.Extensions;
using Aquarium.Targeting;

namespace Aquarium.Agent
{
    public class OrganismAgent : PopulationMember, IAgent, IRayPickable, ITarget
    {
        public delegate void OnDeathEventHandler(OrganismAgent agent);
        public event OnDeathEventHandler OnDeath;
        int totalOrgans;
        public OrganismAgent(BodyGenome genome, Organism organism) : base(genome, organism)
        {
            totalOrgans = organism.Body.Parts.Sum(p => p.Organs.Count);
        }


        public void Draw(float duration, RenderContext renderContext)
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

        bool IRayPickable.IsHit(Microsoft.Xna.Framework.Ray ray)
        {
            var maybe = ray.Intersects(Organism.WorldBB);
            return maybe != null;
        }

        string ITarget.Label
        {
            get { 
                return string.Format(
                        "Organism {0} - {1}\n"
                    +   "Age: {2} Energy: {3}\n"
                    +   "Parts: {4} Organs: {5}",

                    Organism.Position.Round().ToString(),
                    Organism.IsDead ? "Dead" : "Alive",
                    Organism.Age,
                    Organism.Energy,
                    Organism.Body.Parts.Count,
                    totalOrgans
                    ); 
            }
        }

        BoundingBox ITarget.TargetBB
        {
            get { return Organism.WorldBB; }
        }
    }
}
