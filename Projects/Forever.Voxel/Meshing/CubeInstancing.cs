using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.Render.Instancing;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Forever.Render;
using Forever.Extensions; 
namespace Forever.Voxel.Meshing
{
    public interface IInstancer
    {
        void Draw(RenderContext rc, VertexBufferBinding instanceBufferBinding, int instanceCount);
    }

    public class CubeInstancing : IInstancer
    {
        readonly int VertexCount = 8;
        readonly int IndexCount = 72;

        VertexBuffer GeometryBuffer { get; set; }
        IndexBuffer IndexBuffer { get; set; }
        
        VertexBufferBinding[] Bindings { get; set; }

        public CubeInstancing(GraphicsDevice device, float scale=1f)
        {
            var vertexCount = VertexCount;
            GeometryBuffer = new VertexBuffer(device, VertexPositionColor.VertexDeclaration,
                                              vertexCount, BufferUsage.WriteOnly);

            var indexCount = IndexCount;
            IndexBuffer = new IndexBuffer(device, typeof(int), indexCount, BufferUsage.WriteOnly);

            SetUpGeometry(scale);

            Bindings = new VertexBufferBinding[2];
            Bindings[0] = new VertexBufferBinding(GeometryBuffer);
        }

        private void SetUpGeometry(float scale)
        {
            int[] solidIndices = new int[]  
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
            float unitHalfSize = 0.5f;
            var box = new BoundingBox(
                new Vector3(-unitHalfSize, -unitHalfSize, -unitHalfSize), 
                new Vector3(unitHalfSize, unitHalfSize, unitHalfSize));
            // TODO - make a custom vertex class that is only a position
            var verts = box.GetCorners().Select(x => new VertexPositionColor
            {
                Position = x * scale,
                // this color should never appear
                Color = Color.Purple
            }).ToArray();

            GeometryBuffer.SetData(verts);
            IndexBuffer.SetData(solidIndices);
        }

        public void Draw(RenderContext rc, VertexBufferBinding instanceBufferBinding, int instanceCount)
        {
            if (instanceCount == 0) return;
            Bindings[1] = instanceBufferBinding;

            rc.GraphicsDevice.SetVertexBuffers(Bindings);
            rc.GraphicsDevice.Indices = IndexBuffer;

            rc.GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 8, 0, 24, instanceCount);
        }
        
        public void Dispose()
        {
            GeometryBuffer.Dispose();
            IndexBuffer.Dispose();
        }
    }
}
