using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.Life.Spec;
using Microsoft.Xna.Framework;

namespace Aquarium.Life
{
    public class LifeForce
    {
        private const float EnergyFlatline = 1f;
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

        public LifeForce(float maxEnergy)
        {
            Age = 0;
            MaxEnergy = maxEnergy;
            Energy = MaxEnergy;
        }

        public bool CanPayEnergyCost(float cost)
        {
            return Energy >= cost;
        }

        public bool PayEnergyCost(float cost)
        {
            Energy -= cost;
            return true;
        }

        public void AddEnergy(float energy)
        {
            Energy += energy;
            if (Energy > MaxEnergy)
            {
                Energy = MaxEnergy;
            }
        }

        public void Update(float duration, Organism org)
        {
            if (!IsDead)
            {
                PayEnergyCost(CalcLivingCost(org));
                Age++;
            }
        }

        public static LifeForceData Data = new LifeForceData();

        public static float CalcLivingCost(Organism org)
        {
            float totalCost = Data.BasalEnergyCost;
            totalCost += ((float)org.Body.Parts.Count) * Data.BodyPartUnitCost;
            totalCost += ((float)org.TotalOrgans) * Data.OrganUnitCost;

            foreach (var bodyPart in org.Body.Parts)
            {
                var scale = Vector3.Transform(bodyPart.Scale, bodyPart.BodyWorld);
                totalCost += scale.Length() * Data.BodyPartVolumeCost;
                foreach (var organ in bodyPart.Organs)
                {
                    switch (organ.OrganType)
                    {
                        case Organs.OrganType.Neural:
                            totalCost += Data.NeuralOrganCost;
                            break;
                        case Organs.OrganType.Ability:
                            totalCost += Data.AbilityOrganCost;
                            break;
                        case Organs.OrganType.Timer:
                            totalCost += Data.TimerOrganCost;
                            break;
                        default:
                            throw new NotImplementedException(string.Format("organ type {0} needs cost algorithm",  organ.OrganType));
                    }
                }
            }

            return totalCost;
        }

   }
}
