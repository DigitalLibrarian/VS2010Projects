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


            Chunk = new Chunk(16);
            Chunk.LoadContent(ScreenManager.Game.Content);
            Chunk.Initialize(RenderContext.GraphicsDevice);

            var diff = Chunk.Box.Max - Chunk.Box.Min;
            RenderContext.Camera.Position = Vector3.Backward * (diff.Length()/2f);
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
