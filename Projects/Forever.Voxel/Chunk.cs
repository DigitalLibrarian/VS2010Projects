using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.Render;
using Forever.Render.Instancing;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace Forever.Voxel
{
    public class Chunk
    {
        private readonly int NumberOfDimensions = 3;
        private readonly float Unit = 1f;

        Voxel[][][] Voxels { get; set; }
        int ChunksPerDimension { get; set; }

        InstancingClass Instancing { get; set; }

        public int Count { get { return (int)Math.Pow(ChunksPerDimension, NumberOfDimensions); } }

        public Chunk(int chunksPerDimension)
        {
            ChunksPerDimension = chunksPerDimension;
            CreateDefaultChunk();
        }

        public Chunk(Voxel[][][] voxels, int chunkSize)
        {
            Voxels = voxels;
            ChunksPerDimension = chunkSize;
        }

        bool InBound(int x, int y, int z)
        {
            return x >= 0 && x < ChunksPerDimension
                && y >= 0 && y < ChunksPerDimension
                && z >= 0 && z < ChunksPerDimension;
        }

        Voxel? Get(int x, int y, int z)
        {
            if (!InBound(x, y, z)) return null;
            return Voxels[x][y][z];
        }

        #region Chunk Generation
        void VisitCoords(Action<int, int, int> visitor)
        {
            for (int x = 0; x < ChunksPerDimension; x++)
            {
                for (int y = 0; y < ChunksPerDimension; y++)
                {
                    for (int z = 0; z < ChunksPerDimension; z++)
                    {
                        visitor(x, y, z);
                    }
                }
            }
        }

        void CreateDefaultChunk()
        {
            Voxels = new Voxel[ChunksPerDimension][][];

            for (int x = 0; x < ChunksPerDimension; x++)
            {
                Voxels[x] = new Voxel[ChunksPerDimension][];
                for (int y = 0; y < ChunksPerDimension; y++)
                {
                    Voxels[x][y] = new Voxel[ChunksPerDimension];
                    for (int z = 0; z < ChunksPerDimension; z++)
                    {
                        Voxels[x][y][z].State = VoxelState.Active;

                        // product color block
                        var color = new Color(
                            (float) x / (float) ChunksPerDimension,
                            (float) y / (float) ChunksPerDimension,
                            (float) z / (float) ChunksPerDimension
                            );
                        Voxels[x][y][z].Material = new Material(color);
                    }
                }
            }
        }
        #endregion

        #region Graphics Data
        private readonly string EffectName = "Effects\\VoxelEffect";
        Effect Effect { get; set; }
        public void LoadContent(ContentManager content)
        {
            Effect = content.Load<Effect>(EffectName);
        }

        public void Initialize(GraphicsDevice device)
        {
            SetupInstancing(device);
            SetUpGeometry();
            RebuildInstanceBuffer();
        }

        private void SetupInstancing(GraphicsDevice device)
        {
            var vertexCount = 8;
            var geometryBuffer = new VertexBuffer(device, VertexPositionColor.VertexDeclaration,
                                              vertexCount, BufferUsage.WriteOnly);

            var indexCount = 72;
            var indexBuffer = new IndexBuffer(device, typeof(int), indexCount, BufferUsage.WriteOnly);

            SetupInstanceVertexDeclaration();
            var instanceCount = this.Count;
            var instanceBuffer = new VertexBuffer(device, InstanceVertexDeclaration,
                                              instanceCount, BufferUsage.WriteOnly);

            Instancing = new InstancingClass(geometryBuffer, instanceBuffer, indexBuffer, InstanceVertexDeclaration, Effect);
        }
        VertexDeclaration InstanceVertexDeclaration { get; set; }
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
            float unit = Unit/2f;
            var box = new BoundingBox(new Vector3(-unit, -unit, -unit), new Vector3(unit, unit, unit));
            var verts = box.GetCorners().Select(x => new VertexPositionColor
            {
                Position = x,
                // this color should never appear
                Color = Color.Purple
            }).ToArray();

            Instancing.GeometryBuffer.SetData(verts);
            Instancing.IndexBuffer.SetData(solidIndices);
        }

        int InstanceCount { get; set; }
        private void RebuildInstanceBuffer()
        {
            // TODO - make it so that you can only send the active blocks
            var instances = new List<Voxel.ViewState>();
            VisitCoords((x, y, z) =>
            {
                //if (Random.Next(2) == 0) return;
                var viewState = ExtractViewState(x, y, z);
                instances.Add(viewState);
            });

            var instanceBufferData = instances.ToArray();
            InstanceCount = instances.Count();
            Instancing.InstanceBuffer.SetData(0, instanceBufferData, 0, InstanceCount, InstanceVertexDeclaration.VertexStride);
        }
        Random Random = new Random();

        Voxel.ViewState ExtractViewState(Voxel voxel, int x, int y, int z)
        {
            var color = voxel.Material != null ? voxel.Material.Color : Color.Red;
           
            var c = ChunkVector(x, y, z);
            var origin = Box.Min;
            float d = Unit/2f;
            var pos = origin + new Vector3(c.X + d, c.Y + d, c.Z + d);
            return new Voxel.ViewState
            {
                Color = color,
                Position = new Vector4(pos.X, pos.Y, pos.Z, 1)
            };
        }
        Voxel.ViewState ExtractViewState(int x, int y, int z)
        {
            var voxel = Get(x, y, z);
            return ExtractViewState(voxel.Value, x, y, z);
        }

        public BoundingBox Box
        {
            get
            {
                var center = Vector3.Zero;
                var totalSideLength = ChunksPerDimension;
                var halfSide = totalSideLength / 2f;
                var min = center + new Vector3(-halfSide, -halfSide, -halfSide);
                var max = center + new Vector3(halfSide, halfSide, halfSide);
                return new BoundingBox(min, max);
            }
        }

        #endregion

        #region Space Conversions
        Vector3 ChunkVector(int x, int y, int z)
        {
            return new Vector3(x, y, z);
        }
        Vector3 ChunkToWorld(Vector3 chunkCoord)
        {
            return chunkCoord;
        }
        Vector3 WorldToChunk(Vector3 worldCoord)
        {
            return worldCoord;
        }
        #endregion

        public IEnumerable<Voxel> GetVoxels()
        {
            return Voxels.SelectMany<Voxel[][], Voxel[]>(x => x).SelectMany(x => x);
        }

        
        float rot = 0.0f;
        public void Draw(float duration, RenderContext renderContext)
        {
            var world = Matrix.CreateRotationY(rot += 0.001f);

            var wvp = world
                * renderContext.Camera.View
                * renderContext.Camera.Projection;

            var effect = Instancing.Effect;
            effect.CurrentTechnique = effect.Techniques["Instancing"];
            effect.Parameters["WVP"].SetValue(wvp);

            Instancing.Draw(duration, renderContext, InstanceCount);

            Renderer.Render(Box, renderContext.GraphicsDevice, world, renderContext.Camera.View, renderContext.Camera.Projection, Color.Red);
        }
    }
}
