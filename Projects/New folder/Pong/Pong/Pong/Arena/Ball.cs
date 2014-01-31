using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Pong.ArenaMatch
{
    public class Ball
    {
        public Vector2 StartPosition { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 StartVelocity { get; set; }
        public Vector2 Velocity { get; private set; }
        public float Radius { get; set; }
        
        public Vector2 Right { get { return new Vector2(Position.X + Radius, Position.Y); } }
        public Vector2 Left { get { return new Vector2(Position.X - Radius, Position.Y); } }
        public Vector2 Top { get { return new Vector2(Position.Y + Radius); } }
        public Vector2 Bottom { get { return new Vector2(Position.Y - Radius); } }

        public Color Color { get; set; }

        public Rectangle Rectangle { get { return new Rectangle((int)Left.X, (int)Top.Y, (int) Radius * 2, (int)Radius * 2); } }

        public Ball(Vector2 position, Vector2 velocity, float radius)
        {
            StartPosition = position;
            Position = position;
            Velocity = velocity;
            StartVelocity = velocity;
            Radius = radius;
            Color = Color.Gold;
        }

        public void Reset()
        {
            Position = StartPosition;
            Velocity = StartVelocity;
        }

        public void Draw(DrawSpriteBatch spriteBatch)
        {
            // Our ball is really a fat little plus sign

            var topLeft = new Vector2(Position.X - Radius, Position.Y - Radius);
            var cut = (int)Radius / 2;
            var rect = new Rectangle((int)topLeft.X + cut, (int)topLeft.Y, (int)(2  *Radius) - (cut * 2), (int)Radius * 2);
            spriteBatch.DrawRectangle(rect, Color, filled: true);


            rect = new Rectangle((int)topLeft.X, (int)topLeft.Y + cut, (int)Radius * 2, (int)(2 * Radius) - (cut * 2));
            spriteBatch.DrawRectangle(rect, Color, filled: true);
        }

 
        public RunDirection RunDirection { get { return Velocity.X > 0 ? RunDirection.Right : RunDirection.Left; } }
        public RiseDirection RiseDirection { get { return Velocity.Y > 0 ? RiseDirection.Up : RiseDirection.Down; } }

        public void Update(Arena arena, float duration)
        {
            HandleCollisions(arena, duration);
            Position += Velocity * duration;
        }

        public void HandleCollisions(Arena arena, float duration)
        {
            var result = arena.CalcBallCorrection(this, duration);

            Velocity *= result;
            foreach (var paddle in arena.Paddles)
            {
                CollidePaddle(paddle);
            }
        }

        private void CollidePaddle(Paddle paddle)
        {
            if (Rectangle.Intersects(paddle.Rectangle))
            {
                Velocity *= new Vector2(-1f, 1f);
                paddle.OnBallCollide(this);
            }
        }


    }
}
