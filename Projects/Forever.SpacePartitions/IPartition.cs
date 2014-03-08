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
        IEnumerable<T> Objects { get; }
        void Assign(T obj);
        void UnAssign(T obj);
    }
}
