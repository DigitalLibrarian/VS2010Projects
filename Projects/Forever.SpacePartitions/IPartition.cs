using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Forever.SpacePartitions
{
    public interface IPartition<T> 
    {
        BoundingBox Box { get; set; }
        IList<T> Objects { get; }
        bool Assign(T obj);
        bool UnAssign(T obj);
        void Clear();
    }
}
