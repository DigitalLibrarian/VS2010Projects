using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Signals;
using Aquarium.GA.Bodies;
using Microsoft.Xna.Framework;

namespace Aquarium.GA.Organs.OrganAbilities
{
    public class ThrusterAbility : OrganAbility
    {

        public override int NumInputs
        {
            get { return 1; }
        }

        public override int NumOutputs
        {
            get { return 1; }
        }

        public override Signal Fire(NervousSystem nervousSystem, Organ parent, Signal signal)
        {
            var num = signal.Value[0];
            var result = 0;
            if (num > 0.5)
            {
                var rigidBody = nervousSystem.Organism.RigidBody;

                var part = parent.Part;
                var dir = part.LocalPosition;

                if (dir.LengthSquared() == 0)
                {
                    dir = Vector3.Zero;
                    result = 0;
                }
                else
                {
                    dir.Normalize();
                    var mag = 0.00001f * nervousSystem.Organism.RigidBody.Mass;
                    var veloCap = 0.001f;

                    if (rigidBody.Velocity.Length() < veloCap)
                    {
                        rigidBody.addForce(dir * mag);
                        result = 1;
                    }
                }

            }

            return new Signal(new List<double> { result });
        }
    }
}
