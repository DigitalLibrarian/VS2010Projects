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

namespace Aquarium
{
    class VoxelScreen : FlyAroundGameScreen
    {
        const int VoxelsPerDimension = 16;

        ChunkSpace ChunkSpace { get; set; }

        LabelUiElement DebugLabel { get; set; }

        Perlin Perlin = null;

        OctTree<bool> LoadingTree { get; set; }

        int MaxTreeDepth { get; set; }
        int RenderDepth { get; set; }

        Effect VoxelEffect { get; set; }
        private readonly string EffectName = "Effects\\VoxelEffect";
        
        public override void LoadContent()
        {
            base.LoadContent();
            SetupPerlin();

            VoxelEffect = ScreenManager.Game.Content.Load<Effect>(EffectName);

            ChunkSpace = new ChunkSpace(VoxelsPerDimension, ChunkFactory);

            var spawnHeightAboveGround = 5 * VoxelsPerDimension;
            var spawnPoint = new Vector3(0, GetHeight(0, 0) + spawnHeightAboveGround, 0);
            Ui.Elements.AddRange(CreateUILayout());

            MaxTreeDepth = 4;
            RenderDepth = -1;

            var s = (float)VoxelsPerDimension;
            float worldSize = (s * (float) System.Math.Pow(2, MaxTreeDepth));

            var treeBox = new BoundingBox(
                spawnPoint + new Vector3(-worldSize, -worldSize, -worldSize),
                spawnPoint + new Vector3(worldSize, worldSize, worldSize));
            LoadingTree = OctTree<bool>.CreatePreSubdivided(MaxTreeDepth, treeBox);

            var diff = treeBox.Max - treeBox.Min;
            var startPos = Vector3.Backward * (diff.Length() / 2f);
            User.Body.Position = startPos;
            
            for (int i = 0; i < MaxTreeDepth; i++)
            {
                LoadingTree.VisitLeaves(node =>
                {
                    var c = node.Box.Min;
                    var h = GetHeight(c.X, c.Z);
                    if (c.Y > h)
                    {
                        node.Value = true;
                        if (node.Parent != null && node.Parent.SearchFirstChild(x => !x.Value) == null)
                        {
                            node.Parent.Prune();
                        }
                    }
                });
            }

            RenderSet = new Chunk[MaxRenderSetSize];
            ListRenderSet = new List<Chunk>();
        }
        
        Chunk ChunkFactory(BoundingBox bb)
        {
            var chunk = new Chunk(bb, VoxelsPerDimension);
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

            MarkInTree(chunk.Box.GetCenter());

            return chunk;
        }
        void SetupPerlin()
        {
            NoiseQuality quality = NoiseQuality.High;
            int seed = 0;
            int octaves = 2;
            double frequency = 0.005;
            double lacunarity = 0.5;
            double persistence = 0;

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
            int maxHeight = 100 * VoxelsPerDimension;
            float half = maxHeight / 2f;
            var bottomLeft = new Vector3(-half, -half, -half); ;
            var scale = 0.9f;
            float n = SmoothNoise(10+ x * scale, 10+ z * scale);
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
            if (input.CurrentMouseState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                var mousePoint = input.CurrentMousePoint.ToVector2();
                var ray = RenderContext.GetScreenRay(mousePoint);
                ShootChunksBigLaser(ray, ChunkRayTool.Derez, 1);
            }

            if (input.CurrentMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released
                && input.LastMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                var mousePoint = input.CurrentMousePoint.ToVector2();
                var ray = RenderContext.GetScreenRay(mousePoint);
                ShootChunks(ray, ChunkRayTool.Rez, false);
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

            if (input.CurrentMouseState.MiddleButton == ButtonState.Pressed)
            {
                MarkInTreeBomb();
            }

            base.HandleInput(input);
        }

        void MarkInTreeBomb()
        {
            CenteredCubeIterate(MarkInTree, RenderContext.Camera.Position, 4);
        }

        void CenteredCubeIterate(Action<Vector3> action, Vector3 pos, int halfSize)
        {
            for (int x = -halfSize; x < halfSize; x++)
            {
                for (int z = -halfSize; z < halfSize; z++)
                {
                    for (int y = -halfSize; y < halfSize; y++)
                    {
                        action(pos + (new Vector3(x, y, z) * VoxelsPerDimension));
                    }
                }
            }
        }

        public void ShootChunks(Ray ray, ChunkRayTool tool, bool closestFirst)
        {
            var rayHits = ChunkSpace.Query((coord, chunk) =>
            {
                return chunk.Box.Intersects(ray).HasValue;
            });

            if (closestFirst)
            {
                rayHits = rayHits.OrderBy(x => (x.Position - ray.Position).LengthSquared());
            }
            else
            {
                rayHits = rayHits.OrderByDescending(x => (x.Position - ray.Position).LengthSquared());
            }
            foreach (var chunk in rayHits)
            {
                if(chunk.ToolRay(ray, tool))
                {
                    return;
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
                        ShootChunks(r, tool, true);
                    }
                }
            }
        }
        Chunk[] RenderSet { get; set; }
        int MaxRenderSetSize = 2048*2*2*2*2;
        List<Chunk> ListRenderSet { get; set; }

