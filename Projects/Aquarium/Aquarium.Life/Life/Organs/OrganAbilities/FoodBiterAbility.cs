﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Aquarium.Life.Organs.OrganAbilities
{
    public class FoodBiterAbility : OrganAbility
    {

        public FoodBiterAbility(int param0)
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

            if (signal.Value[0] > 0.5f)
            {
                var env = nervousSystem.Organism.Surroundings;
                if (env != null)
                {
                    nervousSystem.Organism.LifeForce.PayEnergyCost(LifeForce.Data.BiterFiringCost);
                    var bb = nervousSystem.Organism.LocalBB;

                    var searchRadius = (bb.Max - bb.Min).Length() / 2f;
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
            }

            return new Signals.Signal(new List<double>{result});
        }
    }
}
