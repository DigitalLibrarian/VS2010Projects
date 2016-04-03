﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Forever.Voxel;
using Microsoft.Xna.Framework;
using Forever.Render;
using Aquarium.Ui;
using Forever.Screens;
using Forever.Extensions;
using Microsoft.Xna.Framework.Graphics;
using Aquarium.UI;

using LibNoise;

namespace Aquarium
{
    class VoxelScreen : FlyAroundGameScreen
    {
        const int ChunksPerDimension = 16;

        ChunkSpace ChunkSpace { get; set; }

        LabelUiElement TotalInstancesLabel { get; set; }
        LabelUiElement FrustumCullingLabel { get; set; }
        LabelUiElement OcclusionsLabel { get; set; }

        public override void LoadContent()
        {
            base.LoadContent();

            ChunkSpace = new ChunkSpace(ChunksPerDimension, ChunkFactory);

            var spawnHeightAboveGround = 5 * ChunksPerDimension;
            var spawnPoint = new Vector3(0, GetHeight(0, 0) + spawnHeightAboveGround, 0);
            User.Body.Position = spawnPoint;
            Ui.Elements.AddRange(CreateUILayout());
            SceneLoad(spawnPoint);
            ConsumeLoadSequence(1000);
        }

        Chunk ChunkFactory(BoundingBox bb)
        {
            var chunk = new Chunk(bb, ChunksPerDimension);
            chunk.LoadContent(ScreenManager.Game.Content);
            chunk.Initialize(RenderContext.GraphicsDevice);
            var pos = bb.Min;
            chunk.VisitCoords((x, y, z) => {
                var world = chunk.ArrayToChunk(new Vector3(x, y, z));
                float tX = (pos.X + (x * ChunksPerDimension));
                float tY = (pos.Y + (y * ChunksPerDimension));
                float tZ = (pos.Z + (z * ChunksPerDimension));
                
                chunk.Voxels[x][y][z].Material = new Material(
                    new Color(
                       (float) x / ChunksPerDimension,
                       (float) y / ChunksPerDimension,
                       (float) z / ChunksPerDimension
                        )
                    );

                float height = GetHeight(world.X, world.Z);
                bool active = world.Y < height;
                chunk.Voxels[x][y][z].State = active ? VoxelState.Active : VoxelState.Inactive;
            });

            return chunk;
        }

        float GetHeight(float x, float z)
        {
            int maxHeight = 100 * ChunksPerDimension;
            float half = maxHeight / 2f;
            var bottomLeft = new Vector3(-half, -half, -half); ;
            var scale = 0.5f;
            float n = SmoothNoise(x * scale, z * scale);
            var threshold = bottomLeft.Y + half + (n * half);
            return threshold;
        }

        Perlin Perlin = null;

        void SetupPerlin()
        {
            NoiseQuality quality = NoiseQuality.Standard;
            int seed = 0;
            int octaves = 6;
            double frequency = 0.005;
            double lacunarity = 0.5;
            double persistence = 0;

            var module = new Perlin();
            module.Frequency = frequency;
            module.NoiseQuality = quality;
            module.Seed = seed;
            module.OctaveCount = octaves;
            module.Lacunarity = lacunarity;
            module.Persistence = persistence;
            Perlin = module;
        }

        public float SmoothNoise(float x, float y)
        {
            if (Perlin == null) SetupPerlin();

            return (float) (Perlin.GetValue((double)x, (double)y, 10));
        }

        List<IUiElement> CreateUILayout()
        {
            var spriteFont = ScreenManager.Font;
            var actionBarSlotHeight = 40;
            var horizontalActionBar = new ActionBar(RenderContext, 30, actionBarSlotHeight, spriteFont);

            TotalInstancesLabel = new LabelUiElement(RenderContext, spriteFont, DebugLabelStrip());
            FrustumCullingLabel = new LabelUiElement(RenderContext, spriteFont, DebugLabelStrip());
            OcclusionsLabel = new LabelUiElement(RenderContext, spriteFont, DebugLabelStrip());

            return new List<IUiElement>{
                horizontalActionBar,
                TotalInstancesLabel, 
                FrustumCullingLabel, 
                OcclusionsLabel
            };
        }

        public override void HandleInput(InputState input)
        {
            if (input.CurrentMouseState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                var mousePoint = input.CurrentMousePoint.ToVector2();
                var ray = GetMouseRay(mousePoint);
                ShootChunks(ray, ChunkRayTool.Derez);
            }

            if (input.CurrentMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released
                && input.LastMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                var mousePoint = input.CurrentMousePoint.ToVector2();
                var ray = GetMouseRay(mousePoint);
                ShootChunks(ray, ChunkRayTool.Rez, false);
            }

            if (input.CurrentMouseState.MiddleButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                var pos = User.Body.Position;
                (ChunkSpace.GetOrCreate(pos) as ChunkSpacePartition).Chunk.Invalidate();
            }

            if(input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.K))
            {
                ToggleDebug();
            }

            base.HandleInput(input);
        }

        public void ShootChunks(Ray ray, ChunkRayTool tool, bool closestFirst = true)
        {
            var rayHits = ChunkSpace.Query((coord, chunk) =>
            {
                return chunk.Box.Intersects(ray).HasValue;
            });

            if (closestFirst)
            {
                rayHits = rayHits.OrderBy(x => (x.Position - ray.Position).LengthSquared());
            }
            else
            {
                rayHits.OrderByDescending(x => (x.Position - ray.Position).LengthSquared());
            }

            foreach (var chunk in rayHits)
            {
                if(chunk.ToolRay(ray, tool))
                {
                    return;
                }
            }
        }

