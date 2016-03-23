using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.Life.Spec
{
    public class NeuralNetworkSpec
    {
        public const int MaxInputs = 4;
        public const int MaxHidden = 4;
        public const int MaxOutputs = 4;

        public int OrganId { get; set; }
        public int NumInputs { get; set; }
        public int NumHidden { get; set; }
        public int NumOutputs { get; set; }
        public double[] Weights { get; set; }

        public static int ComputeNumWeights(int numInputs, int numHidden, int numOutputs)
        {
            return (numInputs * numHidden) + (numHidden * numOutputs) + numHidden + numOutputs;
        }

        public static NeuralNetworkSpec FromGenome(IEnumerator<int> g)
        {
            var organId = Fuzzy.PositiveInteger(g.Next());
            var numInputs = Fuzzy.InRange(Fuzzy.PositiveInteger(g.Next()), 1, MaxInputs);
            var numHidden = Fuzzy.InRange(Fuzzy.PositiveInteger(g.Next()), 0, MaxHidden);
            var numOutputs = Fuzzy.InRange(Fuzzy.PositiveInteger(g.Next()), 1, MaxOutputs);

            var numWeights = NeuralNetworkSpec.ComputeNumWeights(numInputs, numHidden, numOutputs);
            var weights = new double[numWeights];

            for (int i = 0; i < numWeights; i++)
            {
                weights[i] = Fuzzy.TwoComponentDouble(g.Next(), g.Next());
            }

            return new NeuralNetworkSpec
            {
                OrganId = organId,
                NumInputs = numInputs,
                NumHidden = numHidden,
                NumOutputs = numOutputs,
                Weights = weights
            };
        }
    }
}
