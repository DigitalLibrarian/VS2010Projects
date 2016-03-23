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
    public class Chunk : IDisposable
    {
        private readonly int NumberOfDimensions = 3;
        private readonly float Unit = 1f;

        public Voxel[][][] Voxels { get; set; }
        int ChunksPerDimension { get; set; }

        InstancingClass Instancing { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 VoxelScale { get; set; }

        public int Capacity { get { return (int)Math.Pow(ChunksPerDimension, NumberOfDimensions); } }

        public Chunk(BoundingBox bb, int chunksPerDimension)
        {
            ChunksPerDimension = chunksPerDimension;
            Allocate();

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
            Allocate();

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
        public void VisitCoords(Action<int, int, int> visitor)
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

        public int InstanceCount { get; private set; }
        public int TotalOcclusions { get; private set; }
        Voxel.ViewState[] InstanceBuffer { get; set; }
        private void RebuildInstanceBuffer(Ray cameraRay)
        {
            int next = 0;
            var occlusions = 0;
            VisitCoords((x, y, z) =>
            {
                var voxel = Get(x, y, z).Value;
                if (voxel.ShouldRender())
                {
                    if (!IsOccluded(x, y, z, cameraRay))
                    {
                        var viewState = ExtractViewState(x, y, z);
                        //instances.Add(viewState);
                        InstanceBuffer[next++] = viewState;
                    }
                    else
                    {
                        occlusions++;
                    }
                }
            });
            TotalOcclusions = occlusions;
           
            InstanceCount = next;
            if (InstanceCount > 0)
            {
                Instancing.InstanceBuffer.SetData(0, InstanceBuffer, 0, InstanceCount, InstanceVertexDeclaration.VertexStride);
            }
            NeedRebuild = false;
        }
        Random Random = new Random();
        Material DefaultMaterial = new Material(Color.AliceBlue);

        Vector3[] VoxelFaceNormals = {  
                Vector3.UnitX,
                Vector3.UnitY, 
                Vector3.UnitZ,
                -Vector3.UnitX,
                -Vector3.UnitY, 
                -Vector3.UnitZ
                                     };

        bool IsOccluded(int x, int y, int z, Ray cameraRay)
        {
            var camVector = cameraRay.Position + cameraRay.Direction;
            foreach(var d in VoxelFaceNormals)
            {
                int dx, dy, dz;
                Indices(d, out dx, out dy, out dz);
                
                int tx = x + dx, ty = y + dy, tz = z + dz;
                // TODO - if it is a face voxel, then the direction 
                // is one of the faces it participates in.  For such a voxel,
                // it should only be drawn if one of the normals faces the camera
                if (!InBound(tx, ty, tz))
                {
                    if (Vector3.Dot(d, camVector) > 0)
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Get(tx, ty, tz).Value.ShouldRender())
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        Voxel.ViewState ExtractViewState(int x, int y, int z)
        {
            if (Voxels[x][y][z].Material == null)
            {
                Voxels[x][y][z].Material = DefaultMaterial;
            }

            var material = Voxels[x][y][z].Material;
            var c = ArrayVector(x, y, z);
            var pos = ArrayToChunk(c) + BlockOffset();
            return new Voxel.ViewState
            {
                Color = material.Color,
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
            return new Vector3(x, y, z);
        }

        public Vector3 ArrayVector(int x, int y, int z)
        {
            return new Vector3(x, y, z);
        }
        public Vector3 ArrayVector(Vector3 v)
        {
            return new Vector3((int)v.X, (int)v.Y, (int)v.Z);
        }
        public Vector3 ChunkToWorld(Vector3 chunkCoord)
        {
            return Vector3.Transform(chunkCoord, World);
        }
        public Vector3 WorldToChunk(Vector3 worldCoord)
        {
            return Vector3.Transform(worldCoord, Matrix.Invert(World));
        }
        public Vector3 BlockOffset()
        {
            float d = Unit / 2f;
            return new Vector3(d, d, d) * VoxelScale;
        }
        public Vector3 ChunkToArray(Vector3 chunkCoord)
        {
            return (chunkCoord / VoxelScale) - Box.Min;
        }
        public Vector3 ArrayToChunk(Vector3 arrayCoord)
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
            // Getting this far means that the ray is pointing at our box

            float penetration = test.Value;
            var pointOfIntersection = ray.Position + (penetration * ray.Direction); // where did it touch you?

            // rayPos is the camera position in array coordinates as vector3
            var rayPos = WorldToChunk(pointOfIntersection + (Box.Min));

            rayPos = ChunkToArray(rayPos);
            // rayDirect is a normalized vector indicating the direction the mouse "click" was from the 
            // user's POV
            var rayDirect = WorldToChunk(ray.Direction);

            // testPos will be a vector "index" into the Voxels array
            Vector3 testPos = rayPos;
            
            int x, y, z;
          
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
                                Voxels[x][y][z].Derez();
                                Invalidate();
                                return true;
                            }
                            break;
                        case ChunkRayTool.Rez:
                            bool fire = false;

                            if (!voxel.Value.ShouldRender())
                            {
                                // can only place by butting up against another block
                                // (or the edge of a chunk);
                                var nextPos = testPos + rayDirect;
                                var nextVoxel = Get(nextPos);
                                if (nextVoxel.HasValue)
                                {
                                    if(nextVoxel.Value.ShouldRender())
                                    {
                                        // TODO - Emulate 7 days block placement rules
                                        fire = true;
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

        public Vector3? ProjectToArray(Ray ray)
        {
            var chunkBox = Box;
            float? test = ray.Intersects(chunkBox);
            if (!test.HasValue) return null;

            float penetration = test.Value;
            var pointOfIntersection = ray.Position + (penetration * ray.Direction); // where did it touch you?
            var rayPos = WorldToChunk(pointOfIntersection + (Box.Min));
            rayPos = ChunkToArray(rayPos);
            var rayDirect = WorldToChunk(ray.Direction);

            return rayPos;
        }

        public void ArrayVisit(Vector3 start, Vector3 increment, Func<Vector3, bool> callback)
        {
            // testPos will be a vector "index" into the Voxels array
            Vector3 testPos = start;

            int maxTests = ChunksPerDimension + 1;
            int numTests = 0;
            int x, y, z;
            do
            {
                Indices(testPos, out x, out y, out z);
                if (InBound(x, y, z) && !callback(testPos))
                {
                    break;
                }
                
                testPos += increment;
                numTests++;
            } while (numTests < maxTests);
        }


        Vector3 ComputeClosestFaceNormal(Vector3 v, Vector3 delta, int x, int y, int z)
        {
            Vector3 winner = Vector3.UnitX;
            var center = ArrayVector(x, y, z);
            float closest = float.MaxValue;
            foreach (var normal in VoxelFaceNormals)
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
        
        public void Draw(float duration, RenderContext rc)
        {
            if (NeedRebuild)
            {
                RebuildInstanceBuffer(rc.GetCameraRay());
            }

            var wvp = World
                * rc.Camera.View
                * rc.Camera.Projection;

            var effect = Instancing.Effect;
            effect.CurrentTechnique = effect.Techniques["Instancing"];
            effect.Parameters["WVP"].SetValue(wvp);

            Instancing.Draw(duration, rc, InstanceCount);

            //Renderer.Render(Box, renderContext.GraphicsDevice, World, renderContext.Camera.View, renderContext.Camera.Projection, Color.Red);
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

        void Allocate()
        {
            Voxels = new Voxel[ChunksPerDimension][][];
            for (int x = 0; x < ChunksPerDimension; x++)
            {
                Voxels[x] = new Voxel[ChunksPerDimension][];
                for (int y = 0; y < ChunksPerDimension; y++)
                {
                    Voxels[x][y] = new Voxel[ChunksPerDimension];
                }
            }

            InstanceBuffer = new Voxel.ViewState[ChunksPerDimension * ChunksPerDimension * ChunksPerDimension];
        }

        void Deallocate()
        {
            Voxels = null;
            InstanceBuffer = null;
        }
        public void Dispose()
        {
            Deallocate();
            // TODO - might wanna force GC.....probably not tho
        }
    }
}
