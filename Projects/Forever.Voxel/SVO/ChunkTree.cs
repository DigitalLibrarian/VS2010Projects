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
        const int VoxelsPerDimension = 16;
        public int Depth { get; set; }
        public OctTree<Chunk> Tree { get; private set; }
        public static ChunkTree Create(Vector3 pos, float worldSize, Func<BoundingBox, Chunk> chunkFactory)
        {
            float chunkSize = VoxelsPerDimension;
            // hmm
            int depth = (int)Math.Log(worldSize/chunkSize, 2);

            var treeBox = new BoundingBox(
                pos + new Vector3(-worldSize, -worldSize, -worldSize),
                pos + new Vector3(worldSize, worldSize, worldSize));
            var tree = OctTree<Chunk>.CreatePreSubdivided(depth, treeBox);

            tree.VisitAtDepth((n) => n.Value = chunkFactory(n.Box), depth - 1);

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
