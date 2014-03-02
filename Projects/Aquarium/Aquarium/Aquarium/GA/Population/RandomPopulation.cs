using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Genomes;
using Aquarium.GA.Codons;
using Aquarium.GA.Headers;
using Aquarium.GA.Phenotypes;
using Aquarium.GA.SpacePartitions;

namespace Aquarium.GA.Population
{
    public class RandomPopulation : Population
    {
        Random Random { get; set; }
        public float SpawnRange { get; set; }

        public RandomPopulation(int minPop, int maxPop, float spawnRange, int genomeSizeCap)
            : base(minPop, maxPop, genomeSizeCap)
        {
            Random = new Random();
            SpawnRange = spawnRange;
            
        }


        public void GenerateUntilSize(int popSize, float spawnRange, int numPartsEach)
        {
            while (Size < popSize)
            {
                var mem = RandomMember(numPartsEach);

                if (mem != null)
                {
                    mem.Specimen.Position = Random.NextVector() * spawnRange;
                    Register(mem);
                }
            }
        }

        public PopulationMember RandomMember(int numParts)
        {
            var g = BodyGenome.Random(Random, numParts, 20, 10, 3);
            var spawn = SpawnFromGenome(g);

            if (spawn == null) return null;
            return new PopulationMember(g, spawn);
        }




      
    }
}
