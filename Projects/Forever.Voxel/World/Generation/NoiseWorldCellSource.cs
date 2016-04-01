using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LibNoise;
using Microsoft.Xna.Framework;
using Forever.SpacePartitions;

namespace Forever.Voxel.World.Generation
{
    public class NoiseWorldCellSource :  IWorldCellSource
    {
        WorldDimensions Dim { get; set; }
        IModule Noise { get; set; }

        public NoiseWorldCellSource(WorldDimensions dim, IModule module)
        {
            Dim = dim;
            Noise = module;
        }

        public WorldCell Get(SpaceCoord coord)
        {
            var voxelsPerDim = Dim.VoxelsPerDimension;
            var voxelScale = new Vector3(1f, 1f, 1f);

            var chunkCenter = new Vector3(voxelScale.X * coord.X, voxelScale.Y * coord.Y, voxelScale.Z * coord.Z) * voxelsPerDim;
            
            //var chunk = new Chunk(bb, voxelsPerDim);
            var chunk = new Chunk(chunkCenter, voxelScale, voxelsPerDim);
            //chunk.LoadContent(ScreenManager.Game.Content);
            //chunk.Initialize(RenderContext.GraphicsDevice);
            var pos = chunk.Box.Min;
            int maxHeight = 100 * voxelsPerDim;
            float half = maxHeight / 2f;
            var bottomLeft = new Vector3(-half, -half, -half);
            chunk.VisitCoords((x, y, z) =>
            {
                var world = chunk.ArrayToChunk(new Vector3(x, y, z));
                float tX = (pos.X + (x * voxelsPerDim));
                float tY = (pos.Y + (y * voxelsPerDim));
                float tZ = (pos.Z + (z * voxelsPerDim));

                float n = SmoothNoise(world.X, world.Z);
                chunk.Voxels[x][y][z].Material = new Material(
                    new Color(
                       (float)x / voxelsPerDim,
                       (float)y / voxelsPerDim,
                       (float)z / voxelsPerDim
                        )
                    );

                var threshold = bottomLeft.Y + half + (n * half);
                bool active = world.Y < threshold;
                chunk.Voxels[x][y][z].State = active ? VoxelState.Active : VoxelState.Inactive;
            });

            return new WorldCell(chunk);
        }

        float SmoothNoise(float x, float y)
        {
            return (float)(Noise.GetValue((double)x, (double)y, 10));
        }
    }
}
