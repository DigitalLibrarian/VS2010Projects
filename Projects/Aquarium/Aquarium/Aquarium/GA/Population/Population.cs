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
        public int MinPop { get; private set; }
        public int MaxPop { get; private set; }
        public int GenomeSizeCap { get; private set; }
        public  GenomeSplicer Splicer { get; private set; }

        private List<PopulationMember> Members { get; set; }

        public int Size { get { return Members.Count(); } }

        public bool NeedsSpawn
        {
            get { return Size < MinPop; }
        }

        public delegate void OnAddEventHandler(PopulationMember mem);
        public event OnAddEventHandler OnAdd;
        public delegate void OnRemoveEventHandler(PopulationMember mem);
        public event OnRemoveEventHandler OnRemove;

        public Population(int minPop,  int maxPop, int genomeSizeCap)
        {
            Members = new List<PopulationMember>();
            MinPop = minPop;
            MaxPop = maxPop;
            GenomeSizeCap = genomeSizeCap;
            Splicer = new GenomeSplicer();


        }

        public virtual bool Register(PopulationMember mem)
        {
            var genome = mem.Genome;
            var body = mem.Organism.Body;
            if (genome.Size > GenomeSizeCap) return false;
            if (!body.Parts.Any()) return false;
            
            if(OnAdd != null) OnAdd.Invoke(mem);

            Members.Add(mem);

            return true;
        }

        public virtual void UnRegister(PopulationMember mem)
        {

            if (OnRemove != null) OnRemove.Invoke(mem);
            Members.Remove(mem);
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




        public List<PopulationMember> ToList()
        {
            return Members;
        }
    }
}
