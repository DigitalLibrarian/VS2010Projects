using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Aquarium.UI.Steering
{
    public interface ISteering
    {
        Vector3 Force { get; }
        Vector3 Torque { get; }
    }
}
