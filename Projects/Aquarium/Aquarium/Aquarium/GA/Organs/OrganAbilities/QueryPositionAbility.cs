using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Bodies;
using Aquarium.GA.Signals;

namespace Aquarium.GA.Organs.OrganAbilities
{
    public class QueryPositionAbility : OrganAbility
    {
        public QueryPositionAbility(int param0)
            : base(param0)
        { }

        public override int NumInputs { get { return 1; }}
        public override int NumOutputs { get { return 3; } }
        
        public override Signal Fire(NervousSystem nervousSystem, Organ parent, Signal signal)
        {
            if (signal.Value[0] > 0.5f)
            {
                var p = nervousSystem.Organism.Body.Position;
                p = p.Round(3);

                return new Signal(new List<double> { p.X, p.Y, p.Z });
            }

            return new Signal(new List<double> { 0f, 0f, 0f });
        }
    }
}
