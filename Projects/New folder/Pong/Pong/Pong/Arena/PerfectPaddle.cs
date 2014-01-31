using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Pong.ArenaMatch
{
    public class PerfectPaddle : Paddle
    {

        public PerfectPaddle(Vector2 position, float length, float width, float speed = 0.1f) : base(position, length, width, speed) {}

        public override Vector2 ComputeChange(Ball ball, Paddle paddle)
        {
            var change = Vector2.Zero;
            if (ball.Position.Y > paddle.Position.Y) change += Vector2.UnitY;
            if (ball.Position.Y < paddle.Position.Y) change -= Vector2.UnitY;
            return change;
        }
    }
}
