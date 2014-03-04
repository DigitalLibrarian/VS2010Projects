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


    public void Set2DRenderStates()
    {
        GraphicsDevice.BlendState = BlendState.AlphaBlend;
        GraphicsDevice.DepthStencilState = DepthStencilState.None;
        GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
    }
    public void Set3DRenderStates()
    {
        // Set suitable renderstates for drawing a 3D model
        GraphicsDevice.BlendState = BlendState.Opaque;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
    }


  }
}
