using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.GA.Genomes
{
    public class GenomeSplicer
    {
        Random Random { get; set; }

        List<Action<BodyGenome>> Mutators { get; set; }

        public GenomeSplicer()
        {
            Random = new Random();
            Mutators = new List<Action<BodyGenome>>();

            RegisterDefaultMutators();
        }

        private void RegisterDefaultMutators()
        {
            var mutators = new List<Action<BodyGenome>>
            {
                (g) => {
                    var geneIndex = Random.Next(g.Genes.Count());
                    var sourceGene = g.Genes[geneIndex];
                    var newGene = new Gene<int>
                    {
                        Name = sourceGene.Name + 1,
                        Value = sourceGene.Value
                    };
                    SlideAfter(g.Genes, geneIndex + 1, 1);
                    g.Genes.Insert(geneIndex + 1, newGene);
                },
                (g) => {

                    var gene = Random.NextElement(g.Genes);
                    int b = 10;
                    int place = Random.Next(5);
                    int sign = Random.Next(1) == 0 ? 1 : -1;
                    var mutation = (int)(Math.Pow(b, place) * sign);
                    gene.Value = gene.Value + mutation;
                },
                (g) => {
                    var geneIndex = Random.Next(g.Genes.Count());
                    var gene = g.Genes[geneIndex];
                    int sign = 1;
                    if (Random.Next(1) == 0) sign = -1;
                    gene.Value += sign;
                },
                (g) => {
                    var one = Random.NextElement(g.Genes);
                    var two = Random.NextElement(g.Genes);

                    var target = Random.Next(g.Size);
                    for (int k = 0; k < Random.Next(3); k++)
                    {
                        g.Genes.Add( new Gene<int>
                        {
                            Name = g.Genes.Last().Name + 1,
                            Value = two.Value
                        });
                    }
                },
                (g) => {
                    int num = g.Size / 10;
                    int start = Random.Next(g.Size - (num ));
                    int target = Random.Next(g.Size - (num ));

                    var section = g.Genes.GetRange(start, num);
                    var newSection = section.Select(gene => new Gene<int> { Name = gene.Name, Value = gene.Value});


                    SlideAfter(g.Genes, target, num);

                    g.Genes.InsertRange(target, newSection);
                },
                (g) => {
                    var geneIndex = Random.Next(g.Genes.Count());
                    var gene = g.Genes[geneIndex];
                    g.Genes.Remove(gene);
                }
                
            };

            Mutators.AddRange(mutators);
        }

            public BodyGenome[] Meiosis(BodyGenome parent1Gen, BodyGenome parent2Gen, int wiggleSize = 8)
        { 
            int minCount = Math.Min(parent1Gen.Size, parent2Gen.Size);
            int wiggle = Random.Next(minCount/wiggleSize);
            int snip = (-wiggle + Random.Next(wiggle*2)) + (minCount / 2);


            var parent1Prefix = parent1Gen.Genes.Take(snip);
            var parent1Suffix = parent1Gen.Genes.Skip(snip);
            var parent2Prefix = parent2Gen.Genes.Take(snip);
            var parent2Suffix = parent2Gen.Genes.Skip(snip);


            var offspring1Genes = Clone(parent1Prefix.Concat(parent2Suffix));
            var offspring2Genes = Clone(parent2Prefix.Concat(parent1Suffix));

            return new[] { offspring1Genes, offspring2Genes };
        }


        public BodyGenome Clone(BodyGenome g)
        {
            return Clone(g.Genes);
        }
        public BodyGenome Clone(IEnumerable<Gene<int>> genes)
        {
            var cloned = genes.Select(g => new Gene<int> { Name = g.Name, Value = g.Value }).ToList();
            return new BodyGenome(cloned);
        }

        public void SlideAfter(IEnumerable<Gene<int>> genes, int geneIndex, int amount)
        {
            foreach (var gene in genes.Skip(geneIndex))
            {
                gene.Name = gene.Name + amount;
            }
        }

        public void Mutate(BodyGenome g)
        {
            Random.NextElement(Mutators)(g);
        }
    }
}
