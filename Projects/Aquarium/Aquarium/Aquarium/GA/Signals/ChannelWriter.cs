
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.GA.Signals
{
    public class ChannelWriter
    {
        ChanneledSignal Signal { get; set; }
        int Channel { get; set; }
        internal ChannelWriter(ChanneledSignal signal, int channel)
        {
            Signal = signal;
            Channel = channel;
        }

        public void Write(Signal signal)
        {
            Signal.WriteChannel(Channel, signal);
        }

    }
}
