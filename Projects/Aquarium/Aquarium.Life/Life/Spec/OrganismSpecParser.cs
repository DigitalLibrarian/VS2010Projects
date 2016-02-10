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
            MaxBodyParts = 10;
            MinOrgans = 1;
            MaxOrgans = 10;
            MinNeuralNetworks = 1;
            MaxNeuralNetworks = 10;
        }

        public OrganismSpec ReadOrganismSpec(BodyGenome genome)
        {
            IEnumerator<int> genes = genome.Data.Circular<int>().GetEnumerator();
            return new OrganismSpec
            {
                BodyParts = ParseBodyPartSpecGroup(genes),
                Organs = ParseOrganSpecGroup(genes),
                NeuralNetworks = ParseNeuralNetworkSpecGroup(genes)
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
