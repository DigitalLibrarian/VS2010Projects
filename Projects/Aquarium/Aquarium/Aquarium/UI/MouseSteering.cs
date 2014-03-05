using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.Screens;
using Microsoft.Xna.Framework;
using Forever.Physics;

namespace Aquarium.UI
{
    public class MouseSteering
    {
        public bool Engaged { get; set; }

        public Point Point { get; set; }
        private Point LastPoint { get; set; }


        public void HandleInput(InputState input)
        {
            LastPoint = Point;
            Point = input.CurrentMousePoint;

        }

        public Vector3 GetTorqueForBodyAndAnchor(GameTime gameTime, IRigidBody body, Point anchorPoint)
        {
            float veloCap = 0.1f;
            if (Engaged & body.Rotation.Length() < veloCap)
            {
                var anchor = Vector2(anchorPoint);
                var mouse = Vector2(Point);

                var diff = mouse - anchor;
                var duration = (float)gameTime.ElapsedGameTime.Milliseconds;
                float spin = 0.000000001f * duration * diff.Length();
                var upCom = diff.Y;
                var rightCom = diff.X;

                var rot = new Vector3(-upCom * spin, -rightCom * spin, 0f);
                rot = Vector3.Transform(rot, body.Orientation);
                

                return rot;
            }
            return Vector3.Zero;
        }

        Vector2 Vector2(Point p)
        {
            return new Vector2(p.X, p.Y);
        }

    }
}
