using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Codons;
using Aquarium.GA.Headers;

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


        public static BodyGenome Random(Random random, int numParts, int numOrgans, int numNN, int sizeJunk=0)
        {
            var gContents = new List<Gene<int>>();

            List<int> codonContents;
            int name = 0;

            for (int i = 0; i < numParts; i++)
            {
                for (int j = 0; j < sizeJunk; j++)
                {
                    var v = random.Next();
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
                    var v = random.Next();
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
                    var v = random.Next();
                    gContents.Add(
                            new Gene<int> { Name = name++, Value = v }
                        );
                };

            } // parts

            for (int i = 0; i < numOrgans; i++)
            {
                codonContents = new OrganStartCodon().Example();
                codonContents.ForEach(v => gContents.Add(
                            new Gene<int> { Name = name++, Value = v }
                            ));

                for (int j = 0; j < OrganHeader.Size; j++)
                {
                    var v = random.Next();
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
                    var v = random.Next();
                    gContents.Add(
                            new Gene<int> { Name = name++, Value = v }
                        );
                };
            }


            for (int i = 0; i < numNN; i++)
            {

                // neural network


                codonContents = new NeuralNetworkStartCodon().Example();
                codonContents.ForEach(v => gContents.Add(
                            new Gene<int> { Name = name++, Value = v }
                            ));

                for (int j = 0; j < NeuralNetworkHeader.Size; j++)
                {
                    var v = random.Next();
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
                    var v = random.Next();
                    gContents.Add(
                            new Gene<int> { Name = name++, Value = v }
                        );
                };

            }

            codonContents = new BodyEndCodon().Example();

            codonContents.ForEach(v => gContents.Add(
                        new Gene<int> { Name = name++, Value = v }
                        ));
            return new BodyGenome(gContents);
        }

    }

}
