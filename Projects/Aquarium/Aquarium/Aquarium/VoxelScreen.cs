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
using Forever.Voxel.World;
using Aquarium.Sampling;

namespace Aquarium
{
    class VoxelScreen : FlyAroundGameScreen
    {
        const int VoxelsPerDimension = 8;
        int LoadingFrameFrequency = 20;
        int LoadSequencePumpSize = 8;

        LabelUiElement DebugLabel { get; set; }
        
        OctTree<bool> LoadingTree { get; set; }
        OctTree<Chunk> ChunkTree { get; set; }

        int MaxTreeDepth { get; set; }
        int RenderDepth { get; set; }

        Effect VoxelEffect { get; set; }
        private readonly string EffectName = "Effects\\VoxelEffect";

        IInstancer Instancer { get; set; }

        Ray? TestRay;
        BoundingBox? TestBox;
        NoiseHeightMapVoxelSampler VoxelSampler { get; set; }

        float VoxelSize = 1f;

        public override void LoadContent()
        {
            base.LoadContent();
            VoxelSampler = new NoiseHeightMapVoxelSampler()
            {
                MaxHeight = VoxelsPerDimension * VoxelSize * 500,
                StartColor = Color.LightPink,
                EndColor = Color.DarkOrchid
            };
            VoxelEffect = ScreenManager.Game.Content.Load<Effect>(EffectName);

            var spawnHeightAboveGround = 5 * VoxelsPerDimension * VoxelSize;
            var spawnPoint = new Vector3(0, GetHeight(0, 0) + spawnHeightAboveGround, 0);
            Ui.Elements.AddRange(CreateUILayout());

            MaxTreeDepth = 5;
            RenderDepth = 0;

            var chunkSize = (float)VoxelsPerDimension * VoxelSize;
            float worldSize = (chunkSize * (float) System.Math.Pow(2, MaxTreeDepth)) * 0.5f;

            var treeBox = new BoundingBox(
                spawnPoint + new Vector3(-worldSize, -worldSize, -worldSize),
                spawnPoint + new Vector3(worldSize, worldSize, worldSize));
            LoadingTree = OctTree<bool>.CreatePreSubdivided(MaxTreeDepth, treeBox);
            ChunkTree = OctTree<Chunk>.CreatePreSubdivided(MaxTreeDepth, treeBox);

            var diff = treeBox.Max - treeBox.Min;
            //var startPos = Vector3.Zero;
            //startPos = ProjectToHeight(startPos, ChunkTree.Root.Box.Max.Y);
            User.Body.Position = spawnPoint;
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
            
            Instancer = new CubeInstancing(ScreenManager.Game.GraphicsDevice, VoxelSize);
        }
        
        Chunk ChunkFactory(BoundingBox bb)
        {
            //var chunk = new Chunk(bb, VoxelsPerDimension, Instancer);
            var chunk = new Chunk(bb.GetCenter(), VoxelsPerDimension, VoxelSize, Instancer);

            chunk.Initialize(RenderContext.GraphicsDevice, VoxelEffect);
            var pos = bb.Min;
            chunk.VisitCoords((x, y, z) => {
                chunk.Voxels[x][y][z] = VoxelSampler.GetSample(chunk.ArrayToWorld(new Vector3(x, y, z)), VoxelSize);
            });
            
            return chunk;
        }

        float GetHeight(float x, float z)
        {
            return VoxelSampler.GetSurfaceHeight(x, z, VoxelSize);
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
                        var offset = new Vector3(x, y, z) * VoxelSize;
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
            var halfSize = new Vector3(numChunksXZ/2f, numChunksY/2f, numChunksXZ/2f) * VoxelsPerDimension * VoxelSize;
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
        
        IEnumerable<Vector3> SceneLoadSequence_PerlinIsoSurface_RayCast()
        {
            var tree = LoadingTree;
            var pos = tree.Root.Box.Min;
            float delta = VoxelsPerDimension * VoxelSize;
            float half = delta / 2f;
            Vector3[] nodeCorners = new Vector3[8];
            for (float x = tree.Root.Box.Min.X+half; x < tree.Root.Box.Max.X+half; x += half)
            {
                for (float z = tree.Root.Box.Min.Z+half; z < tree.Root.Box.Max.Z+half; z += half)
                {
                    var v = new Vector3(x, tree.Root.Box.Max.Y, z);
                    // ray temporary fix for issue #57
                    var dir = new Vector3(x, tree.Root.Box.Min.Y, z) - v;
                    dir.Normalize();
                    TestRay = new Ray(v, dir);
                    foreach (var node in tree.RayCast(TestRay.Value))
                    {
                        if (node != null)
                        {
                            if (!node.Value)
                            {
                                TestBox = node.Box;
                                yield return node.Box.GetCenter();
                            }

                            bool allUnder = true;
                            node.Box.GetCorners(nodeCorners);
                            foreach (var corner in nodeCorners)
                            {
                                // if the corner is above the surface, it is not submerged
                                if (GetHeight(corner.X, corner.Z) < corner.Y)
                                {
                                    allUnder = false;
                                    break;
                                }
                            }

                            if (allUnder)
                            {
                                // if node is completely submerged then stop loading this ray
                                break;
                            }
                        }
                    }
                }
            }

            TestRay = null;
            TestBox = null;

            foreach(var v in SceneLoadSequence_OctTree())
            {
                yield return v;
            }
        }


        long frameCount = 0;
        void SceneLoad()
        {
            if (LoadingTree.Root.IsLeaf) return;
            if (frameCount++ % LoadingFrameFrequency == 0)
            {
                if (!ConsumeLoadSequence())
                {
                    if (LoadSequence != null) LoadSequence.Dispose();

                    LoadSequence = SceneLoadSequence_PerlinIsoSurface_RayCast().GetEnumerator();
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

        bool ConsumeLoadSequence()
        {
            if (LoadSequence == null)
            {
                return false;
            }
            Vector3 v;
            for (int i = 0; i < LoadSequencePumpSize; i++)
            {
                if (!LoadSequence.MoveNext()) return false;
                v = LoadSequence.Current;
                var loadedNode = ChunkTree.GetLeafContaining(v);
                if (loadedNode != null && loadedNode.Value == null)
                {
                    var chunkLeaf = ChunkTree.GetLeafContaining(v);
                    chunkLeaf.Value = ChunkFactory(chunkLeaf.Box);

                    MarkInTree(v);
                }

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
