using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.GA.GeneParsers
{
    public struct OrganHeader
    {   
        public static int Size { get { return 3; } }

        public int BodyPart;
        public int InputSignal;
        public int OutputSignal;

        public OrganHeader(int bodyPart, int inputSignal, int outputSignal)
        {
            BodyPart = Fuzzy.PositiveInteger(bodyPart);
            InputSignal = Fuzzy.PositiveInteger(inputSignal);
            OutputSignal = Fuzzy.PositiveInteger(outputSignal);
        }

        public static OrganHeader FromGenes(List<int> partGene)
        {
            return new OrganHeader(
                    partGene[0], 
                    partGene[1],
                    partGene[2]
                    );
        }
    }
}
