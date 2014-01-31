using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.GA.Signals
{

    public class Signal
    {
        public List<double> Value { get; private set; }
        public int Band { get { return Value.Count(); } }
        public Signal(List<double> inputs)
        {
            Value = inputs;
        }
    }
    public class  SignalOutOfBandException : Exception {}
  
}
