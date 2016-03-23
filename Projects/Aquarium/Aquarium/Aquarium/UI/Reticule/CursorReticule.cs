using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Forever.Screens;
using Microsoft.Xna.Framework;

namespace Aquarium.Ui
{
    public class CursorReticule : Reticule
    {
        Point Point { get; set; }
        public CursorReticule(Texture2D sheet, int index, int cellWidth, int cellHeight,
            int screenWidth, int screenHeight)
            : base(sheet, index, cellWidth, cellHeight, screenWidth, screenHeight) { }

        public void HandleInput(InputState input)
        {
            Point = input.CurrentMousePoint;
        }

        public void DrawOnCursor(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, Point);
        }
    }
}
