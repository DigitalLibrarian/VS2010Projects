using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Genomes;
using Aquarium.GA.Phenotypes;
using Aquarium.GA.Codons;
using Microsoft.Xna.Framework;
using Aquarium.GA.SpacePartitions;

namespace Aquarium.GA.Population
{
    public class Population
    {
        public int MaxPop { get; private set; }
        public int GenomeSizeCap { get; private set; }
        protected GenomeSplicer Splicer { get; private set; }

        public Space<PopulationMember> Space { get; private set; }

        public int Size { get { return Space.Count; } }

        public Population(int maxPop, int genomeSizeCap)
        {
            Space = new Space<PopulationMember>();
            MaxPop = maxPop;
            GenomeSizeCap = genomeSizeCap;
            Splicer = new GenomeSplicer();

        }

        public virtual bool Register(PopulationMember mem)
        {
            var genome = mem.Genome;
            var body = mem.Specimen.Body;
            if (genome.Size > GenomeSizeCap) return false;
            if (!body.Parts.Any()) return false;

            Space.Register(mem, mem.Position);

            return true;
        }

        public static GenomeTemplate<int> TemplateFor(BodyGenome g)
        {
            return new ZeroIntGenomeTemplate();
        }

        public static Organism SpawnFromGenome(BodyGenome g)
        {
            PhenotypeReader gR = new PhenotypeReader();

            var t = TemplateFor(g);
            var parser = new BodyCodonParser();

            var pheno = parser.ParseBodyPhenotype(g, t);

            if (pheno != null)
            {
                var body = gR.ProduceBody(pheno);
                if (body != null)
                {
                    return new Organism(body);
                }
            }

            return null;
        }


        public virtual IEnumerable<PopulationMember> LocalMembers(Vector3 query, float radius = 100f)
        {
            return Space.Query((coord, mem) => Vector3.Distance(query, mem.Position) < radius);
        }

    }
}
