using System;
using System.Collections.Generic;
using System.Text;


using Microsoft.Xna.Framework;

using Forever.Physics;

namespace Forever.Physics
{

  public interface IForceGenerator
  {
    void updateForce(IPhysicsObject p, float duration);
  }
}
