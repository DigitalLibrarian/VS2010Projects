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

            if (node.IsLeaf)
            {
                node.Subdivide();
                for (int i = 0; i < NodePosition.MaxNodeCount; i++)
                {
                    Subdivide(node.Children[i], depth - 1);
                }
            }
        }

        public OctTreeNode<T> SubdivideOrGet(Vector3 v, int maxDepth)
        {
            return SubdivideOrGet(Root, v, maxDepth);
        }

        private static OctTreeNode<T> SubdivideOrGet(OctTreeNode<T> node, Vector3 v, int maxDepth)
        {
            if (maxDepth == 0)
            {
                if (node.Box.Contains(v) == ContainmentType.Contains)
                {
                    return node;
                }
                return null;
            }

            if (node.IsLeaf)
            {
                Subdivide(node, maxDepth);
            }

            OctTreeNode<T> t = null;
            for (int i = 0; i < NodePosition.MaxNodeCount && t == null; i++)
            {
                t = SubdivideOrGet(node.Children[i], v, maxDepth - 1);
                if (t != null) return t;
            }
            return t;
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

        public IEnumerable<OctTreeNode<T>> RayCast(Ray r)
        {
            foreach (var n in SVO.RayCast.RayTraverse<T>(this.Root, r))
            {
                yield return n;
            }
        }

        public OctTreeNode<T> GetLeafContaining(Vector3 v)
        {
            return FindFirstLeaf((x) => x.Box.Contains(v) != ContainmentType.Disjoint);
        }

        public OctTreeNode<T> FindFirstLeaf(Predicate<OctTreeNode<T>> action)
        {
            return FindFirstLeaf(action, Root);
        }

        OctTreeNode<T> FindFirstLeaf(Predicate<OctTreeNode<T>> action, OctTreeNode<T> node)
        {
            if (action(node))
            {
                if (node.IsLeaf)
                {
                    return node;
                }

                OctTreeNode<T> t = null;
                for (int i = 0; i < OctTreeNode<T>.Subdivisions && t == null; i++)
                {
                    t = FindFirstLeaf(action, node.Children[i]);

                    /*
                    if (action(node.Children[i]))
                    {
                        if (node.Children[i].IsLeaf)
                        {
                            return node.Children[i];
                        }

                        t = FindFirstLeaf(action, node.Children[i]);
                    }
                     * */
                }
                return t;
            }
            return null;
        }

        public IEnumerable<OctTreeNode<T>> GetLeaves(Predicate<OctTreeNode<T>> pred)
        {
            return GetLeaves(Root, pred);
        }

        public IEnumerable<OctTreeNode<T>> GetLeaves(OctTreeNode<T> node, Predicate<OctTreeNode<T>> pred)
        {
            if (pred(node))
            {
                if (node.IsLeaf)
                {
                    yield return node;
                }
                else
                {
                    for (int i = 0; i < OctTreeNode<T>.Subdivisions; i++)
                    {
                        foreach (var l in GetLeaves(node.Children[i], pred))
                        {
                            yield return l;
                        }
                    }
                }
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
