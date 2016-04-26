using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Forever.Voxel;
using Microsoft.Xna.Framework;
using Forever.Render;
using Aquarium.Ui;
using Forever.Screens;
using Forever.Extensions;
using Microsoft.Xna.Framework.Graphics;
using Aquarium.UI;

using LibNoise;
using Forever.Voxel.SVO;
using Forever.SpacePartitions;
using Microsoft.Xna.Framework.Input;
using Forever.Voxel.Meshing;

namespace Aquarium
{
    class VoxelScreen : FlyAroundGameScreen
    {
        const int VoxelsPerDimension = 8;

        LabelUiElement DebugLabel { get; set; }

        Perlin Perlin = null;

        OctTree<bool> LoadingTree { get; set; }
        OctTree<Chunk> ChunkTree { get; set; }

        int MaxTreeDepth { get; set; }
        int RenderDepth { get; set; }

        Effect VoxelEffect { get; set; }
        private readonly string EffectName = "Effects\\VoxelEffect";

        IInstancer Instancer { get; set; }

        public override void LoadContent()
        {
            base.LoadContent();
            SetupPerlin();

            VoxelEffect = ScreenManager.Game.Content.Load<Effect>(EffectName);

            var spawnHeightAboveGround = 30 * VoxelsPerDimension;
            var spawnPoint = new Vector3(0, GetHeight(0, 0) + spawnHeightAboveGround, 0);
            Ui.Elements.AddRange(CreateUILayout());

            MaxTreeDepth = 5;
            RenderDepth = -1;

            var s = (float)VoxelsPerDimension;
            float worldSize = (s * (float) System.Math.Pow(2, MaxTreeDepth));

            var treeBox = new BoundingBox(
                spawnPoint + new Vector3(-worldSize, -worldSize, -worldSize),
                spawnPoint + new Vector3(worldSize, worldSize, worldSize));
            LoadingTree = OctTree<bool>.CreatePreSubdivided(MaxTreeDepth, treeBox);
            ChunkTree = OctTree<Chunk>.CreatePreSubdivided(MaxTreeDepth, treeBox);

            var diff = treeBox.Max - treeBox.Min;
            var startPos = Vector3.Zero;
            startPos = ProjectToHeight(startPos, ChunkTree.Root.Box.Max.Y);
            User.Body.Position = startPos;
            User.Body.Orientation = Quaternion.CreateFromYawPitchRoll(0f, (float)System.Math.PI/-2f, 0f);

            for (int i = 0; i < MaxTreeDepth; i++)
            {
                LoadingTree.VisitLeaves(node =>
                {
                    var c = node.Box.Min;
                    var h = GetHeight(c.X, c.Z);
                    if (c.Y > h && !node.Value)
                    {
                        node.Value = true;
                        if (node.Parent != null && node.Parent.SearchFirstChild(x => !x.Value) == null)
                        {
                            node.Parent.Prune();
                        }
                    }
                });
            }

            Instancer = new CubeInstancing(ScreenManager.Game.GraphicsDevice, 2f);
        }
        
        Chunk ChunkFactory(BoundingBox bb)
        {
            var chunk = new Chunk(bb, VoxelsPerDimension, Instancer);
            chunk.Initialize(RenderContext.GraphicsDevice, VoxelEffect);
            var pos = bb.Min;
            chunk.VisitCoords((x, y, z) => {
                var world = chunk.ArrayToChunk(new Vector3(x, y, z));
                float tX = (pos.X + (x * VoxelsPerDimension));
                float tY = (pos.Y + (y * VoxelsPerDimension));
                float tZ = (pos.Z + (z * VoxelsPerDimension));

                chunk.Voxels[x][y][z].Material = new Material(
                    new Color(
                       (float) x / VoxelsPerDimension,
                       (float) y / VoxelsPerDimension,
                       (float) z / VoxelsPerDimension
                        )
                    );

                float height = GetHeight(world.X, world.Z);
                bool active = world.Y < height;
                chunk.Voxels[x][y][z].State = active ? VoxelState.Active : VoxelState.Inactive;
            });
            
            return chunk;
        }
        void SetupPerlin()
        {
            NoiseQuality quality = NoiseQuality.High;
            int seed = 3;
            int octaves = 8;
            double frequency = 0.0001;
            double lacunarity = 0.0;
            double persistence = 1;

            var module = new Perlin();
            module.Frequency = frequency;
            module.NoiseQuality = quality;
            module.Seed = seed;
            module.OctaveCount = octaves;
            module.Lacunarity = lacunarity;
            module.Persistence = persistence;
            Perlin = module;
        }

