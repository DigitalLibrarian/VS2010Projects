using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Forever.SpacePartitions
{
    public class Partition<T> : IPartition<T>
    {
        public BoundingBox Box { get;  set; }
        List<T> contents;
        public IEnumerable<T> Objects { get { return contents; } }

        public Partition(BoundingBox box)
        {
            contents = new List<T>();
            Box = box;
        }

        public virtual bool Assign(T obj) 
        {
            if (!contents.Contains(obj))
            {
                contents.Add(obj);
                return true;
            }
            return false;
        }

        public virtual bool UnAssign(T obj)
        {
            return contents.Remove(obj);
        }
    }
}
