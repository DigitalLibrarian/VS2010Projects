using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Aquarium.GA
{
    public static class Fuzzy
    {

        public static T CircleIndex<T>(List<T> list, int index)
        {
            var count = list.Count();
            return list[index % count];
        }

        public static int RoundToInt(double d)
        {
            return (int)Math.Round(d, 0);
        }
        public static int PositiveInteger(double d)
        {
            return Math.Abs(RoundToInt(d * 1000));
        }

        public static float PositiveFloat(double d)
        {
            return (float)Math.Abs(d);
        }

        public static float ColorValue(double d)
        {
            return ToFractionalFloat(d);
        }

        public static Color ToColor(double r, double g, double b)
        {
            return new Color(ColorValue(r), ColorValue(g), ColorValue(b));
        }
        
        public static float ToFractionalFloat(double d)
        {
            return (float)(d - Math.Floor(d));
        }

        public static Vector3 ToScaleVector(double x, double y, double z)
        {

            return new Vector3(
                .75f + ToFractionalFloat(x),
                .75f + ToFractionalFloat(y),
                .75f + ToFractionalFloat(z)
                );

            /*
            return new Vector3(
                .5f + ToFractionalFloat(x),
                .5f + ToFractionalFloat(y),
                .5f + ToFractionalFloat(z)
                );
             * */
        }

    }
}
