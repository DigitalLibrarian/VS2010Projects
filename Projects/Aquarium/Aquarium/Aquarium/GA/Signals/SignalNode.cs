﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.GA.Signals
{
    abstract public class SignalNode : ISignalConsumer, ISignalProducer
    {
        public Signal LastInput { get; private set; }
        public Signal OutputSignal { get; protected set; }

        public virtual void ReceiveSignal(Signal signal)
        {
            LastInput = signal;
        }

        public Signal ProduceSignal()
        {
            return OutputSignal;
        }
    }

}
