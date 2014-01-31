using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Signals;

namespace Aquarium.GA.Organs
{
    abstract class OrganAbility
    {
        abstract public Signal Fire(Organ parent, Signal signal);
    }

}
