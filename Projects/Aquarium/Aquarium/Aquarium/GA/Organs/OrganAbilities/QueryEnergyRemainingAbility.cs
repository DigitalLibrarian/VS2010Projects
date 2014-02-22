using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Signals;
using Aquarium.GA.Bodies;

namespace Aquarium.GA.Organs.OrganAbilities
{
    public class QueryEnergyRemainingAbility : OrganAbility
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
            if (signal.Value[0] > 0.5f)
            {
                var p = nervousSystem.Organism.Body.Position;
                p = p.Round(3);
                var ratio = nervousSystem.Organism.Energy / nervousSystem.Organism.MaxEnergy;
                return new Signal(new List<double> { ratio });
            }

            return new Signal(new List<double> { 0 } );
        }
    }
}
