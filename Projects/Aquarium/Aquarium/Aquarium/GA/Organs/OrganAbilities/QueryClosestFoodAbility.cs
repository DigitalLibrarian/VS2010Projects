using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Signals;

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
            //TODO - change this sometime, for now we always bite
            double r1=0, r2=0, r3=0;
            var env = nervousSystem.Organism.Env;
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
                        var p = food.Position - nervousSystem.Organism.Position;
                        r1 = p.X;
                        r2 = p.Y;
                        r3 = p.Z;
                    }
                }
            }

            return new Signals.Signal(new List<double> { r1, r2, r3 });
        }
    }
}
