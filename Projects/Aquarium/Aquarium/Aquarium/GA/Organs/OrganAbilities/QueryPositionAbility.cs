using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Bodies;

namespace Aquarium.GA.Organs.OrganAbilities
{
    public class QueryPositionAbility : OrganAbility
    {
        public override int NumInputs { get { return 1; }}
        public override int NumOutputs { get { return 3; } }
        Body Body { get; set; }
        public QueryPositionAbility(Body body)
        {
            Body = body;
        }

        public override Signals.Signal Fire(Organ parent, Signals.Signal signal)
        {
            if (signal.Value[0] > 0f)
            {
                var p = Body.Position;
                p = p.Round(3);

                return new Signals.Signal(new List<double> { p.X, p.Y, p.Z });
            }

            return new Signals.Signal(new List<double> { 0f, 0f, 0f });
        }
    }
}
