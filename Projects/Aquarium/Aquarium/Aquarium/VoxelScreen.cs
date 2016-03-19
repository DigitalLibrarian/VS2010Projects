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

namespace Aquarium
{
    class InstanceCountUiElement : IUiElement
    {

        RenderContext RenderContext { get; set; }
        SpriteFont Font { get; set; }
        Chunk Chunk { get; set; }
        public InstanceCountUiElement(RenderContext rc, Chunk chunk, SpriteFont font)
        {
            RenderContext = rc;
            Chunk = chunk;
            Font = font;
        }


        public void HandleInput(InputState input)
        {
        }

        public void Draw(GameTime gameTime, SpriteBatch batch)
        {
            var count = Chunk.InstanceCount;
            var capacity = Chunk.Capacity;
            var label = string.Format("Instances : {0} / {1}", count, capacity); 
            var bounds = RenderContext.GraphicsDevice.Viewport.Bounds;
            var position = new Vector2(bounds.Left, bounds.Top);
            var offset = Vector2.Zero;
            position += offset;
            batch.DrawString(Font, label, position, Color.Yellow);
        }

        public void Update(GameTime gameTime)
        {

        }
    }
    class VoxelScreen : UiOverlayGameScreen
    {
        Chunk Chunk { get; set; }

        ControlledCraft User { get; set; }

        public override void LoadContent()
        {
            base.LoadContent();

            User = CreateControlledCraft();
            
            Chunk = new Chunk(64);
            Chunk.LoadContent(ScreenManager.Game.Content);
            Chunk.Initialize(RenderContext.GraphicsDevice);

            var diff = Chunk.Box.Max - Chunk.Box.Min;
            var startPos = Vector3.Backward * (diff.Length()/2f);
            RenderContext.Camera.Position = startPos;
            User.Body.Position = startPos;

            Ui.Elements.AddRange(CreateUILayout());
        }

        List<IUiElement> CreateUILayout()
        {
            var spriteFont = ScreenManager.Font;
            var actionBarSlotHeight = 40;
            var horizontalActionBar = new ActionBar(RenderContext, 30, actionBarSlotHeight, spriteFont);

            var hud = new ControlledCraftHUD(User, RenderContext);
            hud.LoadContent(ScreenManager.Game.Content, ScreenManager.Game.GraphicsDevice);

            var odometer = new OdometerDashboard(User, ScreenManager.Game.GraphicsDevice, new Vector2(0, -actionBarSlotHeight + -15f), 300, 17);

            var counter = new InstanceCountUiElement(RenderContext, Chunk, spriteFont);

            return new List<IUiElement>{
                hud, odometer, counter
            };
        }

        public override void HandleInput(InputState input)
        {
            User.HandleInput(input);

            if (input.CurrentMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                var mousePoint = input.CurrentMousePoint.ToVector2();
                var ray = GetMouseRay(mousePoint);
                Chunk.DerezRay(ray);
            }

            base.HandleInput(input);
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

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            RenderContext.GraphicsDevice.Clear(Color.SkyBlue);
            var duration = (float)gameTime.ElapsedGameTime.Milliseconds;
            Chunk.Draw(duration, RenderContext);
            
            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            User.Update(gameTime);

            RenderContext.Camera.Position = User.Body.Position;
            RenderContext.Camera.Rotation = User.Body.Orientation;

            var duration = (float)gameTime.ElapsedGameTime.Milliseconds;
            Chunk.Update(duration);
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }
    }
}
