using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Forever.Physics;

using Forever.Extensions;
namespace Aquarium.Life.Genomes
{
    public class GenomeSplicer
    {

        List<Action<BodyGenome>> Mutators { get; set; }
        Random Random { get; set; }

        public GenomeSplicer()
        {
            Random = new Random();
            Mutators = new List<Action<BodyGenome>>();

            RegisterDefaultMutators();
        }

        public void Mutate(BodyGenome off)
        {
            Random.NextElement(Mutators)(off);
            /*
            var index = Random.NextIndex(off.Data);
            var wiggle = 2;
            off.Data[index] += Random.Next(wiggle) - (wiggle/2);
             */
        }

        public IEnumerable<BodyGenome> Meiosis(BodyGenome parent1Gen, BodyGenome parent2Gen, int wiggleSize = 5)
        { 
            int minCount = Math.Min(parent1Gen.Size, parent2Gen.Size);
            int wiggle = Random.Next(minCount/wiggleSize);
            int snip = (-wiggle + Random.Next(wiggle*2)) + (minCount / 2);


            var parent1Prefix = parent1Gen.Data.Take(snip);
            var parent1Suffix = parent1Gen.Data.Skip(snip);
            var parent2Prefix = parent2Gen.Data.Take(snip);
            var parent2Suffix = parent2Gen.Data.Skip(snip);


            var offspring1Genes = Clone(parent1Prefix.Concat(parent2Suffix));
            var offspring2Genes = Clone(parent2Prefix.Concat(parent1Suffix));

            return new[] { offspring1Genes, offspring2Genes };
        }

        BodyGenome Clone(IEnumerable<int> g)
        {
            return new BodyGenome(g.Select(x => x).ToList());
        }

        private void RegisterDefaultMutators()
        {
            var mutators = new List<Action<BodyGenome>>
            {
                (g) => {

                    var geneIndex = Random.NextIndex(g.Data);
                    var gene = g.Data[geneIndex];
                    int b = 10;
                    int place = Random.Next(5);
                    int sign = Random.Next(1) == 0 ? 1 : -1;
                    var mutation = (int)(Math.Pow(b, place) * sign);
                    g.Data[geneIndex] = gene + mutation;
                },
                (g) => {
                    var geneIndex = Random.NextIndex(g.Data);
                    var gene = g.Data[geneIndex];
                    int sign = 1;
                    if (Random.Next(1) == 0) sign = -1;
                    g.Data[geneIndex] += sign;
                },
                (g) => {
                    var one = Random.NextElement(g.Data);
                    g.Data.Add(one);
                },
                (g) => {
                    var geneIndex = Random.NextIndex(g.Data);
                    g.Data.RemoveAt(geneIndex);
                }
                
            };

            Mutators.AddRange(mutators);
        }
    }


}
