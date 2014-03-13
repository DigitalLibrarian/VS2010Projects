using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Aquarium.GA.Population;
using Forever.SpacePartitions;

namespace Aquarium.GA.Environments
{

    public interface IEnvMember
    {
        PopulationMember Member { get; }
        Vector3 Position { get; }

        void EnterEnvPartition(ISurroundings surroundings);
        void ExitEnvPartition(ISurroundings surroundings);
    }
   


}
