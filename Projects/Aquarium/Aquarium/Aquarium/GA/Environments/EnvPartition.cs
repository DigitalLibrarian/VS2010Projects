using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.SpacePartitions;
using Microsoft.Xna.Framework;
using Aquarium.GA.Population;
using Forever.SpacePartitions;

namespace Aquarium.GA.Environments
{

    public interface IEnvMember
    {
        PopulationMember Member { get; }
        Vector3 Position { get; }

        void EnterEnvPartition(EnvPartition p);
        void ExitEnvPartition(EnvPartition p);
    }
    public class EnvPartition : Partition<IEnvMember>, ISurroundings
    {

        public Space<IFood> FoodSpace { get; private set; }

        public EnvPartition(BoundingBox box, int gridSize)
            : base(box)
        {
            FoodSpace = new Space<IFood>(gridSize);
        }



        public override void Assign(IEnvMember obj)
        {
            base.Assign(obj);
            FoodSpace.Register(obj.Member.Specimen, obj.Position);
            obj.EnterEnvPartition(this);
        }

        public override void UnAssign(IEnvMember obj)
        {
            base.UnAssign(obj);
            FoodSpace.UnRegister(obj.Member.Specimen);
            obj.ExitEnvPartition(this);
        }

        public IEnumerable<IFood> ClosestFoods(Vector3 pos, float radius)
        {
            return FoodSpace.QueryLocalSpace(pos, radius, (c, f) => Vector3.Distance(f.Position, pos) < radius);
        }
    }


}
