using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Forever.Screens;
using Forever.Render;
using Forever.Voxel;
using Aquarium.Ui;

namespace Aquarium.UI
{
    class InstanceCountUiElement : IUiElement
    {
        RenderContext RenderContext { get; set; }
        SpriteFont Font { get; set; }
        ChunkSpace ChunkSpace { get; set; }
        public InstanceCountUiElement(RenderContext rc, ChunkSpace chunk, SpriteFont font)
        {
            RenderContext = rc;
            ChunkSpace = chunk;
            Font = font;
        }


        public void HandleInput(InputState input)
        {
        }

        public void Draw(GameTime gameTime, SpriteBatch batch)
        {
            int count = 0;
            int capacity = 0;
            foreach (var partition in ChunkSpace.Partitions)
            {
                var chunk = (partition as ChunkSpacePartition).Chunk;
                count += chunk.InstanceCount;
                capacity += chunk.Capacity;
            }

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
}
