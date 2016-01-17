using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.Life.Signals;
using Aquarium.Life.Bodies;

using Forever.Physics;

namespace Aquarium.Life.Organs.OrganAbilities
{
    public class QueryEnergyRemainingAbility : OrganAbility
    {
        public QueryEnergyRemainingAbility(int param0)
            : base(param0)
        { }

        public override int NumInputs
        {
            get { return 1; }
        }

        public override int NumOutputs
        {
            get { return 1; }
        }

        public override Signal Fire(NervousSystem nervousSystem, Organ parent, Signal signal, MutableForceGenerator fg)
        {
            float ratio = 0f;
            if (signal.Value[0] > 0.5f)
            {
                nervousSystem.Organism.LifeForce.PayEnergyCost(LifeForce.SensorFiringCost);
                var p = nervousSystem.Organism.Body.Position;
                p = p.Round(3);
                ratio = nervousSystem.Organism.Energy / nervousSystem.Organism.MaxEnergy;
            }

            return new Signal(SignalEncoding.Encode(ratio));
        }
    }
}
