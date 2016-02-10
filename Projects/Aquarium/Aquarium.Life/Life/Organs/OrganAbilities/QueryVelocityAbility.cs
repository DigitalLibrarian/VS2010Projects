using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.Life.Bodies;
using Aquarium.Life.Signals;
using Microsoft.Xna.Framework;

namespace Aquarium.Life.Organs.OrganAbilities
{
    public class QueryVelocityAbility : OrganAbility
    {
        public QueryVelocityAbility(int param0)
            : base(param0)
        {

        }

        public override int NumInputs
        {
            get { return 1; }
        }

        public override int NumOutputs
        {
            get { return 3; }
        }

        public override Signal Fire(NervousSystem nervousSystem, Organ parent, Signal signal, MutableForceGenerator fg)
        {
            var num = signal.Value[0];
            Vector3 vector = Vector3.Zero;
            if (num > 0.5)
            {
                nervousSystem.Organism.LifeForce.PayEnergyCost(LifeForce.Data.SensorFiringCost);
                var rigidBody = nervousSystem.Organism.RigidBody;
                vector = rigidBody.Velocity;
            }

            return new Signal(SignalEncoding.Encode(vector));
        }
    }
}