        public Ray GetMouseRay(Vector2 mousePosition)
        {
            var projection = RenderContext.Camera.Projection;
            var view = RenderContext.Camera.View;
            Viewport viewport = RenderContext.GraphicsDevice.Viewport;

            Vector3 near = new Vector3(mousePosition, 0);
            Vector3 far = new Vector3(mousePosition, 1);

            near = viewport.Unproject(near, projection, view, Matrix.Identity);
            far = viewport.Unproject(far, projection, view, Matrix.Identity);

            return new Ray(RenderContext.Camera.Position, Vector3.Normalize(far - near));
        }

        int InViewCount = 0;
        int OutOfViewCount = 0;
        public override void Draw(GameTime gameTime)
        {
            var duration = gameTime.GetDuration();
            InViewCount = OutOfViewCount = 0;

            int count = 0;
            int capacity = 0;

            var numChunks = 100;
            var sphere = new BoundingSphere(RenderContext.Camera.Position, ChunksPerDimension * numChunks);
            
            var renderSet = ChunkSpace.Query((coord, chunk) =>
            {
               return sphere.Intersects(chunk.Box);
            });

            foreach(var chunk in renderSet)
            {
                count += chunk.InstanceCount;
                capacity += chunk.Capacity;

                if (RenderContext.InView(chunk.Box))
                {
                    chunk.Draw(duration, RenderContext);
                    if (debug)
                    {
                        var rc = RenderContext;
                        Renderer.Render(chunk.Box, rc.GraphicsDevice, chunk.World, rc.Camera.View, rc.Camera.Projection, Color.DarkSalmon);
                    }

                    InViewCount++;
                }
                else
                {
                    OutOfViewCount++;
                }
            }

            TotalInstancesLabel.Label = string.Format("Instances : {0} / {1}", count, capacity);
            int totalChunks = InViewCount + OutOfViewCount;
            FrustumCullingLabel.Label = string.Format("Chunks In View: {0} / {1}", InViewCount, totalChunks);
            var part = ChunkSpace.GetOrCreate(User.Body.Position) as ChunkSpacePartition;
            OcclusionsLabel.Label = string.Format("Local Occlusions : {0}", part.Chunk.TotalOcclusions);
            base.Draw(gameTime);
        }
        bool debug;
        void ToggleDebug()
        {
            debug = !debug;
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (!otherScreenHasFocus && !coveredByOtherScreen)
            {
                // fill out space around the player
                SceneLoad(User.Body.Position);
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        void SceneLoad(Vector3 pos)
        {
            if (LoadSequence == null || !LoadSequence.MoveNext())
            {
                int numChunks = 25;
                var camHeight = pos.Y;

                if (camHeight > GetHeight(RenderContext.Camera.Position.X, RenderContext.Camera.Position.Z))
                {
                    LoadSequence = SceneLoadSequence_CameraAboveGround(pos, numChunks).GetEnumerator();
                }
                else
                {
                    LoadSequence = SceneLoadSequence_CameraBelowGround(pos, numChunks / 2).GetEnumerator();
                }
            }
            else
            {
                if (Random.NextDouble() < 0.01)
                {
                    ConsumeLoadSequence();
                }
            }
        }

        void ConsumeLoadSequence(int max = 5)
        {
            for (int i = 0; i < max; i++)
            {
                ChunkSpace.GetOrCreate(LoadSequence.Current);

                if (!LoadSequence.MoveNext()) break;
            }
        }

        private void SceneLoad_CameraBelowGround(Vector3 pos, int numChunks)
        {
            for (int x = -numChunks; x < numChunks; x++)
            {
                for (int z = -numChunks; z < numChunks; z++)
                {
                    for (int y = -numChunks; y < numChunks; y++)
                    {
                        ChunkSpace.GetOrCreate(pos + new Vector3(x, y, z) * ChunksPerDimension);
                    }
                }
            }
        }

        void SceneLoad_CameraAboveGround(Vector3 pos, int numChunks)
        {
            float worldX, worldY, worldZ;
            float surfaceY;
            for (int x = -numChunks; x < numChunks; x++)
            {
                worldX = pos.X + (x * ChunksPerDimension);
                for (int z = -numChunks; z < numChunks; z++)
                {
                    worldZ = pos.Z + (z * ChunksPerDimension);
                    surfaceY = GetHeight(worldX, worldZ);
                    for (int y = -1; y < 2; y++)
                    {
                        worldY = surfaceY + ( y * ChunksPerDimension );
                        ChunkSpace.GetOrCreate(new Vector3(worldX, worldY, worldZ));
                    }
                }
            }
        }

        #region Scene Loading Sequences
        IEnumerator<Vector3> LoadSequence { get; set; }
        IEnumerable<Vector3> SceneLoadSequence_CameraBelowGround(Vector3 pos, int numChunks)
        {
            for (int x = -numChunks; x < numChunks; x++)
            {
                for (int z = -numChunks; z < numChunks; z++)
                {
                    for (int y = -numChunks; y < numChunks; y++)
                    {
                        yield return pos + new Vector3(x, y, z) * ChunksPerDimension;
                    }
                }
            }
        }
        IEnumerable<Vector3> SceneLoadSequence_CameraAboveGround(Vector3 pos, int numChunks)
        {
            float worldX, worldY, worldZ;
            float surfaceY;
            for (int x = -numChunks; x < numChunks; x++)
            {
                worldX = pos.X + (x * ChunksPerDimension);
                for (int z = -numChunks; z < numChunks; z++)
                {
                    worldZ = pos.Z + (z * ChunksPerDimension);
                    surfaceY = GetHeight(worldX, worldZ);
                    for (int y = -1; y < 2; y++)
                    {
                        worldY = surfaceY + (y * ChunksPerDimension);
                        yield return new Vector3(worldX, worldY, worldZ);
                    }
                }
            }
        }
        #endregion
    }
}
