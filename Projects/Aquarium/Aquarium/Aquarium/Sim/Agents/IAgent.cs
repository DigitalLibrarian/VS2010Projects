using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.Render;

namespace Aquarium.Sim.Agents
{
    public interface IAgent
    {
        void Draw(float duration, RenderContext renderContext);
        void Update(float duration);

    }
}
