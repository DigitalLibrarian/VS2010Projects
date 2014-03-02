using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Aquarium.GA.Organs.OrganAbilities
{
    public class FoodBitterAbility : OrganAbility
    {

        public FoodBitterAbility(int param0)
            : base(param0)
        {

        }



        public override int NumInputs
        {
            get { return 1; }
        }

        public override int NumOutputs
        {
            get { return 1; }
        }

        public override Signals.Signal Fire(Bodies.NervousSystem nervousSystem, Organ parent, Signals.Signal signal)
        {
            //TODO - change this sometime, for now we always bite
            double result = 0;
            var env = nervousSystem.Organism.Env;
            if (env != null)
            {

                var searchRadius = 50f;
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
