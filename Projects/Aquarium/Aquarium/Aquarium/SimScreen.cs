using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.Xna.Framework;
using Forever.Screens;
using Aquarium.GA.Population;
using Forever.Render;
using Forever.Physics;

namespace Aquarium
{
    public class SimScreen : GameScreen
    {
        Thread GenerateThread;

        public Population Pop { get; private set; }

        protected RenderContext RenderContext { get; private set; }
        public IRigidBody CamBody { get; private set; }
        public UserControls CamControls { get; private set; }

        public SimScreen(RenderContext renderContext)
        {
            RenderContext = renderContext;
            Pop = new RandomPopulation(100, 1000);

            GenerateThread = new Thread(new ThreadStart(() => { }));
        }

        protected ICamera Camera { get { return RenderContext.Camera; } }

        public override void LoadContent()
        {
            base.LoadContent();

            SetupCamera();


            GenerateThread.IsBackground = true;
            GenerateThread.Start();
        }

        public override void UnloadContent()
        {

            GenerateThread.Abort();
            System.Threading.SpinWait.SpinUntil(() => 
                {
                    System.Threading.Thread.Sleep(100);
                    return !GenerateThread.IsAlive;
                }
                );

            base.UnloadContent();


        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Draw(gameTime);
            var members = Pop.LocalMembers(RenderContext.Camera.Position);

            foreach (var member in members)
            {
                member.Specimen.Body.Render(RenderContext);
            }

        }

       

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (!otherScreenHasFocus && !coveredByOtherScreen)
            {
                UpdateCamera(gameTime);
            }

            if (!otherScreenHasFocus)
            {
                float duration = (float)gameTime.ElapsedGameTime.Milliseconds;
                var members = Pop.LocalMembers(Camera.Position);
                foreach (var member in members)
                {
                    member.Specimen.Update(duration);
                }

            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        #region Camera Controls


        private void SetupCamera()
        {
            var cam = Camera;
            CamBody = new RigidBody(cam.Position);
            CamBody.Awake = true;
            CamBody.LinearDamping = 0.9f;
            CamBody.AngularDamping = 0.7f;
            CamBody.Mass = 0.1f;
            CamBody.InertiaTensor = InertiaTensorFactory.Sphere(CamBody.Mass, 1f);
            CamControls = new UserControls(PlayerIndex.One, 0.000015f, 0.0025f, 0.0003f, 0.001f);
        }

        protected void UpdateCamera(Microsoft.Xna.Framework.GameTime gameTime)
        {
            Vector3 actuatorTrans = CamControls.LocalForce;
            Vector3 actuatorRot = CamControls.LocalTorque;


            float forwardForceMag = -actuatorTrans.Z;
            float rightForceMag = actuatorTrans.X;
            float upForceMag = actuatorTrans.Y;

            Vector3 force =
                (Camera.Forward * forwardForceMag) +
                (Camera.Right * rightForceMag) +
                (Camera.Up * upForceMag);



            if (force.Length() != 0)
            {
                CamBody.addForce(force);
            }



            Vector3 worldTorque = Vector3.Transform(CamControls.LocalTorque, CamBody.Orientation);

            if (worldTorque.Length() != 0)
            {
                CamBody.addTorque(worldTorque);
            }
            
            CamBody.integrate((float)gameTime.ElapsedGameTime.Milliseconds);
            Camera.Position = CamBody.Position;
            Camera.Rotation = CamBody.Orientation;
            
        }


        public override void HandleInput(InputState input)
        {
            base.HandleInput(input);
            CamControls.HandleInput(input);
        }
        #endregion
    }
}
