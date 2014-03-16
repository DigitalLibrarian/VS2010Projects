using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Aquarium.Life.Signals
{
    public static class SignalEncoding
    {
        public static Signal Empty(int band)
        {
            return new Signal(new List<double>(new double[band]));
        }

        public static List<double> Encode(Vector3 v)
        {
            var list = new List<double>();
            
            list.InsertRange(list.Count, Encode(v.X));
            list.InsertRange(list.Count, Encode(v.Y));
            list.InsertRange(list.Count, Encode(v.Z));
            return list;
        }

        public static List<double> Encode(float f)
        {
            // raise the number by a some digits so that there 
            // is more good information in the integater component


            f *= 1000f;

            // get integer component
            float iCom = (int)f;


            // now put significant digits in the fraction component
            // then some m0ore to capture more
            iCom /= 100000f;

            // lop off the integer part
            iCom = iCom - ((int)iCom);

            // what remains are these digits xxxDDD.dddxxx and they are expressed as 0.DDDddd00 
            return new List<double>
            {
                (double) f
            };
        }


    }
}
