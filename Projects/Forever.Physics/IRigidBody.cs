using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace Forever.Physics
{


  public interface IRigidBody : IPhysicsObject
  {
      Matrix InertiaTensor { get; set; }
      Matrix InertiaTensorWorld { get; }
      Matrix InverseInertiaTensor { get; set; }
      Matrix InverseInertiaTensorWorld { get; }
      Matrix World { get; }

      Quaternion Orientation { get; set; }

      Vector3 AngularMomentum { get; }

      Vector3 LinearMomentum { get; }


      float Mass { get; set; }
      float InverseMass { get; set; }
      bool HasFiniteMass { get; }


      float LinearDamping { get; set; }

      float AngularDamping { get; set; }

      Vector3 Rotation { get; }

      Vector3 Position { get; set; }
      Vector3 Velocity { get;  }
      Vector3 Acceler { get; set; }

      Vector3 LastAccel { get; }
      bool Awake { get; set; }

      bool CanSleep { get; set; }

      void addTorque(Vector3 torque);
      new void addForce(Vector3 force, Vector3 point);
      void clearAccumulators();
      new void integrate(float duration);

      void addVelocity(Vector3 velo);
      void addRotation(Vector3 rot);

      void calculateDerivedData();

      Vector3 Up { get; }
      Vector3 Right { get; }
      Vector3 Forward { get; }
  };
}
