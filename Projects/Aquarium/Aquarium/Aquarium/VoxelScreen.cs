using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Forever.Voxel;
using Microsoft.Xna.Framework;
using Forever.Render;

namespace Aquarium
{
    class VoxelScreen : UiOverlayGameScreen
    {
        Chunk Chunk { get; set; }
        public override void LoadContent()
        {
            base.LoadContent();

            RenderContext.Camera.Position = Vector3.Backward * 15;

            Chunk = new Chunk(10);
            Chunk.LoadContent(ScreenManager.Game.Content);
            Chunk.Initialize(RenderContext.GraphicsDevice);
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            RenderContext.GraphicsDevice.Clear(Color.SkyBlue);
            var duration = (float)gameTime.ElapsedGameTime.Milliseconds;
            Chunk.Draw(duration, RenderContext);

            base.Draw(gameTime);
        }
    }
}
