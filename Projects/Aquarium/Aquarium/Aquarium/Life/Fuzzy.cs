using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Aquarium.Life
{
    public static class Fuzzy
    {
        public static double TwoComponentDouble(int i1, int i2)
        {
            var precision = 1000;
            double d1 = (double)InRange(i1, 0, precision);
            double d2 = (double)InRange(i2, 0, precision);

            //i1 has major component
            //i2 has minor component
            //both signed

            return (d1 / (precision ))  + (d2 / (precision * precision ));


            //return (d1 + (d2 / precision));
        }


        public static T ScaledCircleIndex<T>(List<T> list, int index, int scale = 100)
        {
            return CircleIndex(list, index / scale);
        }



     
        public static T CircleIndex<T>(List<T> list, int index)
        {
            var count = list.Count();
            return list[index % count];
        }
        

        /// <summary>
        /// Returns positive integer in range (excluding high)
        /// </summary>
        /// <param name="i"></param>
        /// <param name="low"></param>
        /// <param name="high"></param>
        /// <returns></returns>
        public static int InRange(int i, int low, int high)
        {
            i = PositiveInteger(i);
                
            int diff = high - low;
            var mod = i % diff;
            return low + mod;
        }

        public static int PositiveInteger(int i)
        {
            return Math.Abs(i);
        }

        public static int RoundToInt(double d)
        {
            return (int)Math.Round(d, 0);
        }
        public static int PositiveInteger(double d)
        {
            var rounded = RoundToInt(d * 1000) / 1000;
            if (rounded < 0) rounded *= -1;
            return Math.Abs(rounded);
        }

        public static float PositiveFloat(double d)
        {
            return (float)Math.Abs(d);
        }

        public static float ColorValue(double d)
        {
            return ToFractionalFloat(d);
        }


        public static float ColorValue(int i)
        {
            return ToFractionalFloat(i) + 0.05f;
        }

        public static Color ToColor(double r, double g, double b)
        {
            return new Color(ColorValue(r), ColorValue(g), ColorValue(b));
        }

        public static Color ToColor(int r, int g, int b)
        {
            return new Color(ColorValue(r), ColorValue(g), ColorValue(b));
        }
        
        public static float ToFractionalFloat(double d)
        {
            return (float)(d - Math.Floor(d));
        }

        public static float ToFractionalFloat(int i)
        {
            double d = PositiveInteger(i);

            return ToFractionalFloat(d / 4);
        }

        public static Vector3 ToScaleVector(double x, double y, double z)
        {

            return new Vector3(
                .75f + ToFractionalFloat(x),
                .75f + ToFractionalFloat(y),
                .75f + ToFractionalFloat(z)
                );
        }



        public static Vector3 ToScaleVector(int x, int y, int z)
        {

            return new Vector3(
                .5f + ToFractionalFloat(x),
                .5f + ToFractionalFloat(y),
                .5f + ToFractionalFloat(z)
                );
        }
    }
}
