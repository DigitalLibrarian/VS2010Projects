using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.Agent
{
    public interface IOrganismAgentGroup
    {
        ICollection<OrganismAgent> OrganismAgents { get; }

        void Birth(OrganismAgent agent);
        void Death(OrganismAgent agent);
    }
}
