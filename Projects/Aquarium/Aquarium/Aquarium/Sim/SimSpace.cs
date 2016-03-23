
using Forever.SpacePartitions;
using Microsoft.Xna.Framework;
using Forever.Render;
using System.Collections.Generic;
using Aquarium.Life.Environments;

using Aquarium.Agent;

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

    public class SimSpacePartition : Partition<IAgent>, IOrganismAgentGroup, ISurroundings
    {
        SimSpace Space;

        public Space<IFood> FoodSpace { get; private set; }

        public SimSpacePartition(SimSpace space, BoundingBox box, int gridSize) : base(box) 
        {
            Space = space;
            Organisms = new Dictionary<IAgent, OrganismAgent>();
            RayPickers = new TypeFilterList<IRayPickable>();
            FoodSpace = new Space<IFood>(gridSize);
        }

        Dictionary<IAgent, OrganismAgent> Organisms { get; set; }
        TypeFilterList<IRayPickable> RayPickers { get; set; }

         public ICollection<OrganismAgent> OrganismAgents
         {
             get
             {
                 return Organisms.Values;
             }
         }

        public override bool Assign(IAgent obj)
        {
            if (base.Assign(obj))
            {

                if (obj is OrganismAgent)
                {
                    if (!Organisms.ContainsKey(obj))
                    {
                        var orgAgent = (OrganismAgent)obj;
                        Organisms.Add(obj, orgAgent);

                        FoodSpace.Register(orgAgent.Organism, orgAgent.Organism.Position);
                        orgAgent.Organism.Surroundings = this;
                    }
                }

                RayPickers.CheckAdd(obj);
                return true;
            }
            return false;

        }

        public override bool UnAssign(IAgent obj)
        {
            if (base.UnAssign(obj))
            {
                if (Organisms.ContainsKey(obj))
                {
                    Organisms.Remove(obj);
                    var orgAgent = ((OrganismAgent)obj);
                    FoodSpace.UnRegister(orgAgent.Organism);
                    orgAgent.Organism.Surroundings = null;
                }

                RayPickers.CheckRemove(obj);
                return true;
            }
            return false;
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

        public List<IRayPickable> FindAll(Ray ray)
        {
            //TODO
            return  RayPickers.FindAll(picker => picker.IsHit(ray));
            //return list.ConvertAll<IAgent>(new System.Converter<IRayPickable, IAgent>((target) => (IAgent) target));
        }
    }
    public class TypeFilterList<T> : List<T>
    {
        public bool CheckAdd(object item)
        {
            if (item is T)
            {
                Add((T)item);
                return true;
            }
            return false;
        }

        public bool CheckRemove(object item)
        {
            if (item is T)
            {
                Remove((T)item);
                return true;
            }
            return false;
        }
    }

}
