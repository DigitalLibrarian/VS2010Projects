using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Headers;

namespace Aquarium.GA.Phenotypes
{

    public class OrganPhenotype : ComponentPheno, IOrganPhenotype
    {
        public IInstancePointer BodyPartPointer { get; set; }
        public IInstancePointer InputSignal { get; set; }
        public IInstancePointer OutputSignal { get; set; }

        public OrganPhenotype(OrganHeader header)
        {
            BodyPartPointer = new InstancePointer(header.BodyPart);
            InputSignal = new InstancePointer(header.InputSignal);
            OutputSignal = new InstancePointer(header.OutputSignal);
        }
    }

    public class OrganAbilityPhenotype : ComponentPheno, IOrganAbilityPhenotype
    {
        int AbilityId { get; set; }
    }
}
