using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.UI;
using Forever.Render;
using Microsoft.Xna.Framework.Graphics;
using Forever.Physics;
using Microsoft.Xna.Framework;

namespace Aquarium
{
    public class Hunter
    {
        public static string ModelAsset { get { return "Models/enemy"; } }
        public Matrix ModelTransform { get; set; }

        Model Model { get; set; }
        public IRigidBody RigidBody { get; private set; }

        MutableForceGenerator ForceGen { get; set; }

        public Hunter(Model model, IRigidBody body)
        {
            Model = model;
            RigidBody = body;
            ModelTransform = Matrix.CreateScale(0.2f) * Matrix.CreateRotationY((float)Math.PI);

            ForceGen = new MutableForceGenerator();
        }
        public void Draw(RenderContext renderContext)
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
