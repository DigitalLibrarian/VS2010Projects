using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Genomes;
using Aquarium.GA.Codons;
using Aquarium.GA.Headers;
using Aquarium.GA.Phenotypes;

namespace Aquarium.GA.Population
{
    public class RandomPopulation : Population
    {
        Random Random { get; set; }

        public RandomPopulation(int maxPop, int genomeSizeCap)
            : base(maxPop, genomeSizeCap)
        {
            Random = new Random();
            
            GenerateInitial(maxPop);
        }

        protected void GenerateInitial(int popSize, int spawnRange = 100, int numPartsEach = 3)
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

        public  PopulationMember RandomMember(int numParts)
        {
            var gContents = new List<Gene<int>>();
            
            List<int> codonContents;
            int name = 0;
            int sizeJunk = 2;

            for (int i = 0; i < numParts; i++)
            {
                for (int j = 0; j < sizeJunk; j++)
                {
                    var v = Random.Next();
                    gContents.Add(
                            new Gene<int> { Name = name++, Value = v }
                        );
                };

                //define a body part

                codonContents = new BodyPartStartCodon().Example();
                codonContents.ForEach(v => gContents.Add(
                            new Gene<int> { Name = name++, Value = v }
                            ));

                for (int j = 0; j < BodyPartHeader.Size; j++)
                {
                    var v = Random.Next();
                    gContents.Add(
                            new Gene<int> { Name = name++, Value = v }
                        );
                };


                codonContents = new BodyPartEndCodon().Example();
                codonContents.ForEach(v => gContents.Add(
                            new Gene<int> { Name = name++, Value = v }
                            ));

                for (int j = 0; j < sizeJunk; j++)
                {
                    var v = Random.Next();
                    gContents.Add(
                            new Gene<int> { Name = name++, Value = v }
                        );
                };

            } // parts

            for (int i = 0; i < numParts*2; i++)
            {
                codonContents = new OrganStartCodon().Example();
                codonContents.ForEach(v => gContents.Add(
                            new Gene<int> { Name = name++, Value = v }
                            ));

                for (int j = 0; j < OrganHeader.Size; j++)
                {
                    var v = Random.Next();
                    gContents.Add(
                            new Gene<int> { Name = name++, Value = v }
                        );
                };


                codonContents = new OrganEndCodon().Example();
                codonContents.ForEach(v => gContents.Add(
                            new Gene<int> { Name = name++, Value = v }
                            ));


                for (int j = 0; j < sizeJunk; j++)
                {
                    var v = Random.Next();
                    gContents.Add(
                            new Gene<int> { Name = name++, Value = v }
                        );
                };
            }


            for (int i = 0; i < numParts*2; i++)
            {

                // neural network


                codonContents = new NeuralNetworkStartCodon().Example();
                codonContents.ForEach(v => gContents.Add(
                            new Gene<int> { Name = name++, Value = v }
                            ));

                for (int j = 0; j < NeuralNetworkHeader.Size; j++)
                {
                    var v = Random.Next();
                    gContents.Add(
                            new Gene<int> { Name = name++, Value = v }
                        );
                };

                codonContents = new NeuralNetworkEndCodon().Example();
                codonContents.ForEach(v => gContents.Add(
                            new Gene<int> { Name = name++, Value = v }
                            ));


                for (int j = 0; j < sizeJunk; j++)
                {
                    var v = Random.Next();
                    gContents.Add(
                            new Gene<int> { Name = name++, Value = v }
                        );
                };

            }

            codonContents = new BodyEndCodon().Example();

            codonContents.ForEach(v => gContents.Add(
                        new Gene<int> { Name = name++, Value = v }
                        ));
            var g =  new BodyGenome(gContents);
            var spawn = SpawnFromGenome(g);

            if (spawn == null) return null;
            return new PopulationMember(g, spawn);
        }

      
    }
}
