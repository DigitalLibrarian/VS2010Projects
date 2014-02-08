using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.GA.Genomes
{
    public class BodyGenome : Genome<int>
    {
        public BodyGenome(List<Gene<int>> genes) : base(genes) { }

        public override void Mutate(Random random)
        {
            int chance = 50; // one in X
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
                    int sign = 1;
                    if (random.Next(1) == 0) sign = -1;
                    gene.Value += sign;
                }


                if (random.Next(chance) == 0)
                {
                    var one = random.NextElement(Genes);
                    var two = random.NextElement(Genes);

                    Genes.Add(new Gene<int>
                    {
                        Name = one.Name,
                        Value = one.Value
                    });
                }

                if (random.Next(chance + chance) == 0)
                {
                    int num = Size / 20;
                    int start = random.Next(Size - num);
                    int target = random.Next(Size - num);

                    var section = Genes.GetRange(start, num);
                    Genes.InsertRange(target, section);

                }
            }       
        }
    }

}
