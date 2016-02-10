using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.Life.Signals;
using Microsoft.Xna.Framework;

namespace Aquarium.Life.Organs.OrganAbilities
{
    public class QueryClosestFoodAbility : OrganAbility
    {
        private float MinRadius = 25f;
        private float MaxRadius = 150f;
        public float SearchRadius { get; private set; }
        public QueryClosestFoodAbility(int param0)
            : base(param0)
        {
            SearchRadius = (float)Fuzzy.InRange((int)param0, (int)MinRadius, (int)MaxRadius);
        }

        public override int NumInputs
        {
            get { return 1;} 
        }

        public override int NumOutputs
        {
            get { return 3; }
        }

        public override Signal Fire(Bodies.NervousSystem nervousSystem, Organ parent, Signals.Signal signal, MutableForceGenerator fg)
        {
            Vector3 vector = Vector3.Zero;
            var env = nervousSystem.Organism.Surroundings;
            if (env != null)
            {
                nervousSystem.Organism.LifeForce.PayEnergyCost(LifeForce.Data.SensorFiringCost);
                //find food
                var foods = env.ClosestFoods(nervousSystem.Organism.Position, SearchRadius);
                if (foods.Any())
                {
                    var food = foods.FirstOrDefault(f => f != nervousSystem.Organism);
                    if (food != null)
                    {
                        vector = food.Position - nervousSystem.Organism.Position;
                    }
                }
            }

            return new Signals.Signal(SignalEncoding.Encode(vector));
        }
    }
}
