using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.Life.Bodies;
using Aquarium.Life.Signals;
using Microsoft.Xna.Framework;

namespace Aquarium.Life.Organs.OrganAbilities
{
    public class QueryPositionAbility : OrganAbility
    {
        public QueryPositionAbility(int param0)
            : base(param0)
        { }

        public override int NumInputs { get { return 1; }}
        public override int NumOutputs { get { return 3; } }

        public override Signal Fire(NervousSystem nervousSystem, Organ parent, Signal signal, MutableForceGenerator fg)
        {
            Vector3 vector = Vector3.Zero;
            if (signal.Value[0] > 0.5f)
            {
                vector = nervousSystem.Organism.Body.Position;
                vector = vector.Round(3);

            }

            return new Signal(SignalEncoding.Encode(vector));
        }
    }
}
