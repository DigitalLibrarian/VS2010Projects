using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.GA.Codons
{
    public class BodyEndCodon : BodyCodon
    {    

        public override int FrameSize
        {
            get { return 6; }
        }

        public override bool Recognize(List<double> genes)
        {
            for (int i = 0; i < FrameSize; i++)
            {
                if (Math.Round(genes[i], 1) != 0)
                {
                    return false;
                }
            }
            return true;
        }

        public override List<double> Example()
        {
            var l = new List<double>();

            for (int i = 0; i < FrameSize; i++)
            {
                l.Add(0);
            }
            return l;

        }
    }
}
