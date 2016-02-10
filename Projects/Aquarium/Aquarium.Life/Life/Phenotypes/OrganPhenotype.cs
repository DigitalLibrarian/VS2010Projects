using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.Life.Spec;

namespace Aquarium.Life.Phenotypes
{

    public class OrganPhenotype : ComponentPheno, IOrganPhenotype
    {
        public IInstancePointer OrganType { get; set; }
        public IInstancePointer BodyPartPointer { get; set; }
        public IInstancePointer InputSignal { get; set; }
        public IInstancePointer OutputSignal { get; set; }
        public IInstancePointer ForeignId { get; set; }

        public IInstancePointer AbilityParam0 { get; set; }
        public IInstancePointer AbilityParam1 { get; set; }

        public OrganPhenotype(OrganSpec header)
        {
            OrganType = new InstancePointer(header.OrganType);
            BodyPartPointer = new InstancePointer(header.BodyPartId);
            InputSignal = new InstancePointer(header.InputSignal);
            OutputSignal = new InstancePointer(header.OutputSignal);
            ForeignId = new InstancePointer(header.AbilityId);
            AbilityParam0 = new InstancePointer(header.AbilityParam0);
            AbilityParam1 = new InstancePointer(header.AbilityParam1);
        }

    }
}
