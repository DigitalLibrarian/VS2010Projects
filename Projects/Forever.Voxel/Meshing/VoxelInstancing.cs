using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.Render.Instancing;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace Forever.Voxel.Meshing
{
    public class VoxelInstancing
    {
        readonly int VertexCount = 8;
        readonly int IndexCount = 72;

        InstancingClass Instancing { get; set; }
        Effect Effect { get; set; }

        public Vector3 VoxelScale { get; set; }
        public int VoxelCapacity { get; private set; }
        public VertexDeclaration InstanceVertexDeclaration { get; private set; }

        public VoxelInstancing(int voxelCapacity, Vector3 voxelScale)
        {
            VoxelCapacity = voxelCapacity;
            VoxelScale = voxelScale;
        }

        public void LoadContent(Game game)
        {
            var device = game.GraphicsDevice;
            var vertexCount = VertexCount;
            var geometryBuffer = new VertexBuffer(device, VertexPositionColor.VertexDeclaration,
                                              vertexCount, BufferUsage.WriteOnly);

            var indexCount = IndexCount;
            var indexBuffer = new IndexBuffer(device, typeof(int), indexCount, BufferUsage.WriteOnly);

            SetupInstanceVertexDeclaration();
            
            var instanceCount = this.VoxelCapacity;
            var instanceBuffer = new VertexBuffer(device, InstanceVertexDeclaration,
                                              instanceCount, BufferUsage.WriteOnly);

            Instancing = new InstancingClass(geometryBuffer, instanceBuffer, indexBuffer, Effect);
        }

        private void SetupInstanceVertexDeclaration()
        {
            VertexElement[] instanceStreamElements = new VertexElement[2];
            int offset = 0;
            instanceStreamElements[0] =
                new VertexElement(offset, VertexElementFormat.Vector4,
                    VertexElementUsage.Position, 1);
            offset += sizeof(float) * 4;

            instanceStreamElements[1] =
                    new VertexElement(offset, VertexElementFormat.Color,
                        VertexElementUsage.Color, 1);
            offset += sizeof(byte) * 4;

            InstanceVertexDeclaration = new VertexDeclaration(instanceStreamElements);
        }
        private void SetUpGeometry()
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
                new Vector3(-unitHalfSize, -unitHalfSize, -unitHalfSize) * VoxelScale, 
                new Vector3(unitHalfSize, unitHalfSize, unitHalfSize) * VoxelScale);
            // TODO - make a custom vertex class that is only a position
            var verts = box.GetCorners().Select(x => new VertexPositionColor
            {
                Position = x,
                // this color should never appear
                Color = Color.Purple
            }).ToArray();

            Instancing.GeometryBuffer.SetData(verts);
            Instancing.IndexBuffer.SetData(solidIndices);
        }

    }
}
