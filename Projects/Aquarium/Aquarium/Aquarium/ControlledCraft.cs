using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Forever.Physics;
using Microsoft.Xna.Framework;
using Forever.Screens;
using Aquarium.Ui.Steering;
using Aquarium.Ui;


namespace Aquarium
{
    public class ControlledCraft : IOdometerSource
    {
        public IRigidBody Body { get; private set; }
        public SteeringControls ControlForces { get; private set; }


        public ControlledCraft(IRigidBody body, SteeringControls controlForces)
        {
            ControlForces = controlForces;
            Body = body;
        }

        public void Update(GameTime gameTime)
        {
            var duration = gameTime.GetDuration();
            ControlForces.updateForce(Body, duration);
            Body.integrate(gameTime.GetDuration());
        }

        public void HandleInput(InputState input)
        {
            ControlForces.HandleInput(input);
        }

        float IOdometerSource.Ratio
        {
            get
            {
                return ControlForces.ThrusterRatio;
            }
        }

        public Vector3 Velocity
        {
            get { return Body.Velocity; }
        }

        public float MaxSpeed
        {
            get { return 0.01f; }
        }


    }

}
