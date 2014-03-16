using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Headers;

namespace Aquarium.GA.Phenotypes
{

    public class NeuralNetworkPhenotype : ComponentPheno, INeuralNetworkPhenotype
    {

        public int NumHidden { get; set; }

        public int NumInputs { get; set; }

        public int NumOutputs { get; set; }

        public double[] Weights { get; set; }




        public NeuralNetworkPhenotype(NeuralNetworkHeader header)
        {
            NumInputs = header.NumInputs;
            NumHidden = header.NumHidden;
            NumOutputs = header.NumOutputs;
            Weights = header.Weights;
        }

    }

}
