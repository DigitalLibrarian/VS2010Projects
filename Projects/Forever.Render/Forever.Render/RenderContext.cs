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
    public ICamera Camera { get { return _camera; } set { _camera = value; } }

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
        Set2DRenderStates(GraphicsDevice);
    }
    public void Set3DRenderStates()
    {
        Set3DRenderStates(GraphicsDevice);
    }

    public static void Set2DRenderStates(GraphicsDevice gd)
    {
        gd.BlendState = BlendState.AlphaBlend;
        gd.DepthStencilState = DepthStencilState.None;
        gd.RasterizerState = RasterizerState.CullCounterClockwise;
        gd.SamplerStates[0] = SamplerState.LinearClamp;
    }

    public static void Set3DRenderStates(GraphicsDevice gd)
    {
        gd.BlendState = BlendState.Opaque;
        gd.DepthStencilState = DepthStencilState.Default;
        gd.SamplerStates[0] = SamplerState.LinearWrap;
    }



    public BoundingFrustum GetViewFrustum()
    {
        return new BoundingFrustum(Camera.View * Camera.Projection);
    }

    public bool InView(BoundingBox box)
    {
        return GetViewFrustum().Contains(box) != ContainmentType.Disjoint;
    }
    public bool InView(BoundingSphere sphere)
    {
        return GetViewFrustum().Contains(sphere) != ContainmentType.Disjoint;
    }

    public Ray GetCameraRay()
    {
        return new Ray(Camera.Position, Camera.Forward);
    }
  }
}
