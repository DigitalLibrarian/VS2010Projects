using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.Screens;
using Aquarium.Ui;
using Microsoft.Xna.Framework;
using Forever.Render;
using Forever.Render.Cameras;
using Forever.Physics;
using Aquarium.Ui.Steering;

namespace Aquarium
{
    abstract class UiOverlayGameScreen : GameScreen
    {
        protected UiOverlay Ui { get; set; }
        protected RenderContext RenderContext { get; set; }

        public override void LoadContent()
        {
            base.LoadContent();

            var gd = ScreenManager.GraphicsDevice;
            RenderContext = new RenderContext(
                    new EyeCamera(gd),
                    gd
                );
            Ui = new UiOverlay(
                ScreenManager,
                RenderContext
               );
        }
        
        public override void HandleInput(InputState input)
        {
            Ui.HandleInput(input);

            base.HandleInput(input);
        }

        public override void Draw(GameTime gameTime)
        {
            Ui.Draw(gameTime);

            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            Ui.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        protected ControlledCraft CreateControlledCraft()
        {
            var cam = this.RenderContext.Camera;
            var PlayerRigidBody = new RigidBody(cam.Position);
            PlayerRigidBody.Awake = true;
            PlayerRigidBody.CanSleep = false;
            PlayerRigidBody.LinearDamping = 0.9f;
            PlayerRigidBody.AngularDamping = 0.69f;
            PlayerRigidBody.Mass = 0.1f;
            PlayerRigidBody.InertiaTensor = InertiaTensorFactory.Sphere(PlayerRigidBody.Mass, 1f);
            var mouseSteering = new MouseSteering(RenderContext.GraphicsDevice, PlayerRigidBody, 0.000000001f);
            var analogSteering = new AnalogSteering(PlayerIndex.One, 0.000015f, 0.0025f, 0.0003f, 0.001f, PlayerRigidBody);

            var controlForces = new SteeringControls(mouseSteering, analogSteering);
            controlForces.MaxAngular = 0.025f;
            controlForces.MaxLinear = 0.1f;

            return new ControlledCraft(PlayerRigidBody, controlForces);
        }
    }
}
