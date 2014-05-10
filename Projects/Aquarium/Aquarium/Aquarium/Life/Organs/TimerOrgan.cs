using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.Life.Signals;
using Aquarium.Life.Bodies;

namespace Aquarium.Life.Organs.OrganAbilities
{
    class TimerOrgan : IOOrgan
    {
        public TimerOrgan(BodyPart part, int hz, int outputBand)
            : base(part)
        {

            Hz = hz;
            OutputBand = outputBand;

            RecordedSlice = 0;
            
            OnSignal = new Signal(new List<double>(Enumerable.Repeat<double>(1, OutputBand)));
            OffSignal = new Signal(new List<double>(Enumerable.Repeat<double>(0, OutputBand)));
        }
        
        protected int OutputBand { get; private set; }
        protected int Hz { get; private set; }
        protected Signal OnSignal { get; private set; }
        protected Signal OffSignal { get; private set; }

        protected float Slice { get { return 1f / (float)Hz; } }
        protected float RecordedSlice { get; set; }


        public override int NumInputs
        {
            get { return 0; }
        }

        public override int NumOutputs
        {
            get { return OutputBand; }
        }



        public override OrganType OrganType
        {
            get { return Organs.OrganType.Timer; }
        }


        public override void Update(NervousSystem nervousSystem, float duration)
        {
            //duration is milliseconds
            RecordedSlice += (duration * 0.001f);
            Signal toFire;
            if (RecordedSlice > Slice)
            {
                RecordedSlice = 0;
                toFire = OnSignal;
            }
            else
            {
                toFire = OffSignal;
            }

            //TODO - experiment with input signal affecting output signal for timed signal relays

            OutputWriter.Write(toFire);
        }
    }
}
