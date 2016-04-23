using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.Voxel.Meshing;
using Microsoft.Xna.Framework.Graphics;
using Forever.Voxel;
using Forever.Extensions;
using Microsoft.Xna.Framework;
using Forever.Voxel.World;

namespace Aquarium
{
    class InstancingScreen : FlyAroundGameScreen
    {
        const int MaxPrimitives = 1048575;
        int Capacity = MaxPrimitives;
        CubeInstancing Instancing { get; set; }

        VertexDeclaration InstanceVertexDeclaration { get; set; }
        VertexBufferBinding InstanceBufferBinding { get; set; }

        IVoxelEffect VoxelEffect { get; set; }

        public override void LoadContent()
        {
            base.LoadContent();

            Instancing = new CubeInstancing(ScreenManager.Game.GraphicsDevice);

            SetupInstanceVertexDeclaration();
            var instanceCount = this.Capacity;
            var instanceBuffer = new VertexBuffer(ScreenManager.Game.GraphicsDevice, InstanceVertexDeclaration,
                                              instanceCount, BufferUsage.WriteOnly);

            var half = 500;
            var min = new Vector3(-half, -half, -half);
            var max = new Vector3(half, half, half);
            var b = new BoundingBox(min, max);
            var r = new Random();
            Voxel.ViewState[] instances = new Voxel.ViewState[Capacity];
            for (int i = 0; i < Capacity; i++)
            {
                var v = r.NextVector(b);
                instances[i] = new Voxel.ViewState
                {
                    Position = new Vector4(v.X, v.Y, v.Z, 0f),
                    Color = r.NextColor()
                };
            }

            instanceBuffer.SetData(0, instances, 0, Capacity, InstanceVertexDeclaration.VertexStride);
            InstanceBufferBinding = new VertexBufferBinding(instanceBuffer, 0, 1);

            var effect = ScreenManager.Game.Content.Load<Effect>("Effects\\VoxelEffect");
            effect.CurrentTechnique = effect.Techniques["Instancing"];
            VoxelEffect = new VoxelEffectAdapter(effect);
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            var rc = RenderContext;
            var wvp = Matrix.Identity
                    * rc.Camera.View
                    * rc.Camera.Projection;

            var camPos = rc.Camera.Position;
            var lightPos = new Vector3(-200, 200, -200);
            float distance = (Vector3.Zero - lightPos).LengthSquared();


            VoxelEffect.WVP = wvp;
            VoxelEffect.CameraPosition = camPos;
            VoxelEffect.LightPos = lightPos;
            VoxelEffect.LightDistanceSquared = distance;
            float intensity = 0.1f;
            VoxelEffect.LightDiffuseColorIntensity = new Color(intensity, intensity, intensity);
            VoxelEffect.DiffuseColor = Color.White;

            VoxelEffect.CurrentTechnique.Passes[0].Apply();
            Instancing.Draw(RenderContext, InstanceBufferBinding, this.Capacity);

            base.Draw(gameTime);
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

    }
}
