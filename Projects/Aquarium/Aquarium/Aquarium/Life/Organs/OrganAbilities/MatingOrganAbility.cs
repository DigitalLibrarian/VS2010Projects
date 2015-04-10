using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.Life.Environments;
using Aquarium.Sim.Agents;

namespace Aquarium.Life.Organs.OrganAbilities
{

    public class MatingOrganAbility : OrganAbility
    {
        IMatingManager MatingManager { get; set; }
        public MatingOrganAbility(IMatingManager mating, int param0) : base(param0)
        {
            MatingManager = mating;
        }



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

            //TODO - this needs a cooldown, then you can do asexual now and pair selection later

            var num = signal.Value[0];
            float result = 0;
            if (num > 0.5)
            {
                nervousSystem.Organism.LifeForce.PayEnergyCost(LifeForce.TryingToMate);
                var me = nervousSystem.Organism;
                var surround = me.Surroundings;


                var pos = me.Position;
                float rad = 10f;

                var poss = surround.ClosestOrganisms(pos, rad);
                OrganismAgent myAgent = null;
                OrganismAgent other = null;

                foreach (var candi in poss)
                {
                    if (candi.Organism == me)
                    {
                        myAgent = candi;
                    }
                    if (candi.Organism != me)
                    {
                        other = candi;
                    }
                    if (myAgent != null && other != null) break;
                }


                if (myAgent == null || other == null)
                {
                    result = -1f;
                }
                else
                {
                    if(MatingManager.TryMate(myAgent, other))
                    {
                        nervousSystem.Organism.LifeForce.PayEnergyCost(LifeForce.SuccessfullyMating);
                        result = 1f;
                    }else{
                        result = 0f;
                    }
                }
            }
            return new Signals.Signal(Signals.SignalEncoding.Encode(result));
        }


    }
}
