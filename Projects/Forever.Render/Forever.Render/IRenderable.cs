using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using Forever.Render;

namespace Forever.Render
{
  public interface IRenderable
  {
    void LoadContent(ContentManager content);
    void Render(RenderContext renderContext, GameTime gameTime);
  }
}
