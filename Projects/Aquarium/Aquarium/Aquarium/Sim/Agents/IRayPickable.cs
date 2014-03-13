using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Aquarium.Sim.Agents;

namespace Aquarium.Sim.Agents
{
    public interface IRayPickable : IAgent
    {
        bool IsHit(Ray ray);
    }
}
