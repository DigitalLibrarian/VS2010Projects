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

namespace Aquarium
{
    class VoxelScreen : UiOverlayGameScreen
    {
        const int ChunksPerDimension = 16;

        ChunkSpace ChunkSpace { get; set; }

        ControlledCraft User { get; set; }

        LabelUiElement TotalInstancesLabel { get; set; }
        LabelUiElement FrustumCullingLabel { get; set; }
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

            int numChunks = 5;
            for (int x = -numChunks; x < numChunks; x++)
            {
                for (int y = -numChunks; y < numChunks; y++)
                {
                    for (int z = -numChunks; z < numChunks; z++)
                    {
                        ChunkSpace.GetOrCreate(new Vector3(x * ChunksPerDimension, y * ChunksPerDimension, z * ChunksPerDimension));
                    }
                }
            }

            var Box = ChunkSpace.GetBoundingBox();
            var diff = Box.Max - Box.Min;
            var startPos = Vector3.Backward * (diff.Length()/2f);
            RenderContext.Camera.Position = startPos;
            User.Body.Position = startPos;

            //ChunkSpace.GetOrCreate(User.Body.Position);
            Ui.Elements.AddRange(CreateUILayout());
        }

        Chunk ChunkFactory(BoundingBox bb)
        {
            var chunk = new Chunk(bb, ChunksPerDimension);
            chunk.LoadContent(ScreenManager.Game.Content);
            chunk.Initialize(RenderContext.GraphicsDevice);
            return chunk;
        }

        List<IUiElement> CreateUILayout()
        {
            var spriteFont = ScreenManager.Font;
            var actionBarSlotHeight = 40;
            var horizontalActionBar = new ActionBar(RenderContext, 30, actionBarSlotHeight, spriteFont);

            var hud = new ControlledCraftHUD(User, RenderContext);
            hud.LoadContent(ScreenManager.Game.Content, ScreenManager.Game.GraphicsDevice);

            var odometer = new OdometerDashboard(User, ScreenManager.Game.GraphicsDevice, new Vector2(0, -actionBarSlotHeight + -15f), 300, 17);


            Vector2 offset = Vector2.Zero;
            Vector2 delta = Vector2.UnitY * 30;

            TotalInstancesLabel = new LabelUiElement(RenderContext, spriteFont, offset);
            FrustumCullingLabel = new LabelUiElement(RenderContext, spriteFont, offset += delta);
            FPSLabel = new LabelUiElement(RenderContext, spriteFont, offset += delta);

            return new List<IUiElement>{
                hud, odometer, 
                TotalInstancesLabel, 
                FrustumCullingLabel, 
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
                ChunkSpace.GetOrCreate(pos);
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
        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            var duration = (float)gameTime.ElapsedGameTime.Milliseconds;
            InViewCount = OutOfViewCount = 0;

            int count = 0;
            int capacity = 0;

            foreach (var partition in ChunkSpace.Partitions)
            {
                var chunk = (partition as ChunkSpacePartition).Chunk;

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
            FrustumCullingLabel.Label = string.Format("In View: {0} / {1}", InViewCount, totalChunks);
            float fps = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;
            FPSLabel.Label = string.Format("FPS: {0}", (int)fps);
            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            User.Update(gameTime);


            RenderContext.Camera.Position = User.Body.Position;
            RenderContext.Camera.Rotation = User.Body.Orientation;

            var duration = (float)gameTime.ElapsedGameTime.Milliseconds;

            foreach (var partition in ChunkSpace.Partitions)
            {
                var chunk = (partition as ChunkSpacePartition).Chunk;
                chunk.Update(duration);
            }
            
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }
    }
}
