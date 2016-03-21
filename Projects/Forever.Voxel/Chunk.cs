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

    public enum ChunkRayTool
    {
        Derez,
        Rez
    }
    public class Chunk
    {
        private readonly int NumberOfDimensions = 3;
        private readonly float Unit = 1f;

        Voxel[][][] Voxels { get; set; }
        int ChunksPerDimension { get; set; }

        InstancingClass Instancing { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 VoxelScale { get; set; }

        public int Capacity { get { return (int)Math.Pow(ChunksPerDimension, NumberOfDimensions); } }

        public Chunk(BoundingBox bb, int chunksPerDimension)
        {
            ChunksPerDimension = chunksPerDimension;
            CreateDefaultChunk();

            Box = bb;
            var diff = (bb.Max - bb.Min);
            Position = bb.Min + ( diff * 0.5f);
            World = Matrix.Identity;
            float hypotenuse = diff.LengthSquared();

            VoxelScale = diff / (float)chunksPerDimension;
        }

        public Chunk(int chunksPerDimension)
        {
            VoxelScale = new Vector3(1f, 1f, 1f);
            ChunksPerDimension = chunksPerDimension;
            CreateDefaultChunk();

            World = Matrix.Identity;
            Position = Vector3.Zero;

            var totalSideLength = ChunksPerDimension;
            var halfSide = totalSideLength / 2f;
            var min = Position + new Vector3(-halfSide, -halfSide, -halfSide);
            var max = Position + new Vector3(halfSide, halfSide, halfSide);
            Box = new BoundingBox(min, max);
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

        Voxel? Get(Vector3 arrayVector)
        {
            return Get((int)arrayVector.X, (int)arrayVector.Y, (int)arrayVector.Z);
        }

        public void Derez(int x, int y, int z)
        {
            Voxels[x][y][z].Derez();
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
                       // if (x == 0 || x == ChunksPerDimension - 1
                       //     || y == 0 || y == ChunksPerDimension - 1
                       //     || z == 0 || z == ChunksPerDimension - 1)
                        {
                            Voxels[x][y][z].State = VoxelState.Active;
                        }

                        // product color block
                        var color = new Color(
                            (float) x / (float) ChunksPerDimension,
                            (float) y / (float) ChunksPerDimension,
                            (float) z / (float) ChunksPerDimension
                            );
                        
                        color = new Color(
                            (float) Random.NextDouble(),
                            (float) Random.NextDouble(),
                            (float) Random.NextDouble()
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
            NeedRebuild = true;
        }

        private void SetupInstancing(GraphicsDevice device)
        {
            var vertexCount = 8;
            var geometryBuffer = new VertexBuffer(device, VertexPositionColor.VertexDeclaration,
                                              vertexCount, BufferUsage.WriteOnly);

            var indexCount = 72;
            var indexBuffer = new IndexBuffer(device, typeof(int), indexCount, BufferUsage.WriteOnly);

            SetupInstanceVertexDeclaration();
            var instanceCount = this.Capacity;
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
            float unit =  (Unit / 2f);
            var box = new BoundingBox(new Vector3(-unit, -unit, -unit) * VoxelScale, new Vector3(unit, unit, unit) * VoxelScale);
            var verts = box.GetCorners().Select(x => new VertexPositionColor
            {
                Position = x,
                // this color should never appear
                Color = Color.Purple
            }).ToArray();

            Instancing.GeometryBuffer.SetData(verts);
            Instancing.IndexBuffer.SetData(solidIndices);
        }

        public int InstanceCount { get; private set; }
        private void RebuildInstanceBuffer()
        {
            // TODO - make it so that you can only send the active blocks
            var instances = new List<Voxel.ViewState>();
            VisitCoords((x, y, z) =>
            {
                var voxel = Get(x, y, z).Value;
                if (voxel.ShouldRender())
                {
                    var viewState = ExtractViewState(voxel, x, y, z);
                    instances.Add(viewState);
                }
            });

            var instanceBufferData = instances.ToArray();
            InstanceCount = instanceBufferData.Count();
            if (InstanceCount > 0)
            {
                Instancing.InstanceBuffer.SetData(0, instanceBufferData, 0, InstanceCount, InstanceVertexDeclaration.VertexStride);
            }
            NeedRebuild = false;
        }
        Random Random = new Random();

        Voxel.ViewState ExtractViewState(Voxel voxel, int x, int y, int z)
        {
            var color = voxel.Material != null ? voxel.Material.Color : Color.Red;
           
            var c = ArrayVector(x, y, z);
            var pos = ArrayToChunk(c) + BlockOffset();
            return new Voxel.ViewState
            {
                Color = color,
                Position = new Vector4(pos.X, pos.Y, pos.Z, 0)
            };
        }

        public BoundingBox Box { get; private set; }

        #endregion

        #region Space Conversions
        Vector3 ArrayNormal(Vector3 v)
        {
            float x = v.X;
            float y = v.Y;
            float z = v.Z;
            /*
            float tip = 0.5f;
            float high = 1f;

            if(v.X < -tip) x = -high;
            if (v.X > tip) x = high;

            if (v.Y < -tip) y = -high;
            if (v.Y > tip) y = high;

            if (v.Z < -tip) z = -high;
            if (v.Z > tip) z = high;
            */

            return new Vector3(x, y, z) * 1.6f;
        }

        Vector3 ArrayVector(int x, int y, int z)
        {
            return new Vector3(x, y, z);
        }
        Vector3 ArrayVector(Vector3 v)
        {
            return new Vector3((int)v.X, (int)v.Y, (int)v.Z);
        }
        Vector3 ChunkToWorld(Vector3 chunkCoord)
        {
            return Vector3.Transform(chunkCoord, World);
        }
        Vector3 WorldToChunk(Vector3 worldCoord)
        {
            return Vector3.Transform(worldCoord, Matrix.Invert(World));
        }
        Vector3 BlockOffset()
        {
            float d = Unit / 2f;
            return new Vector3(d, d, d) * VoxelScale;
        }
        Vector3 ChunkToArray(Vector3 chunkCoord)
        {
            return (chunkCoord / VoxelScale) - Box.Min;
        }
        Vector3 ArrayToChunk(Vector3 arrayCoord)
        {
            return Box.Min + (new Vector3(arrayCoord.X, arrayCoord.Y, arrayCoord.Z) * VoxelScale);
        }

        BoundingBox VoxelBoundingBoxChunkSpace(int x, int y, int z)
        {
            var chunkMin = Box.Min;
            var voxelMin = chunkMin + new Vector3(x, y, z);
            var voxelMax = voxelMin + new Vector3(1, 1, 1);
            return new BoundingBox(voxelMin, voxelMax);
        }
        #endregion


        #region Queries
        public bool ToolRay(Ray ray, ChunkRayTool tool)
        {
            var chunkBox = Box;
            float? test = ray.Intersects(chunkBox);
            if (!test.HasValue) return false;

            float penetration = test.Value;
            var pointOfIntersection = ray.Position + (penetration * ray.Direction); 

            // Getting this far means that the ray is pointing at our box


            // rayPos is the camera position in array coordinates as vector3
            var rayPos = WorldToChunk(pointOfIntersection + (Box.Min));

            rayPos = ChunkToArray(rayPos);
            // rayDirect is a normalized vector indicating the direction the mouse "click" was from the 
            // user's POV
            var rayDirect = WorldToChunk(ray.Direction);
            //rayDirect = ChunkToArray(rayDirect);
            //rayDirect.Normalize();

            // we need to find the array vector of the Chunk.Position


            // testPos will be a vector "index" into the Voxels array, pointing at the 
            // first voxel we would like to check.  We would like to project it onto the outside (or inside of the array);

            Vector3 testPos = rayPos;


            int x, y, z;
            /*
            Indices(testPos, out x, out y, out z);
            float lastLength = float.MaxValue;
            while (!InBound(x, y, z)
                && testPos.LengthSquared() < lastLength)
            {
                lastLength = testPos.LengthSquared();
                testPos += rayDirect;
                Indices(testPos, out x, out y, out z);
            }
            */
            int maxTests = ChunksPerDimension+1;
            int numTests = 0;
            do
            {
                Indices(testPos, out x, out y, out z);
                var voxel = Get(x, y, z);
                if (voxel.HasValue)
                {
                    switch (tool)
                    {
                        case ChunkRayTool.Derez:
                            if (voxel.Value.ShouldRender())
                            {
                                // TODO - this is nasty cheat
                                Voxels[x][y][z].Derez();
                                Invalidate();
                                return true;
                            }
                            break;
                        case ChunkRayTool.Rez:
                            bool fire = false;

                            if (!voxel.Value.ShouldRender())
                            {
                                var nextPos = testPos + rayDirect;
                                var nextVoxel = Get(nextPos);
                                if (nextVoxel.HasValue)
                                {
                                    if(nextVoxel.Value.ShouldRender())
                                    {
                                        // find the face normal and move in that direction in the array
                                        // from nextPos 

                                        int tX, tY, tZ;
                                        Indices(nextPos, out tX, out tY, out tZ);

                                        var faceNormal = ComputeClosestFaceNormal(testPos, rayDirect, tX, tY, tZ);
                                        int dX, dY, dZ;
                                        Indices(faceNormal, out dX, out dY, out dZ);

                                        var targetVoxel = Get(tX + dX, tY + dY, tZ + dZ);
                                        // TODO - Emulate 7 days block placement rules
                                        if (false && targetVoxel.HasValue && !targetVoxel.Value.ShouldRender())
                                        {
                                            Voxels[tX + dX][tY + dY][tZ + dZ].Rez();
                                            Invalidate();
                                            return true;
                                            //fire = true;
                                        }
                                        else
                                        {
                                            fire = true;
                                        }

                                    }
                                }
                                else
                                {
                                    fire = true;
                                }
                            }

                            if (fire)
                            {
                                Voxels[x][y][z].Rez();
                                Invalidate();
                                return true;
                            }
                            break;
                    }

                }

                testPos += rayDirect;
                numTests++;
            } while (numTests < maxTests);

            return false;
        }

        Vector3 ComputeClosestFaceNormal(Vector3 v, Vector3 delta, int x, int y, int z)
        {
            var normals = new Vector3[]
            {
                Vector3.UnitX,
                Vector3.UnitY,
                Vector3.UnitZ,
                -Vector3.UnitX,
                -Vector3.UnitY,
                -Vector3.UnitZ,
            };
            
            Vector3 winner = Vector3.UnitX;
            var center = ArrayVector(x, y, z);
            float closest = float.MaxValue;
            foreach (var normal in normals)
            {
                var dist = ((center + normal) - v).LengthSquared();

                if (dist < closest)
                {
                    closest = dist;
                    winner = normal;
                }
            }

            return winner;
        }

       

        void Indices(Vector3 arrayVector, out int x, out int y, out int z)
        {
            x = (int)arrayVector.X;
            y = (int)arrayVector.Y;
            z = (int)arrayVector.Z;
        }

        public IEnumerable<Voxel> GetVoxels()
        {
            return Voxels.SelectMany<Voxel[][], Voxel[]>(x => x).SelectMany(x => x);
        }
        #endregion
        
        public void Draw(float duration, RenderContext renderContext)
        {
            if (NeedRebuild)
            {
                RebuildInstanceBuffer();
            }

            var wvp = World
                * renderContext.Camera.View
                * renderContext.Camera.Projection;

            var effect = Instancing.Effect;
            effect.CurrentTechnique = effect.Techniques["Instancing"];
            effect.Parameters["WVP"].SetValue(wvp);

            Instancing.Draw(duration, renderContext, InstanceCount);

            Renderer.Render(Box, renderContext.GraphicsDevice, World, renderContext.Camera.View, renderContext.Camera.Projection, Color.Red);
        }

        Matrix World;
        public void Update(float duration)
        {
        }


        #region Voxel State
        bool NeedRebuild = false;
        public void Invalidate()
        {
            NeedRebuild = true;
        }
        #endregion

    }
}
