using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.GA.GeneParsers
{
    public struct NeuralNetworkHeader
    {
        public static int Size { get { return 5; } }

        public int BodyPartPointer;
        public int OrganPointer;

        public int NumInputs;
        public int NumHidden;
        public int NumOutputs;

        public double[] Weights;

        public NeuralNetworkHeader(int bpp, int op, int numInputs, int numHidden, int numOutputs, double[] weights)
        {
            BodyPartPointer = bpp;
            OrganPointer = op;
            NumInputs = numInputs;
            NumHidden = numHidden;
            NumOutputs = numOutputs;
            Weights = weights;
        }


        public static  int ComputeNumWeights(int numInputs, int numHidden, int numOutputs)
        {
            return (numInputs * numHidden) + (numHidden * numOutputs) + numHidden + numOutputs;
        }


        public static NeuralNetworkHeader FromGenes(List<int> partGene)
        {
            const int maxInputs = 12;
            const int maxHidden = 10;
            const int maxOutputs = 12;


            var bodyPart = Fuzzy.PositiveInteger(partGene[0]);
            var organNum = Fuzzy.PositiveInteger(partGene[1]);

            var numInputs = Fuzzy.InRange(partGene[2], 0, maxInputs);
            var numHidden = Fuzzy.InRange(partGene[3], 0, maxHidden);
            var numOutputs = Fuzzy.InRange(partGene[4], 0, maxOutputs);



            int numWeights = NeuralNetworkHeader.ComputeNumWeights(numInputs, numHidden, numOutputs);
            int numNeeded = numWeights * 2;

            var weights = new double[numWeights];

            var index = 5;

            if (partGene.Count() - index < numNeeded)
            {
                return new NeuralNetworkHeader(bodyPart, organNum, numInputs, numHidden, numOutputs, null);
            }

            int w = 0;
            for (int i = 0; i < numWeights; i++)
            {
                weights[i] = Fuzzy.TwoComponentDouble(partGene[index], partGene[index + 1]);

                index+=2;
            }

            return new NeuralNetworkHeader(bodyPart, organNum, numInputs, numHidden, numOutputs, weights);
        }
    }
}
