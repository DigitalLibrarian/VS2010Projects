using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.GA.Signals
{

    public class ChannelReader
    {
        ChanneledSignal Signal { get; set; }
        int Channel { get; set; }
        internal ChannelReader(ChanneledSignal signal, int channel)
        {
            Signal = signal;
            Channel = channel;
        }

        public Signal Read()
        {
            return Signal.ReadChannel(Channel);
        }

    }
}
