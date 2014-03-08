using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Forever.Physics;

namespace Aquarium
{
    public class MutableForceGenerator : IForceGenerator
    {
        public Vector3 Force { get; set; }
        public Vector3? Position { get; set; }
        public Vector3 Torque { get; set; }

        public MutableForceGenerator()
        {
            Force = Vector3.Zero;
            Position = null;
            Torque = Vector3.Zero;
        }

        public void updateForce(IPhysicsObject p, float duration)
        {
            if (Position != null)
            {
                p.addForce(Force * duration, (Vector3)Position);
            }
            else
            {
                p.addForce(Force * duration);
            }

            p.addTorque(Torque * duration);
        }
    }
}
