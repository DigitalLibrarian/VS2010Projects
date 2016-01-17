using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.Physics;
using Forever.Screens;

namespace Aquarium.Ui.Steering
{
    public class SteeringControls
    {
        public bool MouseSteeringEngaged { get; private set; }
        public MouseSteering Mouse { get; private set; }
        public AnalogSteering Analog { get; private set; }

        public float MaxAngular { get; set; }
        public float MaxLinear { get; set; }
        

        public SteeringControls(MouseSteering mouse, AnalogSteering analog)
        {
            Mouse = mouse;
            Analog = analog;
        }

        public void HandleInput(InputState input)
        {
            Analog.HandleInput(input);
            Mouse.HandleInput(input);
        }


        public void updateForce(IRigidBody body, float duration)
        {
            if (Analog.ControlSchemeToggle)
            {
                MouseSteeringEngaged = !MouseSteeringEngaged;
            }
            var force = Analog.LocalForce;
            var torque = Analog.LocalTorque;

            if (force.Length() != 0 && body.Velocity.Length() < MaxLinear)
            {
                body.addForce(force);
            }

            if (torque.Length() != 0 && body.Rotation.Length() < MaxAngular)
            {
                body.addTorque(torque);
            }

            if (MouseSteeringEngaged)
            {
                var mouseTorque = Mouse.GetTorque();
                {
                    body.addTorque(mouseTorque * duration);
                }
            }

            body.addForce(Mouse.Force);
        }

        public float ThrusterRatio
        {
            get { 
                double r = Math.Abs(Mouse.CurrentThrust) / Mouse.MaxThruster;


                r = Math.Min(1, r);
                return (float)Math.Max(0, r);
            }
        }
    }
}
