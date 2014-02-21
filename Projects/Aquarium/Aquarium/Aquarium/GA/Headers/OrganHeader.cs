using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.GA.Headers
{
    public struct OrganHeader
    {   
        public static int Size { get { return 5; } }

        public int OrganType;
        public int BodyPart;
        public int InputSignal;
        public int OutputSignal;
        public int AbilityId;

        public OrganHeader(int organType, int bodyPart, int inputSignal, int outputSignal, int abilityId)
        {
            OrganType = Fuzzy.PositiveInteger(organType);
            BodyPart = Fuzzy.PositiveInteger(bodyPart);
            InputSignal = Fuzzy.PositiveInteger(inputSignal);
            OutputSignal = Fuzzy.PositiveInteger(outputSignal);
            AbilityId = Fuzzy.PositiveInteger(abilityId);
        }

        public static OrganHeader FromGenes(List<int> partGene)
        {
            return new OrganHeader(
                    partGene[0], 
                    partGene[1],
                    partGene[2],
                    partGene[3],
                    partGene[4]
                    );
        }
    }
}
