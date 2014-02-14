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
            var geneIndex = random.Next(Genes.Count());
            var gene = Genes[geneIndex];
            var mutators = new List<Action>
            {
                () => {
                    var newGene = new Gene<int>
                    {
                        Name = gene.Name + 1,
                        Value = gene.Value
                    };

                    foreach (var g in Genes.Skip(geneIndex + 1))
                    {
                        g.Name = g.Name + 1;
                    }
                    Genes.Insert(geneIndex + 1, newGene);
                },
                () => {
                    int sign = 1;
                    if (random.Next(1) == 0) sign = -1;
                    gene.Value += sign;
                },
                () => {
                    var one = random.NextElement(Genes);
                    var two = random.NextElement(Genes);

                    var target = random.Next(Size);
                    for (int k = 0; k < random.Next(3); k++)
                    {
                        Genes.Add( new Gene<int>
                        {
                            Name = Genes.Last().Name + 1,
                            Value = two.Value
                        });
                    }
                },
                () => {
                    int num = Size / 10;
                    int start = random.Next(Size - (num ));
                    int target = random.Next(Size - (num ));

                    var section = Genes.GetRange(start, num);
                    var newSection = section.Select(g => new Gene<int> { Name = g.Name, Value = g.Value});

                    foreach (var g in Genes.Skip(target))
                    {
                        g.Name = g.Name + num;
                    }

                    Genes.InsertRange(target, newSection);
                },
                () => {
                    Genes.Remove(gene);
                }
                
            };


            random.NextElement(mutators)();

        }
    }

}
