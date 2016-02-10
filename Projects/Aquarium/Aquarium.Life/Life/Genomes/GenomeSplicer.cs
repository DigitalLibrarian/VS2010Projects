using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Forever.Physics;

namespace Aquarium.Life.Genomes
{
    public class GenomeSplicer
    {
        Random Random = new Random();
        public void Mutate(BodyGenome off)
        {
            var index = Random.NextIndex(off.Data);
            var wiggle = 100;
            off.Data[index] += Random.Next(wiggle) - (wiggle/2);
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
    }
}