        float GetHeight(float x, float z)
        {
            int maxHeight = 1000 * VoxelsPerDimension;
            float half = maxHeight / 2f;
            var bottomLeft = new Vector3(-half, -half, -half);
            var scale = 0.9f;
            float n = SmoothNoise(x * scale, z * scale);
            var threshold = bottomLeft.Y + half + (n * half);
            return threshold;
        }

        public float SmoothNoise(float x, float y)
        {
            return (float) (Perlin.GetValue((double)x, (double)y, 10));
        }

        List<IUiElement> CreateUILayout()
        {
            var spriteFont = ScreenManager.Font;
            DebugLabel = new LabelUiElement(RenderContext, spriteFont, DebugLabelStrip());

            return new List<IUiElement>{
                DebugLabel
            };
        }

        public override void HandleInput(InputState input)
        {
            if (input.CurrentMouseState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed
                && input.LastMouseState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                var mousePoint = input.CurrentMousePoint.ToVector2();
                var ray = RenderContext.GetScreenRay(mousePoint);
                ShootChunksBigLaser(ray, ChunkRayTool.Derez, 1);
            }
            else if (input.CurrentMouseState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                var mousePoint = input.CurrentMousePoint.ToVector2();
                var ray = RenderContext.GetScreenRay(mousePoint);
                ShootChunksOctTree(ray, ChunkRayTool.Derez, true);
            }

            if (input.CurrentMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released
                && input.LastMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                var mousePoint = input.CurrentMousePoint.ToVector2();
                var ray = RenderContext.GetScreenRay(mousePoint);
                ShootChunksOctTree(ray, ChunkRayTool.Rez, false);
            }

            if(input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.K))
            {
                ToggleDebug();
            }

            if (input.IsNewKeyPress(Keys.OemPlus))
            {
                if (RenderDepth < MaxTreeDepth) RenderDepth++;
            }

            if (input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.OemMinus))
            {
                if (RenderDepth > -1) RenderDepth--;
            }