        Chunk RenderingChunk;
        public override void Draw(GameTime gameTime)
        {
            var duration = gameTime.GetDuration();

            int count = 0;
            int capacity = 0;

            var numChunksXZ = 400;
            var numChunksY = 100;
            var halfSize = new Vector3(numChunksXZ/2f, numChunksY/2f, numChunksXZ/2f) * VoxelsPerDimension;
            var pos = RenderContext.Camera.Position;
            var sphere = new BoundingBox(pos - halfSize, pos + halfSize);

            ListRenderSet.Clear();
            int numAdded = ChunkSpace.Find((chunk) =>
            {
                return (sphere.Contains(chunk.Box) != ContainmentType.Disjoint) && RenderContext.InView(chunk.Box);
            }, ListRenderSet, MaxRenderSetSize);

            for (int i = 0; i < numAdded; i++)
            {
                this.RenderingChunk = ListRenderSet[i];
                count += RenderingChunk.InstanceCount;
                capacity += RenderingChunk.Capacity;

                RenderingChunk.Draw(duration, RenderContext);
                if (debug)
                {
                    Renderer.Render(RenderingChunk.Box, RenderContext.GraphicsDevice, RenderingChunk.World, RenderContext.Camera.View, RenderContext.Camera.Projection, Color.DarkSalmon);
                }
            }

            if (RenderDepth >= 0)
            {
                LoadingTree.VisitAtDepth(node =>
                {
                    Renderer.Render(RenderContext, node.Box, node.Value ? Color.AntiqueWhite : Color.BlueViolet);
                }, RenderDepth);
            }


            DebugLabel.Label = string.Format("Chunks Rendered : {0}. Voxels Rendered {1}. Volume Rendered {2}.", numAdded, count, capacity);
            base.Draw(gameTime);
        }
        bool debug;
        void ToggleDebug()
        {
            debug = !debug;
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            // fill out space around the player
            SceneLoad();

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
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


        long frameCount = 0;
        void SceneLoad()
        {
            if (LoadingTree.Root.IsLeaf) return;
            if (frameCount++ % 20 == 0)
            {
                if (!ConsumeLoadSequence())
                {
                    if (LoadSequence != null) LoadSequence.Dispose();

                    LoadSequence = SceneLoadSequence_OctTree().GetEnumerator();
                }
            }
        }

        Vector3 ProjectToSurface(Vector3 p)
        {
            return new Vector3(p.X, GetHeight(p.X, p.Z), p.Z);
        }

        bool ConsumeLoadSequence(int max = 2)
        {
            if (LoadSequence == null)
            {
                return false;
            }
            Vector3 v;
            for (int i = 0; i < max; i++)
            {
                v = LoadSequence.Current;
                ChunkSpace.GetOrCreate(v);
                if (!LoadSequence.MoveNext()) return false;
            }
            return true;
        }

        void MarkInTree(Vector3 v)
        {
            var leaf = LoadingTree.FindFirstLeaf(x => !x.Value && x.Box.Contains(v) == ContainmentType.Contains);
            if (leaf != null)
            {
                leaf.Value = true;
                
                while (leaf.IsLeaf && leaf.Parent != null && !leaf.Parent.Value && !leaf.Parent.Children.Any(x => !x.Value))
                {
                    leaf = leaf.Parent;
                    leaf.Value = true;
                    leaf.Prune();
                }
            }
        }


        #endregion
    }
}
