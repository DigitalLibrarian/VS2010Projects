using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Aquarium.GA.SpacePartitions
{
    public class Partition<T>
    {
        public BoundingBox Box { get;  set; }
        public List<T> Objects { get; private set; }

        public Partition(BoundingBox box)
        {
            Objects = new List<T>();
            Box = box;
        }

    }
}
