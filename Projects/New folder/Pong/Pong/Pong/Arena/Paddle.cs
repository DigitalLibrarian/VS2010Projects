using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Pong.ArenaMatch
{

    public abstract class Paddle
    {
        public Vector2 StartPosition { get; set; }
        public Vector2 Position { get; set; }
        public float Length { get; set; }
        public float Width { get; set; }
        public float Speed { get; set; }

        public Color Color { get; set; }
        
        public Vector2 Right { get { return new Vector2(Position.X + Width/2, Position.Y); } }
        public Vector2 Left { get { return new Vector2(Position.X - Width/2, Position.Y); } }
        public Vector2 Top { get { return new Vector2(Position.X,  Position.Y + Length / 2); } }
        public Vector2 Bottom { get { return new Vector2(Position.X, Position.Y - Length / 2); } }

        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle(
                    (int)Position.X - (int)Width / 2,
                    (int)Position.Y - (int)Length / 2,
                    (int)Width,
                    (int)Length);
            }
        }

        
        public Paddle(Vector2 position, float length, float width, float speed = 0.1f)
        {
            StartPosition = position;
            Position = position;
            Length = length;
            Width = width;
            Speed = speed;

            Color = Color.Blue;
        }

        public void Reset()
        {
            Position = StartPosition;
        }
        
        public void Draw(DrawSpriteBatch spriteBatch)
        {
            var rectangle = Rectangle;

            spriteBatch.DrawRectangle(rectangle, Color, filled: true);
        }

        public virtual void Update(Arena arena, Ball ball, float duration)
        {
            var change = ComputeChange(ball);

            
            var allGood = true;
            var test = new[] { 
                Top, Bottom, 
            };
            foreach(var point in test)
            {
                var projPos = point + (duration * change * Speed);
                allGood &= !arena.IsOutOfBounds(projPos);
            }
            if(allGood)
            {
                Position += duration * change * Speed;
            }

            Collisions.Clear();
        }

        public Vector2 ComputeChange(Ball ball)
        {
            return ComputeChange(ball, this);
        }

        public abstract Vector2 ComputeChange(Ball ball, Paddle paddle);


        public RunDirection GetOrientation(Ball ball)
        {
            throw new NotImplementedException();
        }

        List<Ball> Collisions  = new List<Ball>();
        public void OnBallCollide(Ball ball)
        {
            Collisions.Add(ball);
            ball.Color = Color;
        }

    }
}
