using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Genomes;
using Microsoft.Xna.Framework;

namespace Aquarium.GA.Population
{
    public class PopulationMember
    {
        public BodyGenome Genome { get; private set; }

        public Organism Specimen { get; private set; }

        public Vector3 Position { get { return Specimen.Position; } }

        public PopulationMember(BodyGenome genome, Organism specimen)
        {
            Genome = genome;
            Specimen = specimen;
        }

        public virtual double Score
        {
            get
            {
                var b = Specimen.Body;
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


    }
}
