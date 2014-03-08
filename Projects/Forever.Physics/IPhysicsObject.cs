using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Forever.Physics
{
    public interface IPhysicsObject
    {
        bool Awake { get; }
        void addForce(Vector3 force);
        void addTorque(Vector3 torque);
        void addForce(Vector3 force, Vector3 point);
        void integrate(float duration);

        float Mass { get; }

        Vector3 CenterOfMass { get; }
        Vector3 Velocity { get; }
        Vector3 Rotation { get; }

        void Translate(Vector3 translation);
    }
}
