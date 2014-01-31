using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forever.Neural
{
    class NeuronLayer
    {
        public Neuron[] Neurons { get; set; }

        public NeuronLayer(int numNuerons, int numInputsPer, Random random = null)
        {
            Neurons = new Neuron[numNuerons];
            for (int i = 0; i < numNuerons; i++)
            {
                Neurons[i] = new Neuron(numInputsPer, random);
            }

        }
    }
}
