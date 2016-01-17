using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Aquarium.Sim
{
    public interface IRayPickable
    {
        bool IsHit(Ray ray);
    }
}