            base.HandleInput(input);
        }

        public void ShootChunksOctTree(Ray ray, ChunkRayTool tool, bool closestFirst)
        {
            var seq = ChunkTree.RayCast(ray);

            if (!closestFirst)
            {
                // TODO - fix this
                seq = seq.Reverse();
            }

            foreach (var node in seq)
            {
                if (node.Value != null)
                {
                    if (node.Value.ToolRay(ray, tool))
                    {
                        return;
                    }
                }
            }
        }

        public void ShootChunksBigLaser(Ray ray, ChunkRayTool tool, int halfSize)
        {
            var up = RenderContext.Camera.Up;
            var right = RenderContext.Camera.Right;
            for (int x = -halfSize; x < halfSize; x++)
            {
                for (int y = -halfSize; y < halfSize; y++)
                {
                    for(int z = -halfSize; z < halfSize; z++)
                    {
                        var offset = new Vector3(x, y, z);
                        var p = ray.Position + offset;
                        var r = new Ray(p, ray.Direction);
                        ShootChunksOctTree(r, tool, true);
                    }
                }
            }
        }
        Chunk RenderingChunk;
        public override void Draw(GameTime gameTime)
        {
            var duration = gameTime.GetDuration();

            int count = 0;
            int capacity = 0;

            var numChunksXZ = 4000;
            var numChunksY = 1000;
            var halfSize = new Vector3(numChunksXZ/2f, numChunksY/2f, numChunksXZ/2f) * VoxelsPerDimension;
            var pos = RenderContext.Camera.Position;
            var sphere = new BoundingBox(pos - halfSize, pos + halfSize);

            int numRendered = 0;
            ChunkTree.VisitLeaves(leaf =>
            {
                if (leaf.Value != null 
                    && sphere.Contains(leaf.Value.Box) != ContainmentType.Disjoint 
                    && RenderContext.InView(leaf.Value.Box))
                {

                    this.RenderingChunk = leaf.Value;
                    count += RenderingChunk.InstanceCount;
                    capacity += RenderingChunk.Capacity;

                    RenderingChunk.Draw(duration, RenderContext);
                    numRendered++;
                    if (debug)
                    {
                        Renderer.Render(RenderingChunk.Box, RenderContext.GraphicsDevice, RenderingChunk.World, RenderContext.Camera.View, RenderContext.Camera.Projection, Color.DarkSalmon);
                    }

                }
            });

            if (RenderDepth >= 0)
            {
                LoadingTree.VisitAtDepth(node =>
                {
                    Renderer.Render(RenderContext, node.Box, node.Value ? Color.AntiqueWhite : Color.BlueViolet);
                }, RenderDepth);
            }

            if (TestRay.HasValue)
            {
                DebugDrawer.DrawLine(TestRay.Value.Position, TestRay.Value.Position + (TestRay.Value.Direction * ChunkTree.Root.Box.GetHalfSize().Y * 2 ), Color.LimeGreen);
                DebugDrawer.Draw(gameTime);
            }

            if (TestBox.HasValue)
            {
                Renderer.Render(RenderContext, TestBox.Value, Color.Yellow);
            }

            DebugLabel.Label = string.Format("Chunks Rendered : {0}. Voxels Rendered {1}. Volume Rendered {2}.", numRendered, count, capacity);
            base.Draw(gameTime);
        }
        bool debug;
        void ToggleDebug()
        {
            debug = !debug;
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // fill out space around the player
            SceneLoad();
        }




        #region Scene Loading
        IEnumerator<Vector3> LoadSequence { get; set; }
        IEnumerable<Vector3> SceneLoadSequence_CameraBelowGround(Vector3 pos, int numChunks)
        {
            for (int x = -numChunks; x < numChunks; x++)
            {
                for (int z = -numChunks; z < numChunks; z++)
                {
                    for (int y = -numChunks; y < numChunks; y++)
                    {
                        yield return pos + new Vector3(x, y, z) * VoxelsPerDimension;
                    }
                }
            }
        }
        IEnumerable<Vector3> SceneLoadSequence_CameraAboveGround_SurfaceProjection(Vector3 pos, int numChunks)
        {
            int snakeLength = numChunks*numChunks;
            
            int state = 0;            
            float dx, dz;
            int stride = 0;
            int leapSize = 0;
            bool repeat = false;
            for (int i = 0; i < snakeLength; i++)
            {
                pos = ProjectToSurface(pos);
                yield return (pos);
                yield return (pos + new Vector3(0, VoxelsPerDimension, 0));
                yield return (pos + new Vector3(0, -VoxelsPerDimension, 0));
                
                yield return (pos + new Vector3(VoxelsPerDimension, 0, 0));
                yield return (pos + new Vector3(-VoxelsPerDimension, 0, 0));

                yield return (pos + new Vector3(0, 0, VoxelsPerDimension));
                yield return (pos + new Vector3(0, 0,-VoxelsPerDimension));
                
                switch (state)
                {
                    case 0:
                        dx = -1;
                        dz = 0;
                        break;
                    case 1:
                        dx = 0;
                        dz = 1;
                        break;
                    case 2:
                        dx = 1;
                        dz = 0;
                        break;
                    case 3:
                        dx = 0;
                        dz = -1;
                        break;
                    default: throw new NotImplementedException();
                }

                pos.X += dx * (VoxelsPerDimension);
                pos.Z += dz * (VoxelsPerDimension);

                if (++stride > leapSize)
                {
                    stride = 0;

                    if (repeat)
                    {
                        leapSize++;
                    }
                    repeat = !repeat;

                    state++;
                    state %= 4;
                }
            }
        }


        IEnumerable<Vector3> SceneLoadSequence_OctTree()
        {

            while (!LoadingTree.Root.IsLeaf)
            {
                var leaf = LoadingTree.FindFirstLeaf(x => !x.Value);
                if (leaf != null)
                {
                    yield return leaf.Box.GetCenter();
                }
            }
        }

        Ray? TestRay;
        BoundingBox? TestBox;

        IEnumerable<Vector3> SceneLoadSequence_PerlinIsoSurface_RayCast()
        {

            var tree = LoadingTree;
            var pos = tree.Root.Box.Min;
            float delta = VoxelsPerDimension;
            for (float x = tree.Root.Box.Min.X; x < tree.Root.Box.Max.X; x += delta)
            {
                for (float z = tree.Root.Box.Min.Z; z < tree.Root.Box.Max.Z; z += delta)
                {
                    var v = new Vector3(x, tree.Root.Box.Max.Y + (delta / 2), z);
                    var dir = new Vector3(x + 0.00001f, tree.Root.Box.Min.Y - (delta / 2), z + 0.00001f) - v;
                    dir.Normalize();
                    TestRay = new Ray(v, dir);
                    foreach(var node in tree.RayCast(TestRay.Value))
                    {
                        if (node != null)
                        {
                            if (!node.Value)
                            {
                                TestBox = node.Box;
                                yield return node.Box.GetCenter();
                            }
                            if (node.Box.Max.Y + delta < GetHeight(node.Box.Max.X, node.Box.Max.Z)) break;
                        }
                    } 
                }
            }

            TestRay = null;
            TestBox = null;
        }

        IEnumerable<Vector3> SceneLoadSequence_PerlinIsoSurface()
        {   
            var boxSize = ChunkTree.Root.Box.GetHalfSize()*2;
            var offset = new Vector3(0.5f, 0.5f, 0.5f) * VoxelsPerDimension;
            int numChunks = (int)boxSize.X / VoxelsPerDimension;
            var chunkSize = new Vector3(VoxelsPerDimension, VoxelsPerDimension, VoxelsPerDimension);

            var pos = ChunkTree.Root.Box.Min;
            for (int x = 0; x < numChunks; x++)
            {
                for (int z = 0; z < numChunks; z++)
                {
                    var chunkPos = pos + (new Vector3(x, 0, z) * VoxelsPerDimension);
                    float yWorld = GetHeight(chunkPos.X, chunkPos.Z);
                    var v = new Vector3(chunkPos.X, yWorld, chunkPos.Z);

                    OctTreeNode<Chunk> node  = ChunkTree.GetLeafContaining(v);

                    if (node != null)
                    {
                        foreach (var corner in node.Box.GetCorners())
                        {
                            yield return corner+offset;
                        }
                    }
                }
            }
            
        }


        long frameCount = 0;
        void SceneLoad()
        {
            if (LoadingTree.Root.IsLeaf) return;
            if (frameCount++ % 20 == 0)
            {
                if (!ConsumeLoadSequence())
                {
                    if (LoadSequence != null) LoadSequence.Dispose();

                    LoadSequence = SceneLoadSequence_PerlinIsoSurface_RayCast().GetEnumerator();
                    if (!LoadSequence.MoveNext())
                    {
                        LoadSequence = SceneLoadSequence_OctTree().GetEnumerator();
                    }
                }
            }
        }

        Vector3 ProjectToSurface(Vector3 p)
        {
            return ProjectToHeight(p, GetHeight(p.X, p.Z));
        }

        Vector3 ProjectToHeight(Vector3 p, float h)
        {
            return new Vector3(p.X, h, p.Z);
        }

        bool ConsumeLoadSequence(int max = 16)
        {
            if (LoadSequence == null)
            {
                return false;
            }
            Vector3 v;
            for (int i = 0; i < max; i++)
            {
                v = LoadSequence.Current;
                var loadedNode = ChunkTree.GetLeafContaining(v);
                if (loadedNode != null && loadedNode.Value == null)
                {
                    var chunkLeaf = ChunkTree.GetLeafContaining(v);
                    chunkLeaf.Value = ChunkFactory(chunkLeaf.Box);

                    MarkInTree(v);
                }

                if (!LoadSequence.MoveNext()) return false;
            }
            return true;
        }

        void MarkInTree(Vector3 v)
        {
            var loadingLeaf = LoadingTree.GetLeafContaining(v);
            loadingLeaf.Value = true;
            while (loadingLeaf.IsLeaf 
                && loadingLeaf.Parent != null 
                && !loadingLeaf.Parent.Value 
                && !loadingLeaf.Parent.Children.Any(x => !x.Value))
            {
                loadingLeaf = loadingLeaf.Parent;
                loadingLeaf.Value = true;
                loadingLeaf.Prune();
            }
        }


        #endregion
    }
}
