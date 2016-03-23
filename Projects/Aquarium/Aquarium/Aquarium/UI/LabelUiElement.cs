using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.Ui;
using Forever.Render;
using Microsoft.Xna.Framework.Graphics;
using Forever.Screens;
using Microsoft.Xna.Framework;

namespace Aquarium.UI
{
    class LabelUiElement : IUiElement
    {
        public Vector2 Offset { get; set; }
        public string Label { get; set; }
        RenderContext RenderContext { get; set; }
        SpriteFont Font { get; set; }
        public LabelUiElement(RenderContext rc, SpriteFont font, Vector2 offset, string label = "")
        {
            RenderContext = rc;
            Font = font;
            Offset = offset;
            Label = label;
        }


        public void HandleInput(InputState input)
        {
        }

        public void Draw(GameTime gameTime, SpriteBatch batch)
        {
            var bounds = RenderContext.GraphicsDevice.Viewport.Bounds;
            var position = new Vector2(bounds.Left, bounds.Top);
            position += Offset;
            batch.DrawString(Font, Label, position, Color.Yellow);
        }

        public void Update(GameTime gameTime)
        {

        }
    }
}
