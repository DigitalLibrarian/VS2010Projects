using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.Life;
using Aquarium.Sim.Agents;

namespace Aquarium
{
    public interface IMatingManager
    {

        bool TryMate(OrganismAgent me, OrganismAgent other);

    }
}
