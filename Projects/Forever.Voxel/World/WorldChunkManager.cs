using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.Render;
using Microsoft.Xna.Framework;
using Forever.SpacePartitions;
using Forever.Extensions;

namespace Forever.Voxel.World
{
    /*
     * TODO :
     *   - virtualize chunks
     *   - paging
     *   - throttles for rebuilding chunk instance buffers, disposing of chunks, getting new chunks from chunk source
     * */
    public class WorldChunkManager
    {
        IWorldCellSource Source { get; set; }
        RenderContext RenderContext { get; set; }

        ThrottledQueue<SpaceCoord> InsertionQueue { get; set; }
        ThrottledQueue<WorldCell> RebuildQueue { get; set; }
        ThrottledQueue<WorldCell> RemovalQueue { get; set; }
         /// <summary>
         /// This represents the world (in world space) that is loaded in memory
         /// </summary>
        WorldCellSpace WorldSpace { get; set; }
        int TargetChunksLoadedRadius { get; set; }
        float DrawRadius { get; set; }

        public WorldChunkManager(RenderContext rc, IWorldCellSource source, float drawRadius)
        {
            RenderContext = rc;
            Source = source;
            DrawRadius = drawRadius;
        }

        public void Update(GameTime gameTime)
        {
            UpdateInsertions();
        }

        void UpdateInsertions()
        {
            // plan any new chunks that need to be loaded
            if (InsertionQueue.HasCapacity())
            {
                foreach (var coord in FindMissingCoords())
                {
                    if(InsertionQueue.HasCapacity()) break;
                    if (!InsertionQueue.Contains(coord))
                    {
                        InsertionQueue.Enqueue(coord);
                    }
                }
            }

            // now pump the queue
            foreach (var coord in InsertionQueue.Pump())
            {
                LoadPartitionFromSource(coord);
            }
        }


        IEnumerable<SpaceCoord> FindMissingCoords()
        {
            var pos = RenderContext.Camera.Position;
            var numChunks = TargetChunksLoadedRadius;
            for (int x = -numChunks; x < numChunks; x++)
            {
                for (int y = -numChunks; y < numChunks; y++)
                {
                    for (int z = -numChunks; z < numChunks; z++)
                    {
                        var worldVector = pos + new Vector3(x, y, z) * WorldSpace.GridSize;
                        var coord = WorldSpace.PositionToCoord(worldVector);
                        if (!WorldSpace.IsLoaded(coord))
                        {
                            yield return coord;
                        }
                    }
                }
            }
        }

        void LoadPartitionFromSource(SpaceCoord coord)
        {
            var worldCell = Source.Get(coord);
            var partition = WorldSpace.GetOrCreate(coord);
            partition.Clear();
            partition.Assign(worldCell);
        }

        public void Draw(GameTime gameTime)
        {
            var duration = gameTime.GetDuration();
            var drawSphere = new BoundingSphere(RenderContext.Camera.Position, DrawRadius);
            var renderSet = WorldSpace.Query((coord, worldCell) =>
            {
                return drawSphere.Intersects(worldCell.Chunk.Box) && RenderContext.InView(worldCell.Chunk.Box);
            });

            foreach (var worldCell in renderSet)
            {
                worldCell.Chunk.Draw(duration, RenderContext);
            }
        }
    }
}
