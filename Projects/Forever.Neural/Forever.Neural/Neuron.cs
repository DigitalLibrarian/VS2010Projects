using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forever.Neural
{
    public class Neuron
    {
        public double[] Weights { get; set; }
        public double Activation { get; set; }
        public double Error { get; set; }

        public Neuron(int numInputs, Random random = null)
        {
            int allocated = numInputs + 1;// extra for bias
            Weights = new double[allocated];
            for (int i = 0; i < allocated; i++)
            {
                if (random == null)
                {
                    Weights[i] = 0;
                }
                else
                {
                    Weights[i] = (random.NextDouble() - 0.5f) * 2f; //random clamped between -1, 1
                }
            }
        }
    }
}
