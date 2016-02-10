using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.Life
{
    public class LifeForceData
    {
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
            BodyPartUnitCost = 0.0001f;
            BodyPartVolumeCost = 0.01f;
            NeuralOrganCost = 0.001f;
            AbilityOrganCost = 0.001f;
            TimerOrganCost = 0.00001f;
            OrganUnitCost = 0.0001f;
            AbilityFiringBaseCost = 0.00001f;
            ThrusterFiringCost = 0.0001f;
            SpinnerFiringCost = 1f;
            SensorFiringCost = 0.00001f;
            BiterFiringCost = 0.001f;
        }
    }
}
