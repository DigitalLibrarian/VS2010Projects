using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Forever.Voxel.SVO;
using Forever.SpacePartitions;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Forever.Render;

namespace Forever.Voxel.World
{
    public interface IVolumeChunk
    {
        Matrix World { get; }
        int Size { get; }

        int InstanceCount { get; }
        Voxel.ViewState[] Instances { get; }

        void Allocate(Action<int, int, int> initialVisitor = null);
        void Deallocate();

        int RebuildInstances(ReferencePoint reference);

        void SetVoxel(int x, int y, int z, Voxel voxel);
    }

    public enum VolumeCellState
    {
        UnAllocated,
        Allocated,
        Unloaded,
        Loaded
    }

    public class VolumeCell
    {
        public IVolumeChunk Chunk { get; set; }
        public bool HasChunk { get { return Chunk != null; } }
        public bool IsRenderLeaf { get; set; }
        public bool NeedsRebuild { get; set; }
        public VolumeCellState State { get; set; }

        void Allocate(Action<int, int, int> initialVisitor = null)
        {
            AssertState(VolumeCellState.UnAllocated);
            Chunk.Allocate(initialVisitor);
            SetState(VolumeCellState.Allocated);
        }

        void Deallocate()
        {
            AssertNotState(VolumeCellState.UnAllocated);
            Chunk.Deallocate();
            SetState(VolumeCellState.UnAllocated);
        }

        void AssertState(VolumeCellState expected) { }
        void AssertNotState(VolumeCellState betterNot) { }
        void SetState(VolumeCellState newState)
        {
            State = newState;
        }
        
        public void LoadFromSampler(IVoxelVolume volume, Vector3 origin, IVoxelSampler sampler)
        {
            if (State == VolumeCellState.UnAllocated)
            {
                Allocate((x, y, z) => 
                    Chunk.SetVoxel(x, y, z, sampler.GetSample(
                        origin + new Vector3(
                            x * volume.VoxelSize, 
                            y * volume.VoxelSize, 
                            z * volume.VoxelSize), 
                        volume.VoxelSize
                    )));
            }
        }

    }

    public interface IVoxelSampler
    {
        Voxel GetSample(Vector3 samplePoint, float voxelScale);
        Voxel GetSample(float x, float y, float z, float voxelScale);
    }

    public class NullVoxelSampler : IVoxelSampler
    {
        public Voxel GetSample(float x, float y, float z, float voxelScale)
        {
            return new Voxel();
        }

        public Voxel GetSample(Vector3 samplePoint, float voxelScale)
        {
            return GetSample(samplePoint.X, samplePoint.Y, samplePoint.Z, voxelScale);
        }
    }

    public interface IVoxelEffect
    {
        Matrix WVP { get; set; }
        Vector3 CameraPosition { get; set; }
        float VoxelScale { get; set; }

        Vector3 LightPos { get; set; }
        float LightDistanceSquared { get; set; }

        Color LightDiffuseColorIntensity { get; set; }
        Color DiffuseColor { get; set; }
        
        EffectTechnique CurrentTechnique { get; }
    }

    public interface IVoxelVolume
    {
        int ChunkResolution { get; }
        int ChunksPerSide { get; }
        float VoxelSize { get; }
        float ChunkSize { get; }
        float Side { get; }

        ReferencePoint Reference { get; }
    }

    public class ReferencePoint
    {
        public Vector3 Position;
        public Ray Ray;
        public BoundingFrustum Frustum;
    }

    public class VolumeViewer : IVoxelVolume
    {
        public ReferencePoint Reference { get; private set; }

        VertexBuffer InstanceBuffer { get; set; }
        VertexBuffer GeometryBuffer { get; set; }
        IndexBuffer IndexBuffer { get; set; }

        BoundingBox Box { get; set; }

        public int ChunkResolution { get; private set; }
        public int ChunksPerSide { get; private set; }
        public float VoxelSize { get; private set; }
        int TreeDepth { get; set; }

        public float ChunkSize { get { return ChunkResolution * VoxelSize; } }
        public float Side { get { return ChunkSize * ChunksPerSide; } }
        
        // We want to be able to render any sub volume.  This means caching vertex buffers at each level of
        // the tree and tracking whether it is the active depth or not (IsRenderLeaf).  This will probably
        // become a dedicated LOD tree
        
        OctTree<VolumeCell> Tree { get; set; }
        IVoxelSampler Sampler { get; set; }

        IVoxelEffect Effect { get; set; }
        IList<IVolumeChunk> RenderSet { get; set; }

        VertexBufferBinding[] Bindings { get; set; }
        
        public VolumeViewer(IVoxelSampler sampler, IVoxelEffect effect,
            int chunksPerSide = 16, int chunkResolution = 32, float voxelSize = 1f)
        {
            Sampler = sampler;
            Effect = effect;
            ChunkResolution = chunkResolution;
            VoxelSize = voxelSize;
            ChunksPerSide = chunksPerSide;

            // create box centered at {0,0,0} and as big as the cube of chunks of voxels we need
            float half = Side / 2f;
            Box = new BoundingBox(new Vector3(-half, -half, -half), new Vector3(half, half, half));
            // TODO - make tree one level deeper and use ray tracing down to voxels
            TreeDepth = (int)Math.Log((Side / ChunkSize), 2);

            Tree = OctTree<VolumeCell>.CreatePreSubdivided(TreeDepth, Box);
            foreach (var leaf in Tree.GetLeaves())
            {
                leaf.Value = new VolumeCell();
                leaf.Value.LoadFromSampler(this, leaf.Box.Min, Sampler);
            }

            RenderSet = new List<IVolumeChunk>();

            Effect.VoxelScale = VoxelSize;
            Bindings = new VertexBufferBinding[2];
            Bindings[0] = new VertexBufferBinding(GeometryBuffer);
        }

        public void UpdateReferencePoint(Vector3 p, Ray r, BoundingFrustum frustum)
        {
            this.Reference.Position = p;
            this.Reference.Ray = r;
            this.Reference.Frustum = frustum;
        }

        public void Draw(RenderContext rc)
        {
            rc.GraphicsDevice.SetVertexBuffers(Bindings);

            var viewProj = rc.Camera.View * rc.Camera.Projection;
            Effect.CameraPosition = rc.Camera.Position;
            foreach (var chunk in RenderSet)
            {
                Effect.WVP = chunk.World * viewProj;
                rc.GraphicsDevice.Indices = this.IndexBuffer;


                foreach (var pass in Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    rc.GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 8, 0, 24, chunk.InstanceCount);
                }
            }
        }

        public void Update(GameTime gameTime)
        {

        }
    }
}
