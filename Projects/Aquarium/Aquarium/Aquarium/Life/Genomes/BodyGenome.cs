using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.Life.Codons;
using Aquarium.Life.Headers;

namespace Aquarium.Life.Genomes
{
    public class BodyGenome : Genome<int>
    {
        public BodyGenome(List<Gene<int>> genes) : base(genes) { }

        
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
