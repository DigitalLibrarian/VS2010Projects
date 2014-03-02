using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.GA.Headers
{
    public struct NeuralNetworkHeader
    {

        public const int MaxInputs = 4;
        public const int MaxHidden = 4;
        public const int MaxOutputs = 4;

        public static int Size { get { return 6 + (ComputeNumWeights(MaxInputs, MaxHidden, MaxOutputs) * 2); } }

        public int BodyPartPointer;
        public int OrganPointer;

        public int NumInputs;
        public int NumHidden;
        public int NumOutputs;

        //TODO - add rng seed from read method as property like others

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


        public static NeuralNetworkHeader FromGenes(List<int> genes)
        {


            var bodyPart = Fuzzy.PositiveInteger(genes[0]);
            var organNum = Fuzzy.PositiveInteger(genes[1]);

            var numInputs = Fuzzy.InRange(genes[2], 0, MaxInputs);
            var numHidden = Fuzzy.InRange(genes[3], 0, MaxHidden);
            var numOutputs = Fuzzy.InRange(genes[4], 0, MaxOutputs);

            numInputs = Math.Max(numInputs, 1);
            numOutputs = Math.Max(numOutputs, 1);

            var rngSeed = Fuzzy.PositiveInteger(genes[5]);
            
            int numWeights = NeuralNetworkHeader.ComputeNumWeights(numInputs, numHidden, numOutputs);
            int numNeeded = numWeights * 2;

            var weights = new double[numWeights];

            var index = 6;


            for (int i = 0; i < numWeights; i++)
            {
                var one = genes[index];
                var two = genes[index + 1];
                weights[i] = Fuzzy.TwoComponentDouble(one, two);

                index+=2;
            }

            return new NeuralNetworkHeader(bodyPart, organNum, numInputs, numHidden, numOutputs, weights);
        }


    }
}
