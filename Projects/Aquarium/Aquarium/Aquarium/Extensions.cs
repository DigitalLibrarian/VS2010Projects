using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Aquarium
{
    static class AqExtensions
    {
        public static Vector3 HalfwayTo(this Vector3 v1, Vector3 v2)
        {
            var half = (v1 - v2);
            half = v2 + (half * 0.5f);
            return half;
        }

        /// <summary>
        /// Returns the number of milliseconds since last update.  This is what I usually use for an update method.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public static float GetDuration(this GameTime gameTime)
        {
            return (float)gameTime.ElapsedGameTime.Milliseconds;
        }

        public static Vector3 Round(this Vector3 source, int decimals = 0)
        {
            Vector3 rounded = source;
            rounded.X = (float)Math.Round(rounded.X, decimals);
            rounded.Y = (float)Math.Round(rounded.Y, decimals);
            rounded.Z = (float)Math.Round(rounded.Z, decimals);
            return rounded;
        }

        public static T NextElement<T>(this Random me, List<T> list)
        {
            return list[me.Next(list.Count)];
        }
        public static T NextElement<T>(this Random me, ICollection<T> list)
        {
            return list.ElementAt(me.Next(list.Count()));
        }

        public static Vector3 NextVector(this Random me)
        {
            Vector3 v = new Vector3((float)me.NextDouble(), (float)me.NextDouble(), (float)me.NextDouble());
            v -= new Vector3(0.5f, 0.5f, 0.5f);
            v *= 2f;
            return v;
        }

        public static Vector2 ToVector2(this Point p)
        {
            return new Vector2(p.X, p.Y);
        }

        public static BoundingBox ExtendToContain(this BoundingBox me, BoundingBox box)
        {
            return new BoundingBox(Vector3.Min(me.Min, box.Min), Vector3.Max(me.Max, box.Max));
        }

        
    }
}
