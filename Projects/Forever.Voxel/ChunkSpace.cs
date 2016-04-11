using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.SpacePartitions;
using Microsoft.Xna.Framework;

namespace Forever.Voxel
{
    public class ChunkSpace : Space<Chunk>
    {
        Func<BoundingBox, Chunk> ChunkFactory { get; set; }
        public ChunkSpace(int gridSize, Func<BoundingBox, Chunk> chunkFactory) : base(gridSize)
        {
            ChunkFactory = chunkFactory;
        }

        protected override IPartition<Chunk> CreateNewPartition(BoundingBox box)
        {
            return new ChunkSpacePartition(box, ChunkFactory(box));
        }
    }

    public class ChunkSpacePartition : Partition<Chunk>
    {
        public Chunk Chunk { get; private set; }
        public ChunkSpacePartition(BoundingBox box, Chunk chunk) : base(box)
        {
            Chunk = chunk;
            this.Assign(chunk);
        }
    }
}
