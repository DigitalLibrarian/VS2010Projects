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
            int chance = 10; 
            var gene = random.NextElement(Genes);
            if (random.Next(chance) == 0) // one in X
            {
                gene.Name = Genes.Count() + (-2) + random.Next(4);
                    
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

                var target = random.Next(Size);
                for (int k = 0; k < random.Next(3); k++)
                {
                    Genes.Add( new Gene<int>
                    {
                        Name = one.Name + two.Name + k,
                        Value = two.Value
                    });
                }
            }

                
            if (random.Next(chance) == 0)
            {
                int num = Math.Min(Size / 10, 20);
                int start = random.Next(Size - num);
                int target = random.Next(Size - num);

                var section = Genes.GetRange(start, num);
                Genes.InsertRange(target, section.Select(g => new Gene<int> { Name = g.Name + target, Value = g.Value}));

            
            }
                
            if(random.Next(chance * chance) == 0)
            {
                Genes.Remove(gene);
            }
        }
    }

}
