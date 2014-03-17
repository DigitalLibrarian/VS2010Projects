using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Aquarium.Life.Organs.OrganAbilities
{
    public class FoodBitterAbility : OrganAbility
    {

        public FoodBitterAbility(int param0)
            : base(param0)
        {

        }

        // param0 could be socket index whose normal we use to construct a ray to find find to bite

        public override int NumInputs
        {
            get { return 1; }
        }

        public override int NumOutputs
        {
            get { return 1; }
        }

        public override Signals.Signal Fire(Bodies.NervousSystem nervousSystem, Organ parent, Signals.Signal signal, MutableForceGenerator fg)
        {
            //TODO - change this sometime, for now we always bite
            double result = 0;
            var env = nervousSystem.Organism.Surroundings;
            if (env != null)
            {
                nervousSystem.Organism.LifeForce.PayEnergyCost(LifeForce.BitterFiringCost);
                var searchRadius = 50;
                //find food
                var foods = env.ClosestFoods(nervousSystem.Organism.Position, searchRadius);
                if (foods.Any())
                {
                    var food = foods.FirstOrDefault(f => f != nervousSystem.Organism);
                    if (food != null)
                    {
                        nervousSystem.Organism.Consume(food);
                        result = 1;
                    }
                }
            }

            return new Signals.Signal(new List<double>{result});
        }
    }
}
