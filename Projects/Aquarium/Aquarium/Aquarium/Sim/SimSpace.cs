
using Aquarium.Sim.Agents;
using Forever.SpacePartitions;
using Microsoft.Xna.Framework;
using Forever.Render;
using System.Collections.Generic;
using Aquarium.GA.Environments;

namespace Aquarium.Sim
{
    public class SimSpace : Space<IAgent>
    {
        public float UpdateRadius { get; private set; }
        int FoodSpaceGridSize;
        public SimSpace(int gridSize, int foodSpaceGridSize)
            : base(gridSize)
        {
            FoodSpaceGridSize = foodSpaceGridSize;
        }

        protected override IPartition<IAgent> CreateNewPartition(Microsoft.Xna.Framework.BoundingBox box)
        {
            return new SimSpacePartition(this, box, FoodSpaceGridSize);
        }
    }

    public class SimSpacePartition : Partition<IAgent>, IOrganismAgentPool, ISurroundings
    {
        SimSpace Space;

        
        public Space<IFood> FoodSpace { get; private set; }

        public SimSpacePartition(SimSpace space, BoundingBox box, int gridSize) : base(box) 
        {
            Space = space;
            Organisms = new Dictionary<IAgent, OrganismAgent>();
            FoodSpace = new Space<IFood>(gridSize);
        }

         Dictionary<IAgent, OrganismAgent> Organisms { get; set; }

         public ICollection<OrganismAgent> OrganismAgents
         {
             get
             {
                 return Organisms.Values;
             }
         }

        public override void Assign(IAgent obj)
        {
            base.Assign(obj);

            if (obj is OrganismAgent)
            {
                if (!Organisms.ContainsKey(obj))
                {
                    var orgAgent = (OrganismAgent)obj;
                    Organisms.Add(obj, orgAgent);

                    FoodSpace.Register(orgAgent.Organism, orgAgent.Organism.Position);
                    orgAgent.Organism.Env = this;
                }
            }

        }

        public override void UnAssign(IAgent obj)
        {
            base.UnAssign(obj);
            if (Organisms.ContainsKey(obj))
            {
                Organisms.Remove(obj);
                var orgAgent = ((OrganismAgent) obj);
                FoodSpace.UnRegister(orgAgent.Organism);
                orgAgent.Organism.Env = null;
            }
        }


        public void Birth(OrganismAgent agent)
        {
            Space.Register(agent, agent.Organism.Position);
        }


        public void Death(OrganismAgent agent)
        {
            Space.UnRegister(agent);
        }


        public IEnumerable<IFood> ClosestFoods(Vector3 pos, float radius)
        {
            return FoodSpace.QueryLocalSpace(pos, radius, (c, f) => Vector3.Distance(f.Position, pos) < radius);
        }
    }
}
