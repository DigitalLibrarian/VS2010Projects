using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.Life.Spec
{
    public class OrganismSpec
    {
        public Group<BodyPartSpec> BodyParts { get; set; }
        public Group<OrganSpec> Organs { get; set; }
        public Group<NeuralNetworkSpec> NeuralNetworks { get; set; }

    }
}
