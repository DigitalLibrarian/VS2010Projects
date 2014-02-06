using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Forever.Render
{
  public class Renderer
  {

    public static void RenderModel(Model model, 
      Matrix world, RenderContext renderContext)
    {

      Renderer.RenderModel(model, 
        world, renderContext.Camera.View, renderContext.Camera.Projection);
    }

    public static void RenderModel(Model model, 
      Matrix world, Matrix view, Matrix proj)
    {
      Matrix[] transforms = new Matrix[model.Bones.Count];
      model.CopyAbsoluteBoneTransformsTo(transforms);

      foreach (ModelMesh mesh in model.Meshes)
      {
        foreach (BasicEffect effect in mesh.Effects)
        {
          effect.EnableDefaultLighting();
          
          effect.View = view;
          effect.Projection = proj;
          effect.World = transforms[mesh.ParentBone.Index] * world;
        }
        mesh.Draw();
        
      }
    }


    public static void RenderVertexPositionColorList(RenderContext render_context, 
      Matrix world,
      VertexPositionColor[] vertices, VertexDeclaration vertexDeclaration, 
      VertexBuffer vertex_buffer  )
    {
      GraphicsDevice gd = render_context.GraphicsDevice;
      BasicEffect effect = render_context.BasicEffect;
      Matrix view = render_context.Camera.View;
      Matrix proj = render_context.Camera.Projection;
      /* Would the real RenderVertexPositionColorList please stand up? */
      RenderVertexPositionColorList(gd, effect, world, view, proj, vertices, vertexDeclaration, vertex_buffer);
    }

    public static void RenderVertexPositionColorList(GraphicsDevice gd, 
      BasicEffect effect, Matrix world, Matrix view, Matrix proj, 
      VertexPositionColor[] vertices, VertexDeclaration vertexDeclaration, 
      VertexBuffer vertex_buffer  )
    {

     // gd.VertexDeclaration = vertexDeclaration;
       

      effect.World = world;
      effect.View = view;
      effect.Projection = proj;
      effect.VertexColorEnabled = true;

      if (vertex_buffer == null)
      {
        vertex_buffer = new VertexBuffer(gd, typeof(VertexPositionColor), vertices.Length, BufferUsage.WriteOnly);
        vertex_buffer.SetData<VertexPositionColor>(vertices);
      }

      foreach (EffectPass pass in effect.CurrentTechnique.Passes)
      {
          pass.Apply();
        gd.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertices, 0, vertices.Length / 3);

      }

    }



    #region Fields

    static VertexPositionColor[] verts = new VertexPositionColor[8];
    static int[] wireframeIndices = new int[]  
        {  
            0, 1,  
            1, 2,  
            2, 3,  
            3, 0,  
            0, 4,  
            1, 5,  
            2, 6,  
            3, 7,  
            4, 5,  
            5, 6,  
            6, 7,  
            7, 4,  
        };

    static int[] solidIndices = new int[]  
        {  
          0, 1, 3,
          1, 2, 3,
          1, 5, 2, 
          5, 2, 6,
          4, 1, 0, 
          4, 5, 1, 
          4, 7, 6,
          4, 6, 5,
          0, 4, 3,
          4, 3, 7,
          7, 3, 2,
          6, 7, 2,

          
          3, 1, 0,
          3, 2, 1,
          2, 5, 1,
          6, 2, 5,
          0, 1, 4,
          1, 5, 4,
          6, 7, 4,
          5, 6, 4,
          3, 4, 0,
          7, 3, 4,
          2, 3, 7,
          2, 7, 6

        };

    static BasicEffect effect;
    static VertexDeclaration vertDecl;

    private static BoundingBox UnitBox = new BoundingBox(new Vector3(-1f, -1f, -1f), new Vector3(1f, 1f, 1f));
    #endregion

    public static void RenderUnitCubeTransform(RenderContext renderContext, Matrix unitCubeTransform,  Matrix world, Color color, bool wireFrame = false)
    {
        RenderUnitCubeTransform(renderContext.GraphicsDevice, unitCubeTransform, world, renderContext.Camera.View, renderContext.Camera.Projection, color, wireFrame);
    }

    public static void RenderUnitCubeTransform(
        GraphicsDevice graphicsDevice,
        Matrix unitCubeTransform,
        Matrix world,
        Matrix view,
        Matrix projection,
        Color color,
        bool wireFrame)
    {
        if (effect == null)
        {
            effect = new BasicEffect(graphicsDevice);
            effect.VertexColorEnabled = true;
            effect.LightingEnabled = false;

            vertDecl = VertexPositionColor.VertexDeclaration;
        }
        var box = UnitBox;
        Vector3[] corners = box.GetCorners();


        for (int i = 0; i < 8; i++)
        {
            verts[i].Position = Vector3.Transform(corners[i], unitCubeTransform);
            verts[i].Color = color;
        }

        effect.World = world;
        effect.View = view;
        effect.Projection = projection;


        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();

            if (wireFrame)
            {


                graphicsDevice.DrawUserIndexedPrimitives(
                  PrimitiveType.LineList,
                   verts,
                    0,
                    8,
                    wireframeIndices,
                    0,
                    wireframeIndices.Length / 2);
            }
            else
            {

                graphicsDevice.DrawUserIndexedPrimitives(
                  PrimitiveType.TriangleList,
                   verts,
                    0,
                    8,
                    solidIndices,
                    0,
                    24);
            }

        }
    }

  }
}
