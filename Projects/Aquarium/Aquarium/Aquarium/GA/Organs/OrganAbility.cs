using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Signals;

namespace Aquarium.GA.Organs
{
    public abstract class OrganAbility
    {
        public abstract int NumInputs { get; }
        public abstract int NumOutputs { get; }
        public abstract Signal Fire(Organ parent, Signal signal);
    }

}
