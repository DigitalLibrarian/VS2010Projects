using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using Aquarium.Agent;

namespace Aquarium.Ui.Targets
{
    public interface ITarget
    {
        string Label { get; }
        IAgent Agent { get; }
        BoundingBox TargetBB { get; }
    }

}
