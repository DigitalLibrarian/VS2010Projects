using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Forever.Render;
using Forever.Screens;

namespace Aquarium.Ui
{
    //public class Button
    //{
    //    public delegate void OnClickEventHandler();
    //    public event OnClickEventHandler OnClick;

    //    public Rectangle ScreenRect { get; private set; }
    //    public Texture2D Texture { get; private set; }
    //    public Color Color { get; set; }
    //    public Button(Rectangle rect, Texture2D texture)
    //    {
    //        ScreenRect = rect;
    //        Texture = texture;
    //        Color = Color.White;
    //    }

    //    public void HandleInput(InputState input)
    //    {
    //        if (input.IsMouseLeftClick() && ScreenRect.Contains(input.CurrentMousePoint))
    //        {
    //            if(OnClick != null) OnClick();
    //        }
    //    }

    //    public void Draw(GameTime gameTime, SpriteBatch batch, RenderContext renderContext)
    //    {
    //        batch.Draw(Texture, ScreenRect, Color);
    //    }
    //}
}
