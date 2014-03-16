using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.Life.Signals;
using Aquarium.Life.Bodies;
using Aquarium.Life.Organs.OrganAbilities;

namespace Aquarium.Life.Organs
{
    public abstract class OrganAbility
    {
        public abstract int NumInputs { get; }
        public abstract int NumOutputs { get; }
        public abstract Signal Fire(NervousSystem nervousSystem, Organ parent, Signal signal, MutableForceGenerator fg);


        public OrganAbility(int abilityParam0)
        {
        }
    }
}
