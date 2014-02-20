using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Genomes;
using Aquarium.GA.Phenotypes;
using Aquarium.GA.Codons;
using Microsoft.Xna.Framework;

namespace Aquarium.GA.Population
{
    public class Population
    {
        public int MaxPop { get; private set; }
        public int GenomeSizeCap { get; private set; }
        protected GenomeSplicer Splicer { get; private set; }

        private List<PopulationMember> Registry { get; set; }

        public int Size { get { return Registry.Count(); } }

        public Population(int maxPop, int genomeSizeCap)
        {
            MaxPop = maxPop;
            GenomeSizeCap = genomeSizeCap;
            Splicer = new GenomeSplicer();

            Registry = new List<PopulationMember>();
        }

        public virtual bool Register(PopulationMember mem)
        {
            var genome = mem.Genome;
            var body = mem.Specimen.Body;
            if (genome.Size > GenomeSizeCap) return false;
            if (!body.Parts.Any()) return false;

            Registry.Add(mem);

            return true;
        }

        protected GenomeTemplate<int> TemplateFor(BodyGenome g)
        {
            return new ZeroIntGenomeTemplate();
        }

        protected Organism SpawnFromGenome(BodyGenome g)
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


        public virtual List<PopulationMember> LocalMembers(Vector3 query, float radius = 1f)
        {
            //TODO - only retun subset near query point and radius

            return Registry;
        }
    }
}
