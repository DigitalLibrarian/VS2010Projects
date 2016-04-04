using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

using Forever.Extensions;

namespace Forever.Voxel.SVO
{
    public class OctTree
    {
        public OctTreeNode Root { get; private set; }

        public OctTree(BoundingBox box)
        {
            Root = new OctTreeNode(box);
        }

        public static OctTree CreatePreSubdivided(int depth, BoundingBox box)
        {
            var tree = new OctTree(box);
            Subdivide(tree.Root, depth);
            return tree;
        }

        public static void Subdivide(OctTreeNode node, int depth)
        {
            if (node == null) return;
            if (depth == 0) return; // leaf

            node.Subdivide();
            for(int i = 0; i < NodePosition.MaxNodeCount; i++)
            {
                Subdivide(node.Children[i], depth-1);
            }
        }
        #region Traversal

        public void Visit(Action<OctTreeNode> action, int depth)
        {
            Visit(action, depth, Root);
        }

        void Visit(Action<OctTreeNode> action, int depth, OctTreeNode node)
        {
            if (node == null) return;
            if (depth < 0) return;

            action(node);

            for (int i = 0; i < OctTreeNode.Subdivisions; i++)
            {
                Visit(action, depth - 1, node.Children[i]);
            }
        }

        public void VisitAtDepth(Action<OctTreeNode> action, int depth)
        {
            VisitAtDepth(action, depth, Root);
        }

        void VisitAtDepth(Action<OctTreeNode> action, int depth, OctTreeNode node)
        {
            if (node == null) return;
            if (depth == 0)
            {
                action(node);
                return;
            }

            for (int i = 0; i < OctTreeNode.Subdivisions; i++)
            {
                VisitAtDepth(action, depth - 1, node.Children[i]);
            }
        }
        #endregion
    }
}
