using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.Life
{
    
        public class LifeForce
        {
            private const float EnergyFlatline = 0f;
            public const float EnergyDamping = 0.999f;
            public float BasalEnergyCost { get; private set; }
            public float MaxEnergy { get; private set; }
            public float Energy { get; private set; }
            public long Age { get; private set; }
            public bool IsDead
            {
                get
                {
                    return Energy <= EnergyFlatline;
                }
            }

            public LifeForce(float maxEnergy, float basalEnergyCost)
            {
                Age = 0;
                MaxEnergy = maxEnergy;
                BasalEnergyCost = basalEnergyCost;
                Energy = MaxEnergy;
            }

            public bool CanPayEnergyCost(float cost)
            {
                return Energy >= cost;
            }

            public bool PayEnergyCost(float cost)
            {
                if (!CanPayEnergyCost(cost)) return false;
                Energy -= cost;
                return true;
            }

            public void AddEnergy(float energy)
            {
                Energy += energy;
            }

            public void Update(float duration)
            {
                if (!IsDead)
                {
                    PayEnergyCost(BasalEnergyCost);
                    Age++;

                    Energy *= EnergyDamping;
                }
            }

        public const float BodyPartUnitCost = 0.01f;
        public const float NeuralOrganCost = 0.001f;
        public const float AbilityOrganCost = 0.001f;
        public const float OrganUnitCost = 0.0001f;
        public const float AbilityFiringBaseCost = 0.00001f;
        public static float CalcBasal(Organism org)
        {

            float totalCost =
                ((float) org.Body.Parts.Count) * BodyPartUnitCost + 
                ((float)org.TotalOrgans) * OrganUnitCost;
            foreach (var bodyPart in org.Body.Parts)
            {
                foreach (var organ in bodyPart.Organs)
                {
                    switch (organ.OrganType)
                    {
                        case Organs.OrganType.Neural:
                            totalCost += NeuralOrganCost;
                            break;
                        case Organs.OrganType.Ability:
                            totalCost += AbilityOrganCost;
                            break;
                        default:
                            throw new NotImplementedException("organ type need cost algorithm");
                    }
                }
            }

            return totalCost;
        }

   }
}
