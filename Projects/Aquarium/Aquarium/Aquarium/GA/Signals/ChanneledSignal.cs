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



    public class ChanneledSignal : Signal
    {
        Dictionary<int, Tuple<int, int>> inputRegs = new Dictionary<int, Tuple<int, int>>();
        Dictionary<int, Tuple<int, int>> outputRegs = new Dictionary<int, Tuple<int, int>>();

        int _nextInChannel = 0;
        int _nextOutChannel = 0;
        int _totIn = 0;
        int _totOut = 0;

        public ChanneledSignal(List<double> inputs) : base(inputs) { }

        public ChannelWriter RegisterInputChannel(int num)
        {
            int channel = _nextInChannel++;

            RegisterChannel(_totIn, channel, num, inputRegs);

            _totIn += num;
            AccomodateSpace();
            return new ChannelWriter(this, channel);
        }


        public ChannelReader RegisterOutputChannel(int num)
        {
            int channel = _nextOutChannel++;
            var start = _totOut;

            RegisterChannel(start, channel, num, outputRegs);

            _totOut += num;
            AccomodateSpace();

            return new ChannelReader(this, channel);
        }

        private int RegisterChannel(int start, int channel, int num, Dictionary<int, Tuple<int, int>> regs)
        {
            
            regs.Add(channel, new Tuple<int,int>(start, num));

            return channel;
        }

        public void AccomodateSpace()
        {
            var size = Math.Max(_totIn, _totOut);
            if (Value.Count() < size)
            {
                var diff = size - Value.Count();
                for (int i = 0; i < diff; i++) Value.Add(0);
            }
        }

        internal void WriteChannel(int channel, Signal signal)
        {
            var reg  = inputRegs[channel];
            var inSig = 0;
            for (int i = reg.Item1; i < reg.Item1 +  reg.Item2; i++)
            {
                Value[i] = signal.Value[inSig++];
            }
        }


        internal Signal ReadChannel(int channel)
        {
            var reg = outputRegs[channel];
            var start = reg.Item1;
            var num = reg.Item2;

            var list = Value.GetRange(start, num);

            return new Signal(list);
        }
    }






    public class  SignalOutOfBandException : Exception {}
  
}
