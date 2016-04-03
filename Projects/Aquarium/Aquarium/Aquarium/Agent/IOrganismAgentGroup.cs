using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Aquarium.Agent
{
    public interface IOrganismAgentGroup
    {
        ICollection<OrganismAgent> OrganismAgents { get; }

        void Birth(OrganismAgent agent);
        void Death(OrganismAgent agent);

        BoundingBox Box { get; set; }

        int TotalAssigned { get; set; }
    }
}
