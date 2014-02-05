using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.GA.Signals
{
    abstract public class SignalNode 
    {
        public Signal LastInput { get; private set; }
        public Signal OutputSignal { get; protected set; }

        public int Band { get { return LastInput.Value.Count(); } }

        public event Action OnReceive;

        public virtual void ReceiveSignal(Signal signal)
        {
            if (signal == null)
            {
                throw new Exception();
            }
            LastInput = signal;
            if (OnReceive != null)
            {
                OnReceive();
            }
        }
    }

    

    

}
