using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.Life
{
    public class LifeForceData
    {
        public int MaxEnergy { get; set; }
        public float BasalEnergyCost { get; set; }
        public float BodyPartUnitCost { get; set; }
        public float BodyPartVolumeCost { get; set; }
        public float NeuralOrganCost { get; set; }
        public float AbilityOrganCost { get; set; }
        public float TimerOrganCost { get; set; }
        public float OrganUnitCost { get; set; }
        public float AbilityFiringBaseCost { get; set; }
        public float ThrusterFiringCost { get; set; }
        public float SpinnerFiringCost { get; set; }
        public float SensorFiringCost { get; set; }
        public float BiterFiringCost { get; set; }

        public LifeForceData()
        {
            var epsilon = 0.0001f;
            MaxEnergy = 100;
            BasalEnergyCost = epsilon;
            BodyPartUnitCost = epsilon;
            BodyPartVolumeCost = epsilon;
            NeuralOrganCost = epsilon;
            AbilityOrganCost = epsilon;
            TimerOrganCost = epsilon;
            OrganUnitCost = epsilon;
            AbilityFiringBaseCost = epsilon;
            ThrusterFiringCost = epsilon;
            SpinnerFiringCost = epsilon;
            SensorFiringCost = epsilon;
            BiterFiringCost = epsilon;
        }
    }
}
