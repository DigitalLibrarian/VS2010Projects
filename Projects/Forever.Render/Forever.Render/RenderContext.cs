using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Microsoft.Xna.Framework.Content;

namespace Forever.Render
{
  public class RenderContext
  {

    ICamera _camera;
    public ICamera Camera { get { return _camera; } }

    BasicEffect basic_effect;
    public BasicEffect BasicEffect { get { return basic_effect; } }

    GraphicsDevice graphics_device;
    public GraphicsDevice GraphicsDevice { get { return graphics_device; } }

    public RenderContext(ICamera cam, GraphicsDevice gd)
    {
      _camera = cam;
      basic_effect = new BasicEffect(gd);
      graphics_device = gd;
    }


    public void Render(IRenderable entity, GameTime gameTime){

      entity.Render(this, gameTime);



    }
  }
}
