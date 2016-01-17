using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


using Forever.Physics;
using Forever.Render;
using Aquarium.Life;

namespace Aquarium.Agent
{
    public class Hunter : IAgent
    {
        public static string ModelAsset { get { return "Models/enemy"; } }
        public Matrix ModelTransform { get; set; }

        Model Model { get; set; }
        public IRigidBody RigidBody { get; private set; }

        public Vector3 Position { get { return RigidBody.Position; } }

        MutableForceGenerator ForceGen { get; set; }

        public Hunter(Model model, IRigidBody body)
        {
            Model = model;
            RigidBody = body;
            ModelTransform = Matrix.CreateScale(0.2f) * Matrix.CreateRotationY((float)Math.PI);

            ForceGen = new MutableForceGenerator();
        }
        public void Draw(float duration, RenderContext renderContext)
        {
            var world = RigidBody.World;
            Renderer.RenderModel(Model, ModelTransform, world, renderContext);
        }

        public void Update(float duration)
        {
            UpdateAI(duration);
            UpdatePhysics(duration);
        }

        private void UpdatePhysics(float duration)
        {
            ForceGen.updateForce(RigidBody, duration);

            RigidBody.integrate(duration);
        }

        protected void UpdateAI(float duration)
        {
            var veloCap = 0.005f;
            if (RigidBody.Velocity.Length() < veloCap)
            {
                var f = new Vector3(0, 0, -0.00000002f);
                f = Vector3.Transform(f, RigidBody.Orientation);
                ForceGen.Force = f;
            }
            else
            {
                ForceGen.Force = Vector3.Zero;
            }

        }


    }

    
}
