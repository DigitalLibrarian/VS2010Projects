using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.Neural;
using Aquarium.GA.Signals;
using Aquarium.GA.Bodies;

namespace Aquarium.GA.Organs
{

    public class NeuralOrgan : IOOrgan
    {
        override public OrganType OrganType { get { return OrganType.Neural; } }
        public NeuralNetwork Network { get; private set; }

        public NeuralOrgan(BodyPart bodyPart, NeuralNetwork network)
            : base(bodyPart)
        {
            Network = network;
        }

        public override int NumInputs
        {
            get { return Network.NumInputs; }
        }

        public override int NumOutputs
        {
            get { return Network.NumOutputs; }
        }

        public override void ReceiveSignal(Signal signal)
        {
            if (signal.Band != Network.NumInputs) throw new SignalOutOfBandException();
            base.ReceiveSignal(signal);

            OutputSignal = new Signal(Network.ComputeOutputs(LastInput.Value.ToArray()).ToList());


        }

        

    }

}
