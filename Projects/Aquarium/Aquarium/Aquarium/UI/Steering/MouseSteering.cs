using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.Screens;
using Microsoft.Xna.Framework;
using Forever.Physics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Aquarium.UI.Steering
{
    public class MouseSteering : ISteering
    {
        public Point Anchor { get { return _graphics.Viewport.Bounds.Center; } }
        public Point Point { get; set; }
        private Point LastPoint { get; set; }
        private GraphicsDevice _graphics;

        public IRigidBody Body { get; private set; }
        public float Impulse { get; set; }

        /// <summary>
        /// Create a MouseSteering instance.
        /// </summary>
        /// <param name="graphics">The graphics device.</param>
        public MouseSteering(GraphicsDevice graphics, IRigidBody body, float impulse)
        {
            _graphics = graphics;
            Body = body;
            Impulse = impulse;
        }

        public void HandleInput(InputState input)
        {
            LastPoint = Point;
            Point = input.CurrentMousePoint;

            CurrentThrust += ThrustIncrement * ((float) input.ScrollbarWheelChange);
            CurrentThrust = Math.Max(CurrentThrust, MinThruster);
            CurrentThrust = Math.Min(CurrentThrust, MaxThruster);

            if (input.IsNewKeyPress(Keys.Back))
            {
                CurrentThrust = 0f;
            }
        }


        public Vector3 GetTorque()
        {
            return GetTorqueToBodyFromAnchor(Body, Anchor, Point);
        }

        public float MinThruster = -0.0001f;
        public float MaxThruster = 0.001f;
        public float ThrustIncrement = 0.0000005f;
        public float CurrentThrust = 0f;

        private Vector3 GetForce()
        {
            var dir = Vector3.Forward;
            dir = Vector3.Transform(dir, Body.Orientation);

            return dir * CurrentThrust;
        }


        public Vector3 GetTorqueToBodyFromAnchor(IRigidBody body, Point anchorPoint, Point point)
        {
            var anchor = Vector2(anchorPoint);
            var mouse = Vector2(point);

            var diff = mouse - anchor;
            var upCom = diff.Y;
            var rightCom = diff.X;

            var rot = new Vector3(-upCom, -rightCom, 0f) * diff.Length()* Impulse;
            rot = Vector3.Transform(rot, body.Orientation);
            return rot ;
        }

        // todo - move to extension method
        Vector2 Vector2(Point p)
        {
            return new Vector2(p.X, p.Y);
        }


        public Vector3 Force
        {
            get { return GetForce(); }
        }

        public Vector3 Torque
        {
            get { return GetTorque(); }
        }
    }
}
