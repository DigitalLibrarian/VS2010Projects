using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

using Forever.Extensions;
namespace Forever.Voxel.SVO
{
    public class OctTreeNode
    {
        public const int Subdivisions = 8;

        public OctTreeNode(OctTreeNode parent, BoundingBox box)
        {
            Parent = parent;
            Box = box;
            Children = null;
            Occupied = false;
        }

        public bool IsLeaf { get { return Children == null; } }
        public BoundingBox Box { get; set; }
        public OctTreeNode Parent { get; private set; }
        public OctTreeNode[] Children { get; set; }
        public bool Occupied { get; set; }


        public void Subdivide()
        {
            var min = Box.Min;
            var max = Box.Max;
            var mid = Box.GetCenter();
            // TODO - This assumes uniform cubes.  If we want cuboid voxels this will need to be reworked
            var n = Math.Abs(max.X - min.X) / 2;
            var halfSize = mid - min;
            var halfX = new Vector3(n, 0, 0);
            var halfY = new Vector3(0, n, 0);
            var halfZ = new Vector3(0, 0, n);

            Children = new OctTreeNode[Subdivisions];

            Children[NodePosition.NEAR_BOTTOM_LEFT] = new OctTreeNode(this, BBMin(min, halfSize));
            Children[NodePosition.NEAR_BOTTOM_RIGHT] = new OctTreeNode(this, BBMin(min + halfX, halfSize));

            Children[NodePosition.NEAR_TOP_LEFT] = new OctTreeNode(this, BBMin(min + halfY, halfSize));
            Children[NodePosition.NEAR_TOP_RIGHT] = new OctTreeNode(this, BBMin(min + halfY + halfX, halfSize));

            Children[NodePosition.FAR_BOTTOM_RIGHT] = new OctTreeNode(this, BBMin(min+halfX+halfZ, halfSize)); 
            Children[NodePosition.FAR_BOTTOM_LEFT] = new OctTreeNode(this, BBMin(min + halfZ, halfSize));
            
            Children[NodePosition.FAR_TOP_RIGHT] = new OctTreeNode(this, BBMin(max - halfSize, halfSize));
            Children[NodePosition.FAR_TOP_LEFT] = new OctTreeNode(this, BBMin(max-halfSize-halfX, halfSize));
        }

        public void PruneChildren()
        {
            Occupied = true;
            Children = null;
            if (Parent != null  && !Parent.Occupied && Parent.SearchFirstChild(x => !x.Occupied) == null)
            {
                Parent.PruneChildren();
            }
        }

        public OctTreeNode SearchFirstChild(Predicate<OctTreeNode> pred)
        {
            if (IsLeaf) return null;
            foreach (var child in Children)
            {
                if (pred(child))
                {
                    return child;
                }
            }
            return null;
        }

        BoundingBox BBMin(Vector3 min, Vector3 boxSize)
        {
            return new BoundingBox(min, min + boxSize);
        }

        public OctTreeNode FindLeaf(Vector3 v)
        {
            // TODO - optimize this
            if(IsLeaf) return this;
            if (Box.Contains(v) == ContainmentType.Contains) return this;

            foreach (var child in Children)
            {
                var w = child.FindLeaf(v);
                if (w != null)
                {
                    return w;
                }
            }
            return null;
        }
    }
}
