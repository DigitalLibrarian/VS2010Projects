﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.Render;
using Forever.Render.Instancing;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Forever.Voxel.Meshing;

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
        int VoxelsPerDimension { get; set; }

        IInstancer Instancer { get; set; }

        public Vector3 Position { get; set; }
        public float VoxelSize { get; set; }

        public int Capacity { get { return (int)Math.Pow(VoxelsPerDimension, NumberOfDimensions); } }

        public Chunk(Vector3 position, int voxelsPerDimension, float voxelSize, IInstancer instancer)
        {
            World = Matrix.Identity;
            VoxelsPerDimension = voxelsPerDimension;
            Position = position;
            VoxelSize = voxelSize;
            Instancer = instancer;

            Allocate();
            
            var halfSize = new Vector3(1f, 1f, 1f) * (VoxelsPerDimension * VoxelSize) * 0.5f;
            Box = new BoundingBox(Position - halfSize, Position + halfSize);
        }

        public Chunk(BoundingBox bb, int voxelsPerDimension, IInstancer instancer)
        {
            VoxelsPerDimension = voxelsPerDimension;
            Allocate();

            Box = bb;
            var diff = (bb.Max - bb.Min);
            Position = bb.Min + ( diff * 0.5f);
            VoxelSize = diff.X / (float)voxelsPerDimension;
             World = Matrix.Identity;
            Instancer = instancer;  
        }

        bool InBound(int x, int y, int z)
        {
            return x >= 0 && x < VoxelsPerDimension
                && y >= 0 && y < VoxelsPerDimension
                && z >= 0 && z < VoxelsPerDimension;
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
            for (int x = 0; x < VoxelsPerDimension; x++)
            {
                for (int y = 0; y < VoxelsPerDimension; y++)
                {
                    for (int z = 0; z < VoxelsPerDimension; z++)
                    {
                        visitor(x, y, z);
                    }
                }
            }
        }

       
        #endregion

        #region Graphics Data
        Effect Effect { get; set; }
        public void Initialize(GraphicsDevice device, Effect effect)
        {
            Effect = effect;
            SetupInstancing(device);
            NeedRebuild = true;
        }

        VertexBufferBinding InstanceBufferBinding { get; set; }
        private void SetupInstancing(GraphicsDevice device)
        {
            var instanceCount = this.Capacity;
            var instanceBuffer = new VertexBuffer(device, VD.InstanceVertexDeclaration,
                                              instanceCount, BufferUsage.WriteOnly);

            InstanceBufferBinding = new VertexBufferBinding(instanceBuffer, 0, 1);
        }

        private static class VD
        {
            static VD()
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
            public static VertexDeclaration InstanceVertexDeclaration { get; private set; }
        }
        public int InstanceCount { get; private set; }
        Voxel.ViewState[] InstanceBuffer { get; set; }
        private void RebuildInstanceBuffer(Ray cameraRay)
        {
            int next = 0;
            VisitCoords((x, y, z) =>
            {
                var voxel = Get(x, y, z).Value;
                if (voxel.ShouldRender())
                {
                    if (!IsOccluded(x, y, z, cameraRay))
                    {
                        var viewState = ExtractViewState(x, y, z);
                        InstanceBuffer[next++] = viewState;
                    }
                }
            });
           
            InstanceCount = next;
            if (InstanceCount > 0)
            {
                InstanceBufferBinding.VertexBuffer.SetData(0, InstanceBuffer, 0, InstanceCount, VD.InstanceVertexDeclaration.VertexStride);
            }
            NeedRebuild = false;
        }
        Random Random = new Random();
        static Material DefaultMaterial = new Material(Color.AliceBlue);

        static Vector3[] VoxelFaceNormals = {  
                Vector3.UnitX,
                Vector3.UnitY, 
                Vector3.UnitZ,
                -Vector3.UnitX,
                -Vector3.UnitY, 
                -Vector3.UnitZ
                                     };
                                  
        bool IsOccluded(int x, int y, int z, Ray cameraRay)
        {
            var camVector = this.ArrayToWorld(this.ArrayVector(x, y, z)) - cameraRay.Position;
            camVector.Normalize();
            Vector3 d;
            for(int i = 0; i < VoxelFaceNormals.Length;i++)
            {
                d = VoxelFaceNormals[i];
                
                // is normal facing camera
                // TODO 
                //if (Vector3.Dot(d, camVector) < 0)
                {
                    int dx, dy, dz;
                    Indices(d, out dx, out dy, out dz);
                
                    int tx = x + dx, ty = y + dy, tz = z + dz;
                    // if i can step off the edge of the chunk, then i am not occluded
                    // if voxel the in the face direction is not being drawn then i am not occluded
                    if (!InBound(tx, ty, tz) || !Get(tx, ty, tz).Value.ShouldRender())
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

            var pos = ArrayToWorld(ArrayVector(x, y, z));
            return new Voxel.ViewState
            {
                Color = Voxels[x][y][z].Material.Color,
                Position = new Vector4(pos.X, pos.Y, pos.Z, 0f)
            };
        }

        public BoundingBox Box { get; private set; }

        #endregion

        #region Space Conversions

        public Vector3 ArrayVector(int x, int y, int z)
        {
            return new Vector3(x, y, z);
        }
        public Vector3 ArrayVector(Vector3 v)
        {
            return new Vector3((int)v.X, (int)v.Y, (int)v.Z);
        }
       
        private Vector3 BlockOffset()
        {
            float d = Unit * 0.5f;
            return new Vector3(d, d, d) * VoxelSize;
        }
        public Vector3 ChunkToArray(Vector3 chunkCoord)
        {
            return (chunkCoord - Box.Min) / VoxelSize;
        }
        public Vector3 ArrayToWorld(Vector3 arrayCoord)
        {
            return Box.Min + ((arrayCoord * VoxelSize) + BlockOffset());
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
            var rayPos = pointOfIntersection;

            rayPos = ChunkToArray(rayPos);
            // rayDirect is a normalized vector indicating the direction the mouse "click" was from the 
            // user's POV
            var rayDirect = ray.Direction;

            // testPos will be a vector "index" into the Voxels array
            Vector3 testPos = rayPos;
            
            int x, y, z;
          
            int maxTests = VoxelsPerDimension+1;
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
                                var nextPos = testPos + rayDirect * VoxelSize;
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

                testPos += rayDirect*VoxelSize;
                numTests++;
            } while (numTests < maxTests);

            return false;
        }

       

        void Indices(Vector3 arrayVector, out int x, out int y, out int z)
        {
            x = (int)arrayVector.X;
            y = (int)arrayVector.Y;
            z = (int)arrayVector.Z;
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
            
            var camPos = rc.Camera.Position;
            var lightPos = new Vector3(-200, 200, -200);
            float distance = (Position - lightPos).LengthSquared();

            Effect.CurrentTechnique = Effect.Techniques["Instancing"];
            Effect.Parameters["WVP"].SetValue(wvp);

            Effect.Parameters["World"].SetValue(World);
            Effect.Parameters["View"].SetValue(rc.Camera.View);
            Effect.Parameters["Proj"].SetValue(rc.Camera.Projection);

            Effect.Parameters["VoxelScale"].SetValue(VoxelSize);

            Effect.Parameters["CameraPos"].SetValue(camPos);
            Effect.Parameters["LightPosition"].SetValue(lightPos);
            Effect.Parameters["LightDistanceSquared"].SetValue(distance);
            float intensity = 0.1f;
            Effect.Parameters["LightDiffuseColorIntensity"].SetValue(new Color(intensity, intensity, intensity).ToVector3());
            Effect.Parameters["DiffuseColor"].SetValue(Color.White.ToVector3());

            Effect.CurrentTechnique.Passes[0].Apply();
            Instancer.Draw(rc, InstanceBufferBinding, InstanceCount);
        }

        public Matrix World;

        #region Voxel State
        bool NeedRebuild = false;
        public void Invalidate()
        {
            NeedRebuild = true;
        }
        #endregion

        void Allocate(Action<int, int, int> initialVisitor = null)
        {
            Voxels = new Voxel[VoxelsPerDimension][][];
            for (int x = 0; x < VoxelsPerDimension; x++)
            {
                Voxels[x] = new Voxel[VoxelsPerDimension][];
                for (int y = 0; y < VoxelsPerDimension; y++)
                {
                    Voxels[x][y] = new Voxel[VoxelsPerDimension];
                    if (initialVisitor != null)
                    {
                        for (int z = 0; z < VoxelsPerDimension; z++)
                        {
                            initialVisitor(x, y, z);
                        }
                    }
                }
            }

            InstanceBuffer = new Voxel.ViewState[VoxelsPerDimension * VoxelsPerDimension * VoxelsPerDimension];
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
