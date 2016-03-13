using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.Life.Spec;
using Aquarium.Life.Genomes;

namespace Aquarium.Life.Spec
{
    public class OrganismSpecParser
    {
        public int MinBodyParts { get; set; }
        public int MaxBodyParts { get; set; }
        public int MinOrgans { get; set; }
        public int MaxOrgans { get; set; }
        public int MinNeuralNetworks { get; set; }
        public int MaxNeuralNetworks { get; set; }

        public OrganismSpecParser()
        {
            MinBodyParts = 1;
            MaxBodyParts = 25;
            MinOrgans = 1;
            MaxOrgans = 25;
            MinNeuralNetworks = 1;
            MaxNeuralNetworks = 25;
        }

        public OrganismSpec ReadOrganismSpec(BodyGenome genome)
        {
            IEnumerator<int> genes = genome.Data.Circular<int>().GetEnumerator();

            var bodyParts = new Group<BodyPartSpec>(MinBodyParts, MaxBodyParts, BodyPartSpec.FromGenome);
            var organs = new Group<OrganSpec>(MinOrgans, MaxOrgans, OrganSpec.FromGenome);
            var nns = new Group<NeuralNetworkSpec>(MinNeuralNetworks, MaxNeuralNetworks, NeuralNetworkSpec.FromGenome);

            bool done = false;
            while (!done)
            {
                var totalBodyParts = bodyParts.Index.Collection.Count;
                var totalOrgans = organs.Index.Collection.Count;
                var totalNNs = nns.Index.Collection.Count;

                var parsers = new List<Action<IEnumerator<int>>>();

                if (totalBodyParts < MinBodyParts || totalBodyParts < MaxBodyParts)
                {
                    parsers.Add((g) => bodyParts.Add((BodyPartSpec)BodyPartSpec.FromGenome(g)));
                }

                if (totalOrgans < MinOrgans || totalOrgans < MaxOrgans)
                {
                    parsers.Add((g) => organs.Add(OrganSpec.FromGenome(g)));
                }

                if(totalNNs < MinNeuralNetworks || totalNNs < MaxNeuralNetworks)
                {
                    parsers.Add((g) => nns.Add(NeuralNetworkSpec.FromGenome(g)));
                }

                if (!parsers.Any())
                {
                    break;
                }

                genes.MoveNext();
                var parserIndex = genes.Current;
                var parser = Fuzzy.CircleIndex(parsers, parserIndex);

                parser(genes);

            }

            return new OrganismSpec
            {
                BodyParts = bodyParts,//ParseBodyPartSpecGroup(genes),
                Organs = organs, //ParseOrganSpecGroup(genes),
                NeuralNetworks = nns//ParseNeuralNetworkSpecGroup(genes)
            };
        }

        Group<BodyPartSpec> ParseBodyPartSpecGroup(IEnumerator<int> genome)
        {
            var nodeGroup = new Group<BodyPartSpec>(MinBodyParts, MaxBodyParts, BodyPartSpec.FromGenome);
            nodeGroup.Read(genome);
            return nodeGroup;
        }

        Group<OrganSpec> ParseOrganSpecGroup(IEnumerator<int> genome)
        {
            var nodeGroup = new Group<OrganSpec>(MinOrgans, MaxOrgans, OrganSpec.FromGenome);
            nodeGroup.Read(genome);
            return nodeGroup;
        }

        Group<NeuralNetworkSpec> ParseNeuralNetworkSpecGroup(IEnumerator<int> genome)
        {
            var nodeGroup = new Group<NeuralNetworkSpec>(MinNeuralNetworks, MaxNeuralNetworks, NeuralNetworkSpec.FromGenome);
            nodeGroup.Read(genome);
            return nodeGroup;
        }
    }
}
