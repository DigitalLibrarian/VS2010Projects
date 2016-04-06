using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Forever.Render;

namespace Forever.Voxel.SVO
{
    public class ChunkTree
    {
        public int Depth { get; set; }
        public OctTree<Chunk> Tree { get; private set; }
        public static ChunkTree Create(Vector3 pos, float chunkSize, int depth)
        {
            var s = chunkSize;
            float worldSize = s * (float)System.Math.Pow(2, depth);

            var treeBox = new BoundingBox(
                pos + new Vector3(-worldSize, -worldSize, -worldSize),
                pos + new Vector3(worldSize, worldSize, worldSize));
            var tree = OctTree<Chunk>.CreatePreSubdivided(depth, treeBox);

            return new ChunkTree
            {
                Depth = depth,
                Tree = tree
            };
        }

        public void Draw(float duration, RenderContext rc)
        {

        }
    }
}
