using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.GA.Genomes
{
    public class BodyGenome : Genome<double>
    {
        public BodyGenome(List<Gene<double>> genes) : base(genes) { }

        public override void Mutate(Random random)
        {
            int chance = 20; // one in X
            int numAffected = Genes.Count() / 10;

            for (int i = 0; i < numAffected; i++)
            {
                var gene = random.NextElement(Genes);
                if (random.Next(chance) == 0)
                {
                    gene.Name = Genes.Count() + -i / 2 + random.Next(i);
                    
                }
                if (random.Next(chance) == 0)
                {
                    var r = -0.5f + random.NextDouble();//  (-0.5f, 0.5f) is range
                    // we are going change it by a porportion of it's own dist from zero
                    double offset = gene.Value * r * 0.01f;
                    offset = (gene.Value / 10f) * (float)random.NextDouble();
                    gene.Value = (float)Math.Round(gene.Value + offset, 5);
                }
            }       
        }
    }

}
