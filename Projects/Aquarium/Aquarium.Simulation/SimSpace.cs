
using Forever.SpacePartitions;
using Microsoft.Xna.Framework;
using Forever.Render;
using System.Collections.Generic;
using Aquarium.Life.Environments;


namespace Aquarium.Sim
{
    public class SimSpace : Space<ISimObject>
    {
        public float UpdateRadius { get; private set; }
        int FoodSpaceGridSize;
        public SimSpace(int gridSize, int foodSpaceGridSize)
            : base(gridSize)
        {
            FoodSpaceGridSize = foodSpaceGridSize;
        }

        protected override IPartition<ISimObject> CreateNewPartition(Microsoft.Xna.Framework.BoundingBox box)
        {
            return new SimSpacePartition(this, box, FoodSpaceGridSize);
        }
    }

    public class SimSpacePartition<IFood> : Partition<ISimObject>, IOrganismAgentPool, ISurroundings
    {
        SimSpace Space;

        public Space<IFood> FoodSpace { get; private set; }

        public SimSpacePartition(SimSpace space, BoundingBox box, int gridSize) : base(box) 
        {
            Space = space;
            SimObjects = new List<ISimObject>();

            //Organisms = new Dictionary<ISimObject, OrganismAgent>();
            RayPickers = new TypeFilterList<IRayPickable>();
            FoodSpace = new Space<IFood>(gridSize);
        }

        List<ISimObject> SimObjects { get; set; }

        //Dictionary<ISimObject, OrganismAgent> Organisms { get; set; }
        TypeFilterList<IRayPickable> RayPickers { get; set; }
        /*
         public ICollection<OrganismAgent> OrganismAgents
         {
             get
             {
                 return Organisms.Values;
             }
         }
         * */
        
         public override bool Assign(ISimObject obj)
        {
            if (!base.Assign(obj))
            {

                //if (obj is OrganismAgent)
                if(!SimObjects.Contains(obj))
                {
                    //if (!Organisms.ContainsKey(obj))
                    {
                        var orgAgent = (OrganismAgent)obj;
                        Organisms.Add(obj, orgAgent);

                        FoodSpace.Register(orgAgent.Organism, orgAgent.Organism.Position);
                        orgAgent.Organism.Surroundings = this;
                    }
                }

                return RayPickers.CheckAdd(obj);
            }
            return true;

        }

         public override bool UnAssign(ISimObject obj)
        {

            if (RayPickers.CheckRemove(obj))
            {

                if (Organisms.ContainsKey(obj))
                {
                    Organisms.Remove(obj);
                    var orgAgent = ((OrganismAgent)obj);
                    FoodSpace.UnRegister(orgAgent.Organism);
                    orgAgent.Organism.Surroundings = null;
                }
                return base.UnAssign(obj);
            }
            return false;

        }


        public void Birth(ISimObject agent)
        {
            Space.Register(agent, agent.Position);
        }


        public void Death(ISimObject agent)
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
}
