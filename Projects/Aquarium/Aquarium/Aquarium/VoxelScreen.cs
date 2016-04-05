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
        const int ChunksPerDimension = 16;

        ChunkSpace ChunkSpace { get; set; }

        LabelUiElement TotalInstancesLabel { get; set; }
        LabelUiElement FrustumCullingLabel { get; set; }

        Perlin Perlin = null;

        OctTree Tree { get; set; }

        int MaxTreeDepth { get; set; }
        int RenderDepth { get; set; }

        public override void LoadContent()
        {
            base.LoadContent();
            SetupPerlin();

            ChunkSpace = new ChunkSpace(ChunksPerDimension, ChunkFactory);

            var spawnHeightAboveGround = 5 * ChunksPerDimension;
            var spawnPoint = new Vector3(0, GetHeight(0, 0) + spawnHeightAboveGround, 0);
            User.Body.Position = spawnPoint;
            Ui.Elements.AddRange(CreateUILayout());

            MaxTreeDepth = 4;
            RenderDepth = 4;

            var s = (float)ChunksPerDimension;
            float worldSize = s * (float) System.Math.Pow(2, MaxTreeDepth);

            // TODO - make this bounding box size exactly big enough for the leaves to be chunks
            Tree = OctTree.CreatePreSubdivided(MaxTreeDepth,
                new BoundingBox(
                new Vector3(-worldSize, -worldSize, -worldSize),
                new Vector3(worldSize, worldSize, worldSize)));

            for (int i = 0; i < RenderDepth; i++)
            {
                Tree.VisitLeaves(node =>
                {
                    var c = node.Box.GetCenter();
                    var h = GetHeight(c.X, c.Z);
                    if (c.Y > h)
                    {
                        node.Occupied = true;
                        if (node.Parent != null && node.Parent.SearchFirstChild(x => !x.Occupied) == null)
                        {
                            node.Parent.PruneChildren();
                        }
                    }
                });
            }
        }

        Chunk ChunkFactory(BoundingBox bb)
        {
            var chunk = new Chunk(bb, ChunksPerDimension);
            chunk.LoadContent(ScreenManager.Game.Content);
            chunk.Initialize(RenderContext.GraphicsDevice);
            var pos = bb.Min;
            chunk.VisitCoords((x, y, z) => {
                var world = chunk.ArrayToChunk(new Vector3(x, y, z));
                float tX = (pos.X + (x * ChunksPerDimension));
                float tY = (pos.Y + (y * ChunksPerDimension));
                float tZ = (pos.Z + (z * ChunksPerDimension));
                
                chunk.Voxels[x][y][z].Material = new Material(
                    new Color(
                       (float) x / ChunksPerDimension,
                       (float) y / ChunksPerDimension,
                       (float) z / ChunksPerDimension
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
            NoiseQuality quality = NoiseQuality.Standard;
            int seed = 0;
            int octaves = 6;
            double frequency = 0.0005;
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
            int maxHeight = 100 * ChunksPerDimension;
            float half = maxHeight / 2f;
            var bottomLeft = new Vector3(-half, -half, -half); ;
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
            TotalInstancesLabel = new LabelUiElement(RenderContext, spriteFont, DebugLabelStrip());
            FrustumCullingLabel = new LabelUiElement(RenderContext, spriteFont, DebugLabelStrip());

            return new List<IUiElement>{
                TotalInstancesLabel, 
                FrustumCullingLabel
            };
        }

        public override void HandleInput(InputState input)
        {
            if (input.CurrentMouseState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                var mousePoint = input.CurrentMousePoint.ToVector2();
                var ray = GetMouseRay(mousePoint);
                ShootChunksBigLaser(ray, ChunkRayTool.Derez, 2);
            }

            if (input.CurrentMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released
                && input.LastMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                var mousePoint = input.CurrentMousePoint.ToVector2();
                var ray = GetMouseRay(mousePoint);
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
                if (RenderDepth > 0) RenderDepth--;
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
                        action(pos + (new Vector3(x, y, z) * ChunksPerDimension));
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

        public Ray GetMouseRay(Vector2 mousePosition)
        {
            var projection = RenderContext.Camera.Projection;
            var view = RenderContext.Camera.View;
            Viewport viewport = RenderContext.GraphicsDevice.Viewport;

            Vector3 near = new Vector3(mousePosition, 0);
            Vector3 far = new Vector3(mousePosition, 1);

            near = viewport.Unproject(near, projection, view, Matrix.Identity);
            far = viewport.Unproject(far, projection, view, Matrix.Identity);

            return new Ray(RenderContext.Camera.Position, Vector3.Normalize(far - near));
        }

        int InViewCount = 0;
        int OutOfViewCount = 0;
        public override void Draw(GameTime gameTime)
        {
            var duration = gameTime.GetDuration();
            InViewCount = OutOfViewCount = 0;

            int count = 0;
            int capacity = 0;

            var numChunks = 100;
            var sphere = new BoundingSphere(RenderContext.Camera.Position, ChunksPerDimension * numChunks);
            
            var renderSet = ChunkSpace.Query((coord, chunk) =>
            {
               return sphere.Intersects(chunk.Box);
            });

            foreach(var chunk in renderSet)
            {
                count += chunk.InstanceCount;
                capacity += chunk.Capacity;

                if (RenderContext.InView(chunk.Box))
                {
                    chunk.Draw(duration, RenderContext);
                    if (debug)
                    {
                        var rc = RenderContext;
                        Renderer.Render(chunk.Box, rc.GraphicsDevice, chunk.World, rc.Camera.View, rc.Camera.Projection, Color.DarkSalmon);
                    }

                    InViewCount++;
                }
                else
                {
                    OutOfViewCount++;
                }
            }

            Tree.VisitAtDepth(node =>
            {
                Renderer.Render(RenderContext, node.Box, node.Occupied ? Color.AntiqueWhite : Color.BlueViolet);
            }, RenderDepth);


            TotalInstancesLabel.Label = string.Format("Instances : {0} / {1}", count, capacity);
            int totalChunks = InViewCount + OutOfViewCount;
            FrustumCullingLabel.Label = string.Format("Chunks In View: {0} / {1}", InViewCount, totalChunks);
            base.Draw(gameTime);
        }
        bool debug;
        void ToggleDebug()
        {
            debug = !debug;
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (!otherScreenHasFocus && !coveredByOtherScreen)
            {
                // fill out space around the player
                SceneLoad(User.Body.Position);
            }

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
                        yield return pos + new Vector3(x, y, z) * ChunksPerDimension;
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
                yield return (pos + new Vector3(0, ChunksPerDimension, 0));
                yield return (pos + new Vector3(0, -ChunksPerDimension, 0));
                
                yield return (pos + new Vector3(ChunksPerDimension, 0, 0));
                yield return (pos + new Vector3(-ChunksPerDimension, 0, 0));

                yield return (pos + new Vector3(0, 0, ChunksPerDimension));
                yield return (pos + new Vector3(0, 0,-ChunksPerDimension));
                
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

                pos.X += dx * (ChunksPerDimension);
                pos.Z += dz * (ChunksPerDimension);

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
            foreach (var v in Tree.Search((node) =>
            {
                if (node.Occupied) return false;
                return true;
            }))
            {
                yield return v.Box.GetCenter();
            }
        }

        long frameCount = 0;
        void SceneLoad(Vector3 pos)
        {
            if (frameCount++ % 20 == 0)
            {
                if (!ConsumeLoadSequence())
                {
                     LoadSequence = SceneLoadSequence_OctTree().GetEnumerator();
                    return;
                    // get new sequence
                    var camHeight = pos.Y;

                    if (camHeight > GetHeight(RenderContext.Camera.Position.X, RenderContext.Camera.Position.Z))
                    {
                        LoadSequence = SceneLoadSequence_CameraAboveGround_SurfaceProjection(pos, numChunks: 10).GetEnumerator();
                    }
                    else
                    {
                        LoadSequence = SceneLoadSequence_CameraBelowGround(pos, numChunks: 10).GetEnumerator();
                    }
                }
            }
        }

        Vector3 ProjectToSurface(Vector3 p)
        {
            return new Vector3(p.X, GetHeight(p.X, p.Z), p.Z);
        }

        bool ConsumeLoadSequence(int max = 1)
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
            Tree.VisitLeaves((child) =>
            {
                var containment = child.Box.Contains(v);
                if (containment != ContainmentType.Disjoint)
                {
                    if (!child.Occupied)
                    {
                        child.Occupied = true;
                        if (child.Parent == null) return;
                        if (child.Parent.Children.All(x => x.Occupied))
                        {
                            // parent should be pruned
                            child.Parent.PruneChildren();
                        }
                    }
                }
            });
        }



        #endregion
    }
}
