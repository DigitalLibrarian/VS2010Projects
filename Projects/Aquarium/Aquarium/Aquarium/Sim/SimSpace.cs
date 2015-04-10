
using Aquarium.Sim.Agents;
using Forever.SpacePartitions;
using Microsoft.Xna.Framework;
using Forever.Render;
using System.Collections.Generic;
using Aquarium.Life.Environments;
using Aquarium.Life;

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

        public Space<OrganismAgent> InnerSpace { get; private set; }

        public SimSpacePartition(SimSpace space, BoundingBox box, int gridSize) : base(box) 
        {
            Space = space;
            Organisms = new Dictionary<IAgent, OrganismAgent>();
            RayPickers = new TypedSubList<IRayPickable>();
            FoodSpace = new Space<IFood>(gridSize);
            InnerSpace = new Space<OrganismAgent>(gridSize);
        }

        Dictionary<IAgent, OrganismAgent> Organisms { get; set; }
        TypedSubList<IRayPickable> RayPickers { get; set; }

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
                    var pos = orgAgent.Organism.Position;
                    InnerSpace.Register(orgAgent, pos);
                    FoodSpace.Register(orgAgent.Organism, pos);
                    orgAgent.Organism.Surroundings = this;
                }
            }

            RayPickers.CheckAdd(obj);

        }

        public override void UnAssign(IAgent obj)
        {
            base.UnAssign(obj);
            if (Organisms.ContainsKey(obj))
            {
                Organisms.Remove(obj);
                var orgAgent = ((OrganismAgent) obj);
                InnerSpace.UnRegister(orgAgent);
                FoodSpace.UnRegister(orgAgent.Organism);
                orgAgent.Organism.Surroundings = null;
            }

            RayPickers.CheckRemove(obj);
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

        public IEnumerable<OrganismAgent> ClosestOrganisms(Vector3 pos, float radius)
        {
            return InnerSpace.QueryLocalSpace(pos, radius, (c, f) => Vector3.Distance(f.Position, pos) < radius);
        }

        public List<IAgent> FindAll(Ray ray)
        {
            //TODO
            var list = RayPickers.FindAll(picker => picker.IsHit(ray));
            return list.ConvertAll<IAgent>(new System.Converter<IRayPickable, IAgent>((target) => (IAgent) target));
        }



        void ISurroundings.Track(Life.Organism org)
        {
            
        }


     
    }



    public class TypedSubList<T> : List<T>
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
