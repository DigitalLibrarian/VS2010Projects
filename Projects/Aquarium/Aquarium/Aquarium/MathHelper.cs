using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Aquarium
{
    static class MathHelper
    {
        public static float AngleBetween(Vector3 vA, Vector3 vB)
        {
            var v1 = vA;
            v1.Normalize();
            var v2 = vB;
            v2.Normalize();
            var angle = (float)Math.Acos((float)Vector3.Dot(v1, v2));
            
            if (angle >= 0)
                angle /= ((float)Math.PI * 0.25f);
            else
                angle = ((angle + (float)Math.PI * 2f) / ((float)Math.PI * 0.25f));

            return angle;
        }

        public static Matrix GetRotationAToB(Vector3 a, Vector3 b)
        {
            var src = a;
            var dest = b;
            src.Normalize();
            dest.Normalize();

            float d = Vector3.Dot(src, dest);

            if (d >= 1f)
            {
                return Matrix.Identity;
            }
            else if (d < (1e-6f - 1.0f))
            {
                Vector3 axis = Vector3.Cross(Vector3.UnitX, src);

                if (axis.LengthSquared() == 0)
                {
                    axis = Vector3.Cross(Vector3.UnitY, src);
                }

                axis.Normalize();
                return Matrix.CreateFromAxisAngle(axis,(float) Math.PI);
            }
            else
            {
                float s = (float)Math.Sqrt((1 + d) * 2);
                float invS = 1 / s;

                Vector3 c = Vector3.Cross(src, dest);
                Quaternion q = new Quaternion(invS * c, 0.5f * s);
                q.Normalize();

                return Matrix.CreateFromQuaternion(q);
            }
        }

       
        public static Vector3 Round(this Vector3 source, int decimals=0)
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


    }
}
