using System;
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
    class VoxelScreen : UiOverlayGameScreen
    {
        const int ChunksPerDimension = 16;

        ChunkSpace ChunkSpace { get; set; }

        ControlledCraft User { get; set; }

        LabelUiElement TotalInstancesLabel { get; set; }
        LabelUiElement FrustumCullingLabel { get; set; }
        LabelUiElement OcclusionsLabel { get; set; }
        LabelUiElement FPSLabel { get; set; }

        public override void LoadContent()
        {
            base.LoadContent();

            User = CreateControlledCraft();
            User.Body.AngularDamping = 0.67f;
            User.Body.LinearDamping = 0.5f;
            User.Body.Mass = 50f;
            User.ControlForces.Mouse.ThrustIncrement = 0.0000001f;

            ChunkSpace = new ChunkSpace(ChunksPerDimension, ChunkFactory);

            var Box = ChunkSpace.GetBoundingBox();
            var diff = Box.Max - Box.Min;
            var startPos = Vector3.Backward * (diff.Length()/2f);
            RenderContext.Camera.Position = startPos;
            User.Body.Position = startPos;

            Ui.Elements.AddRange(CreateUILayout());
        }

        Chunk ChunkFactory(BoundingBox bb)
        {
            var chunk = new Chunk(bb, ChunksPerDimension);
            chunk.LoadContent(ScreenManager.Game.Content);
            chunk.Initialize(RenderContext.GraphicsDevice);
            var offset = chunk.BlockOffset();
            var pos = bb.Min;// +offset;
            int maxHeight = 100 * ChunksPerDimension;
            float half = maxHeight / 2f;
            var bottomLeft = new Vector3(-half, -half, -half);// +offset;
            chunk.VisitCoords((x, y, z) => {
                var world = chunk.ArrayToChunk(new Vector3(x, y, z));
                float tX = (pos.X + (x * ChunksPerDimension));
                float tY = (pos.Y + (y * ChunksPerDimension));
                float tZ = (pos.Z + (z * ChunksPerDimension));
                
                float n = SmoothNoise(world.X, world.Z);
                chunk.Voxels[x][y][z].Material = new Material(
                    new Color(
                       (float) x / ChunksPerDimension,
                       (float) y / ChunksPerDimension,
                       (float) z / ChunksPerDimension
                        )
                    );

                //if (n < 0) n = 0;
                //var threshold = 0 + (n * ChunksPerDimension) + offset.Y;
                var threshold = bottomLeft.Y + half + (n * half);
                bool active = world.Y < threshold;
                chunk.Voxels[x][y][z].State = active ? VoxelState.Active : VoxelState.Inactive;
            });

            return chunk;
        }
       
        public float Noise(int x, int y)
        {
            long n = x + (y * 57);
            n = (long)((n << 13) ^ n);
            return (float)(1.0 - ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0);
        }


        public float SmoothNoise(float x, float y)
        {

            NoiseQuality quality = NoiseQuality.Standard;
            int seed = 0;
            int octaves = 6;
            double frequency = 0.0005;
            double lacunarity = 0.1;
            double persistence = 0.05;

            var module = new Perlin();
            module.Frequency = frequency;
            module.NoiseQuality = quality;
            module.Seed = seed;
            module.OctaveCount = octaves;
            module.Lacunarity = lacunarity;
            module.Persistence = persistence;
            return (float) (module.GetValue((double)x, (double)y, 10));
            //return (float) (module.GetValue((double)x, (double)y, 10) + 1) / 2.0f;
        }

        public float SmoothNoiseOLD(float x, float y)
        {
            float corners = (Noise((int)(x - 1), (int)(y - 1)) + Noise((int)(x + 1), (int)(y - 1)) + Noise((int)(x - 1), (int)(y + 1)) + Noise((int)(x + 1), (int)(y + 1))) / 16;
            float sides = (Noise((int)(x - 1), (int)y) + Noise((int)(x + 1), (int)y) + Noise((int)x, (int)(y - 1)) + Noise((int)x, (int)(y + 1))) / 8;
            float center = Noise((int)x, (int)y) / 4;
            return corners + sides + center;
        }

        List<IUiElement> CreateUILayout()
        {
            var spriteFont = ScreenManager.Font;
            var actionBarSlotHeight = 40;
            var horizontalActionBar = new ActionBar(RenderContext, 30, actionBarSlotHeight, spriteFont);

            //horizontalActionBar.Slots[0].Action = new ActionBarAction(() => SomeFunc);

            var hud = new ControlledCraftHUD(User, RenderContext);
            hud.LoadContent(ScreenManager.Game.Content, ScreenManager.Game.GraphicsDevice);

            var odometer = new OdometerDashboard(User, ScreenManager.Game.GraphicsDevice, new Vector2(0, -actionBarSlotHeight + -15f), 300, 17);
            
            Vector2 offset = Vector2.Zero;
            Vector2 delta = Vector2.UnitY * 30;

            TotalInstancesLabel = new LabelUiElement(RenderContext, spriteFont, offset);
            FrustumCullingLabel = new LabelUiElement(RenderContext, spriteFont, offset += delta);
            OcclusionsLabel = new LabelUiElement(RenderContext, spriteFont, offset += delta);
            FPSLabel = new LabelUiElement(RenderContext, spriteFont, offset += delta);

            return new List<IUiElement>{
                horizontalActionBar,
                hud, 
                odometer, 
                TotalInstancesLabel, 
                FrustumCullingLabel, 
                OcclusionsLabel,
                FPSLabel
            };
        }

        public override void HandleInput(InputState input)
        {
            User.HandleInput(input);
            

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

            var numChunks = 20;
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
            float fps = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;
            FPSLabel.Label = string.Format("FPS: {0}", (int)fps);
            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            User.Update(gameTime);

            RenderContext.Camera.Position = User.Body.Position;
            RenderContext.Camera.Rotation = User.Body.Orientation;

            
            // fill out space around the player
            GenerateChunks(User.Body.Position, 5);

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        void GenerateChunks(Vector3 pos, int numChunks)
        {
            for (int x = -numChunks; x < numChunks; x++)
            {
                for (int y = -numChunks; y < numChunks; y++)
                {
                    for (int z = -numChunks; z < numChunks; z++)
                    {
                        ChunkSpace.GetOrCreate(pos + new Vector3(x, y, z) * ChunksPerDimension);
                    }
                }
            }
        }
    }
}
