﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.Neural;
using Aquarium.Life.Signals;
using Aquarium.Life.Bodies;

namespace Aquarium.Life.Organs
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

        public override void ReceiveSignal(NervousSystem nervousSystem, Signal signal)
        {
            if (signal.Band != Network.NumInputs) throw new SignalOutOfBandException();
            base.ReceiveSignal(nervousSystem, signal);

            var nextOutput = Network.ComputeOutputs(LastInput.Value.ToArray()).ToList();
            OutputSignal = new Signal(nextOutput);


        }

        

    }

}
