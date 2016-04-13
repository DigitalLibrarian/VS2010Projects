using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Aquarium.Targeting
{
    public interface ITarget
    {
        string Label { get; }
        BoundingBox TargetBB { get; }
    }

}
