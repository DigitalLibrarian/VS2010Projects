using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Genomes;
using Microsoft.Xna.Framework;
using Aquarium.GA.Environments;

namespace Aquarium.GA.Population
{
    public class PopulationMember : IEnvMember
    {
        public BodyGenome Genome { get; private set; }

        public Organism Organism { get; private set; }

        public Vector3 Position { get { return Organism.Position; } }

        
        public PopulationMember(BodyGenome genome, Organism specimen)
        {
            Genome = genome;
            Organism = specimen;
        }

        public virtual double Score
        {
            get
            {
                var b = Organism.Body;
                var g = Genome;
                int numOrgans = 0;
                var numConnected = 0;
                b.Parts.ForEach(p =>
                {
                    if (p.ChanneledSignal.NumRegistrations > 0)
                    {
                        numConnected += p.ChanneledSignal.NumRegistrations - 1;
                    }
                    numOrgans += p.Organs.Count();
                });

                var numParts = b.Parts.Count();
                return (numParts * 1000) + (numConnected * 900) + (numOrgans / 10) - (g.Size / 1000);
            }
        }



        public PopulationMember Member
        {
            get { return this; }
        }

        public void EnterEnvPartition(ISurroundings surroundings)
        {
            Organism.Surroundings = surroundings;
        }

        public void ExitEnvPartition(ISurroundings surroundings)
        {
            Organism.Surroundings = null;
        }
    }
}
