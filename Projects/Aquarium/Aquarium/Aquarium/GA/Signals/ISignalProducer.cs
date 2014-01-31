using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.GA.Signals
{
    interface ISignalProducer
    {
        Signal ProduceSignal();
    }
}
