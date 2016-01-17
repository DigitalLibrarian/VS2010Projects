using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.Life.Headers
{
    public struct NeuralNetworkHeader
    {

        public const int MaxInputs = 4;
        public const int MaxHidden = 4;
        public const int MaxOutputs = 4;

        public static int Size { get { return 3 + (ComputeNumWeights(MaxInputs, MaxHidden, MaxOutputs) * 2); } }

        public int NumInputs;
        public int NumHidden;
        public int NumOutputs;
        
        public double[] Weights;

        public NeuralNetworkHeader(int numInputs, int numHidden, int numOutputs, double[] weights)
        {
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



            var numInputs = Fuzzy.InRange(genes[0], 0, MaxInputs);
            var numHidden = Fuzzy.InRange(genes[1], 0, MaxHidden);
            var numOutputs = Fuzzy.InRange(genes[2], 0, MaxOutputs);

            numInputs = Math.Max(numInputs, 1);
            numOutputs = Math.Max(numOutputs, 1);

            var rngSeed = Fuzzy.PositiveInteger(genes[3]);
            
            int numWeights = NeuralNetworkHeader.ComputeNumWeights(numInputs, numHidden, numOutputs);
            int numNeeded = numWeights * 2;

            var weights = new double[numWeights];

            var index = 4;


            for (int i = 0; i < numWeights; i++)
            {
                var one = genes[index];
                var two = genes[index + 1];
                weights[i] = Fuzzy.TwoComponentDouble(one, two);

                index+=2;
            }

            return new NeuralNetworkHeader(numInputs, numHidden, numOutputs, weights);
        }


    }
}
