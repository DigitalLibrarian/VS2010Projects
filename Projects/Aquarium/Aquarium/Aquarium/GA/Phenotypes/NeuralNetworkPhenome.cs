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


        public IInstancePointer BodyPartPointer
        {
            get;
            set;
        }

        public IInstancePointer OrganPointer
        {
            get;
            set;
        }



        public NeuralNetworkPhenotype(NeuralNetworkHeader header)
        {
            BodyPartPointer = new InstancePointer(header.BodyPartPointer);
            OrganPointer = new InstancePointer(header.OrganPointer);
            NumInputs = header.NumInputs;
            NumHidden = header.NumHidden;
            NumOutputs = header.NumOutputs;
            Weights = header.Weights;
        }

    }

}
