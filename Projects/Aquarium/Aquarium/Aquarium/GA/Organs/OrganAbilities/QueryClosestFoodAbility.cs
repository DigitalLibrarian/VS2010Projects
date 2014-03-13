using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Signals;
using Microsoft.Xna.Framework;

namespace Aquarium.GA.Organs.OrganAbilities
{
    public class QueryClosestFoodAbility : OrganAbility
    {

        public QueryClosestFoodAbility(int param0)
            : base(param0)
        {

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

                var searchRadius = 25f;
                //find food
                var foods = env.ClosestFoods(nervousSystem.Organism.Position, searchRadius);
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
