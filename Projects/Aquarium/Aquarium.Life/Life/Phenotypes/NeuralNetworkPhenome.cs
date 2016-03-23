using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.Life.Spec;

namespace Aquarium.Life.Phenotypes
{
    public class NeuralNetworkPhenotype : ComponentPheno, INeuralNetworkPhenotype
    {
        public int NumHidden { get; set; }
        public int NumInputs { get; set; }
        public int NumOutputs { get; set; }
        public double[] Weights { get; set; }
        
        public NeuralNetworkPhenotype(NeuralNetworkSpec header)
        {
            NumInputs = header.NumInputs;
            NumHidden = header.NumHidden;
            NumOutputs = header.NumOutputs;
            Weights = header.Weights;
        }
    }
}
