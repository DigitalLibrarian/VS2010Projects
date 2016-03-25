using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Forever.SpacePartitions
{
    public class DynamicSpace<T> : Space<T> where T : class
    {
        public delegate void OnRegisterEventHandler(T obj, Vector3 pos);
        public event OnRegisterEventHandler OnRegister;

        public delegate void OnUnRegisterEventHandler(T obj);
        public event OnUnRegisterEventHandler OnUnRegister;
        Dictionary<T, IPartition<T>> PartitionAssignment { get; set; }

        public DynamicSpace(int gridSize) : base(gridSize)
        {
            PartitionAssignment = new Dictionary<T, IPartition<T>>();
        }

        public void AssignPartition(T obj, IPartition<T> par)
        {
            par.Assign(obj);
            PartitionAssignment[obj] = par;
        }

        public void UnRegister(T obj)
        {
            if (PartitionAssignment.ContainsKey(obj))
            {
                IPartition<T> p = PartitionAssignment[obj];
                p.UnAssign(obj);
                PartitionAssignment.Remove(obj);
            }

            if (OnUnRegister != null) OnUnRegister.Invoke(obj);
        }


        public void Register(T obj, Vector3 position)
        {
            var coord = PositionToCoord(position);
            var par = GetOrCreate(coord);
            AssignPartition(obj, par);

            if (OnRegister != null) OnRegister.Invoke(obj, position);
        }

        public override void RemovePartition(IPartition<T> partition)
        {
            var coord = GetPartitionCoord(partition);
            if (coord.HasValue)
            {
                foreach (var o in partition.Objects)
                {
                    UnRegister(o);
                }

                base.RemovePartition(partition);
            }

        }

        public void Update(T obj, Vector3 position)
        {
            //TODO - this hasn't been tested

            var p = PartitionAssignment[obj];
            if (p.Box.Contains(position) == ContainmentType.Disjoint)
            {
                UnRegister(obj);
                Register(obj, position);
            }
        }


    }
}
