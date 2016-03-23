using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using Forever.Render;

namespace Aquarium.Sim
{
    public interface ISimObject
    {
        void Draw(float duration, RenderContext renderContext);
        void Update(float duration);

        Vector3 Position { get; }
    }
}
