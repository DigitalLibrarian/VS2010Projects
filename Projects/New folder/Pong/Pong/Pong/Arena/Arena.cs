using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Pong.ArenaMatch
{
    public class Arena
    {
        public List<Paddle> Paddles { get; set; }
        public Ball Ball { get; set; }
        public Rectangle Bounds { get; set; }
        public Vector2 StartingThump { get; set; }


        public Arena(int width, int height, List<Paddle> paddles, Ball ball)
        {
            Bounds = new Rectangle(0, 0, width, height);
            Paddles = paddles;
            Ball = ball;
        }

        public void StartNewRound()
        {
            Ball.Reset();
            Paddles.ForEach(x => x.Reset());

        }


        public void Draw(DrawSpriteBatch spriteBatch)
        {
            spriteBatch.DrawRectangle(Bounds, Color.White);

            Ball.Draw(spriteBatch);
            Paddles.ForEach(x => x.Draw(spriteBatch));
        }

        public void Update(float duration)
        {
            Ball.Update(this, duration);
            Paddles.ForEach(x => x.Update(this, Ball, duration));
        }

        public Vector2 CalcBallCorrection(Ball ball, float duration)
        {
            var result = Vector2.One;

            if (IsOutOfBounds(ball.Position + ball.Velocity * (2 * duration)))
            {
                if (ball.Top.Y > Bounds.Bottom && ball.RiseDirection == RiseDirection.Up
                    || ball.Bottom.Y < Bounds.Top && ball.RiseDirection == RiseDirection.Down)
                {
                    result *= new Vector2(1, -1f);
                }

                if (ball.RunDirection == RunDirection.Right && ball.Right.X > Bounds.Right
                    || ball.RunDirection == RunDirection.Left && ball.Left.X < Bounds.Left)
                {

                    result *= new Vector2(-1f, 1f);
                    OnSideBall(ball);
                }
            }
            
            return result;
        }


        public void OnSideBall(Ball ball)
        {
            //
        }

        public bool IsOutOfBounds(Vector2 worldCoord)
        {
            var point = new Point((int)worldCoord.X,(int) worldCoord.Y);

            return !Bounds.Contains(point);
        }



    }
}
