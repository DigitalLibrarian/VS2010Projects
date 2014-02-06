using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.GA.Codons
{
    public class BodyPartStartCodon : BodyCodon
    {
        public override int FrameSize
        {
            get { return 3; }
        }

        public override bool Recognize(List<double> genes)
        {
            var one = genes[0];
            var two = genes[1];
            var three = genes[2];

            return one > 100 && two > 100 && three > 100;
        }

        public override List<double> Example()
        {
            return new List<double> { 150, 150, 150 };
        }

       
    }

    public class BodyPartEndCodon : BodyCodon
    {   
        public override int FrameSize
        {
            get { return 3; }
        }

        public override bool Recognize(List<double> genes)
        {
            var one = genes[0];
            var two = genes[1];
            var three = genes[2];

            return one > 100 && two < 100 && three < 100;
        }

        public override List<double> Example()
        {
            return new List<double> { 150, 50, 150 };
        }


    }



}
