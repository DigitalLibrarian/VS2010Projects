using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Forever.Render.Instancing
{
    public class InstancingClass : IDisposable
    {
        public VertexBuffer GeometryBuffer { get; private set; }
        public VertexBuffer InstanceBuffer { get; set; }
        public IndexBuffer IndexBuffer { get; private set; }
        public VertexDeclaration InstanceVertexDeclaration { get; private set; }
        public Effect Effect { get; private set; }

        public InstancingClass(
            VertexBuffer geometryBuffer, 
            VertexBuffer instanceBuffer, 
            IndexBuffer indexBuffer, 
            VertexDeclaration vertexDeclaration,
            Effect effect
            )
        {
            GeometryBuffer = geometryBuffer;
            InstanceBuffer = instanceBuffer;
            IndexBuffer = indexBuffer;


            InstanceVertexDeclaration = vertexDeclaration;
            Effect = effect;
        }

        public void Draw(float duration, RenderContext renderContext, int instanceCount)
        {
            var gd = renderContext.GraphicsDevice;

            var bindings = new VertexBufferBinding[2];
            bindings[0] = new VertexBufferBinding(GeometryBuffer);
            bindings[1] = new VertexBufferBinding(InstanceBuffer, 0, 1);
            gd.SetVertexBuffers(bindings);
            gd.Indices = IndexBuffer;

            Effect.CurrentTechnique.Passes[0].Apply();
            gd.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 8, 0, 24, instanceCount);
        }


        public void Dispose()
        {
            GeometryBuffer.Dispose();
            InstanceBuffer.Dispose();
            IndexBuffer.Dispose();
        }
    }
}
