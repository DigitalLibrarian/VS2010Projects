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

        public ThrottledQueue<SpaceCoord> SourceQueue { get; private set; }
        public ThrottledQueue<Tuple<SpaceCoord, WorldCell>> InitQueue { get; private set; }
        public ThrottledQueue<SpaceCoord> RebuildQueue { get; private set; }
        public ThrottledQueue<SpaceCoord> RemovalQueue { get; private set; }
         /// <summary>
         /// This represents the world (in world space) that is loaded in memory
         /// </summary>
        WorldCellSpace WorldSpace { get; set; }
        WorldDimensions Dim { get; set; }
        int TargetChunksLoadedRadius { get; set; }
        float DrawRadius { get; set; }

        SpaceCoord[] MissingCoordBuffer;

        public WorldChunkManager(RenderContext rc, WorldDimensions dim, IWorldCellSource source, float drawRadius, int loadedChunksRadius)
        {
            RenderContext = rc;
            Dim = dim;
            Source = source;
            DrawRadius = drawRadius;
            TargetChunksLoadedRadius = loadedChunksRadius;

            int length = TargetChunksLoadedRadius * 2;
            int pumpSize = 1;
            int capacity = 10;
            SourceQueue = new ThrottledQueue<SpaceCoord>(pumpSize, capacity);
            InitQueue = new ThrottledQueue<Tuple<SpaceCoord, WorldCell>>(pumpSize, capacity);
            RebuildQueue = new ThrottledQueue<SpaceCoord>(pumpSize, capacity);
            RemovalQueue = new ThrottledQueue<SpaceCoord>(pumpSize, capacity);

            WorldSpace = new WorldCellSpace(dim.VoxelsPerDimension, source);
            MissingCoordBuffer = new SpaceCoord[length * length * length];
        }

        public void Update(GameTime gameTime)
        {
            UpdateInsertions();
        }

        void UpdateInsertions()
        {
            // plan any new chunks that need to be loaded
            if (SourceQueue.HasCapacity())
            {
                //int num = FindMissingCoords(MissingCoordBuffer);
                //for(int i = 0; i < num; i++)
                foreach (var coord in FindMissingCoords())
                {
                   // var coord = MissingCoordBuffer[i];
                    //if (!SourceQueue.HasCapacity()) return;
                    if (!SourceQueue.Contains(coord))
                    {
                        WorldSpace.GetOrCreate(coord);
                        SourceQueue.Enqueue(coord);
                    }
                }
            }

            // now pump the queue
            //foreach (var coord in SourceQueue.Pump())
            {
                //LoadPartitionFromSource(coord);

                //var worldCell = Source.Get(coord);
            }
        }


        // TODO - this needs to go
        public void Initialize(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            if (!SourceQueue.Any()) return;
            //foreach (var tuple in InitQueue.Pump())
            foreach(var coord in SourceQueue.Pump())
            {
                //var coord = tuple.Item1;
                //var worldCell = tuple.Item2;

                var worldCell = Source.Get(coord);

                worldCell.Chunk.LoadContent(content);
                worldCell.Chunk.Initialize(RenderContext.GraphicsDevice);
                var partition = WorldSpace.GetOrCreate(coord);
                partition.Clear();
                partition.Assign(worldCell);
            }
        }

        int FindMissingCoords(SpaceCoord[] buffer)
        {
            int index = 0;
            var pos = RenderContext.Camera.Position;
            var numChunks = TargetChunksLoadedRadius;
            for (int x = -numChunks; x < numChunks; x++)
            {
                for (int y = -numChunks; y < numChunks; y++)
                {
                    for (int z = -numChunks; z < numChunks; z++)
                    {
                        var worldVector = pos + (new Vector3(x, y, z) * WorldSpace.GridSize);
                        var coord = WorldSpace.PositionToCoord(worldVector);
                        if (!WorldSpace.IsLoaded(coord))
                        {
                            buffer[index++] = coord;
                        }
                    }
                }
            }
            return index;
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

            //worldCell.Chunk.Initialize(RenderContext.GraphicsDevice);
            var partition = WorldSpace.GetOrCreate(coord);
            //partition.Clear();
            //partition.Assign(worldCell);

            InitQueue.Enqueue(new Tuple<SpaceCoord, WorldCell>(coord, worldCell));
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
