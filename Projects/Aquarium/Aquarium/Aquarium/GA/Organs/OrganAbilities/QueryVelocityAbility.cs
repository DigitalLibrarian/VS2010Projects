using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Bodies;
using Aquarium.GA.Signals;

namespace Aquarium.GA.Organs.OrganAbilities
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
            var result = new List<double>{ 0, 0, 0 };
            if (num > 0.5)
            {
                var rigidBody = nervousSystem.Organism.RigidBody;
                result = new List<double> { rigidBody.Velocity.X, rigidBody.Velocity.Y, rigidBody.Velocity.Z };
            }

            return new Signal(result);
        }
    }
}
