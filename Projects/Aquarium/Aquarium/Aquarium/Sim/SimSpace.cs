
using Aquarium.Sim.Agents;
using Forever.SpacePartitions;
using Microsoft.Xna.Framework;
using Forever.Render;

namespace Aquarium.Sim
{
    public class SimSpace : Space<IAgent>
    {
        public float UpdateRadius { get; private set; }

        public SimSpace(int gridSize)
            : base(gridSize)
        {

        }

        protected override IPartition<IAgent> CreateNewPartition(Microsoft.Xna.Framework.BoundingBox box)
        {
            return new SimSpacePartition(box);
        }
    }

    public class SimSpacePartition : Partition<IAgent>
    {
        public SimSpacePartition(BoundingBox box) : base(box) { }

        public void Update(GameTime gameTime)
        {
            
        }

        //TODO - keep list of organisms available for gene pool

        public override void Assign(IAgent obj)
        {
            base.Assign(obj);
        }

        public override void UnAssign(IAgent obj)
        {
            base.UnAssign(obj);
        }
    }
}
