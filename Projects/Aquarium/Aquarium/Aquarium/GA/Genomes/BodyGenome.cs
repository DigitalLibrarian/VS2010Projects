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
            Genes.ForEach(gene => gene.Value = Math.Round(gene.Value, 9));

            int chance = 20; // one in X
            int numAffected = random.Next(1 + Genes.Count() / 3);

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
                    gene.Value += r;   
                }

                if (random.Next(chance) == 0)
                {
                    var one = random.NextElement(Genes);
                    var two = random.NextElement(Genes);

                    Genes.Add(new Gene<double>
                    {
                        Name = one.Name + two.Name,
                        Value = one.Value
                    });
                }
            }       
        }
    }

}
