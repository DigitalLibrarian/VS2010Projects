using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Forever.Render;
using Microsoft.Xna.Framework;

namespace Aquarium.Ui
{
    public class Reticule
    {
        Texture2D SpriteSheet { get; set; }
        int CellWidth { get; set; }
        int CellHeight { get; set; }

        int ScreenWidth { get; set; }
        int ScreenHeight { get; set; }

        int Index { get; set; }

        public Color Color { get; set; }

        public Reticule(Texture2D sheet, int index, int cellWidth, int cellHeight, 
            int screenWidth, int screenHeight)
        {
            SpriteSheet = sheet;
            Index = index;
            CellWidth = cellWidth;
            CellHeight = cellHeight;
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
            Color = Color.White;
        }

        public void Draw(SpriteBatch spriteBatch, Point center)
        {
            var destRect = new Rectangle(center.X - (ScreenWidth / 2), center.Y - (ScreenHeight / 2), ScreenWidth, ScreenHeight);
            var sourceRect = GetSourceRect(Index);

            spriteBatch.Draw(SpriteSheet, destRect, sourceRect, Color);
        }

        private Rectangle GetSourceRect(int index)
        {
            var leftX = index * CellWidth;
            var upY = 0;

            return new Rectangle(leftX, upY, CellWidth, CellHeight);
        }
    }
}
