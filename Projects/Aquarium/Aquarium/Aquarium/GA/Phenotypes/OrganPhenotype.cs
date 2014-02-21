using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Headers;

namespace Aquarium.GA.Phenotypes
{

    public class OrganPhenotype : ComponentPheno, IOrganPhenotype
    {
        public IInstancePointer OrganType { get; set; }
        public IInstancePointer BodyPartPointer { get; set; }
        public IInstancePointer InputSignal { get; set; }
        public IInstancePointer OutputSignal { get; set; }
        public IInstancePointer AbilityId { get; set; }

        public OrganPhenotype(OrganHeader header)
        {
            OrganType = new InstancePointer(header.OrganType);
            BodyPartPointer = new InstancePointer(header.BodyPart);
            InputSignal = new InstancePointer(header.InputSignal);
            OutputSignal = new InstancePointer(header.OutputSignal);
            AbilityId = new InstancePointer(header.AbilityId);
        }
    }
}
