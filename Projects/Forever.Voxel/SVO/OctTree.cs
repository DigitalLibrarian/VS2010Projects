using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

using Forever.Extensions;

namespace Forever.Voxel.SVO
{
    public class OctTree<T>
    {
        public OctTreeNode<T> Root { get; private set; }

        public OctTree(BoundingBox box)
        {
            Root = new OctTreeNode<T>(null, box);
        }

        public static OctTree<T> CreatePreSubdivided(int depth, BoundingBox box)
        {
            var tree = new OctTree<T>(box);
            Subdivide(tree.Root, depth);
            return tree;
        }

        public static void Subdivide(OctTreeNode<T> node, int depth)
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

        
        public IEnumerable<OctTreeNode<T>> Search(Predicate<OctTreeNode<T>> consumer)
        {
            return Search(consumer, Root);
        }

        IEnumerable<OctTreeNode<T>> Search(Predicate<OctTreeNode<T>> consumer, OctTreeNode<T> node)
        {
            bool keeper = consumer(node);
            if (keeper)
            {
                if (!node.IsLeaf)
                {
                    for (int i = 0; i < NodePosition.MaxNodeCount; i++)
                    {
                        if (!node.IsLeaf)
                        {
                            foreach (var n in Search(consumer, node.Children[i]))
                            {
                                yield return n;
                            }
                        }
                    }
                }

                yield return node;
            }
        }

        public void Visit(Action<OctTreeNode<T>> action, int depth)
        {
            Visit(action, depth, Root);
        }


        void Visit(Action<OctTreeNode<T>> action, int depth, OctTreeNode<T> node)
        {
            if (node == null) return;
            if (depth < 0) return;

            action(node);

            for (int i = 0; i < OctTreeNode<T>.Subdivisions; i++)
            {
                Visit(action, depth - 1, node.Children[i]);
            }
        }

        public void VisitLeaves(Action<OctTreeNode<T>> action)
        {
            VisitLeaves(action, Root);
        }

        void VisitLeaves(Action<OctTreeNode<T>> action, OctTreeNode<T> node)
        {
            if(node.IsLeaf)
            {
                action(node);
            }

            for (int i = 0; i < OctTreeNode<T>.Subdivisions; i++)
            {
                if (node.IsLeaf) return;
                VisitLeaves(action, node.Children[i]);
            }
        }

        public void VisitAtDepth(Action<OctTreeNode<T>> action, int depth)
        {
            VisitAtDepth(action, depth, Root);
        }

        void VisitAtDepth(Action<OctTreeNode<T>> action, int depth, OctTreeNode<T> node)
        {
            if (node == null) return;
            if (depth == 0 || node.IsLeaf)
            {
                action(node);
                return;
            }

            for (int i = 0; i < OctTreeNode<T>.Subdivisions; i++)
            {
                VisitAtDepth(action, depth - 1, node.Children[i]);
            }
        }
        #endregion
    }
}
