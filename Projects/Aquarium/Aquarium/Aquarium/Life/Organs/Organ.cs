using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.Neural;
using Aquarium.Life.Signals;
using Aquarium.Life.Bodies;
using Forever.Physics;

namespace Aquarium.Life.Organs
{
    abstract public class Organ : SignalNode 
    {
        #region madness
        /*
         * 
         * 

A processor organ is one that is all inputs and outputs around a single network or memory node.


It consists of body parts that can also be rendered.

The body parts will get their meaning from the organism's genome and how it is controlled or used by the nervous system.


It should have a well thought formula for it's genetic cost
factors:
 - volume
 - calculated energy cost (noticed by g.a. selection)
 - calculated activation energy cost
 - damage rating (scaled cost)
 - combined neural system costs
 - has ability (big cost)

Organs can be "fired" by the nervous system.
 - A sense organ can be queried to provide input from the neuros
 - A limb can be kicked

Abilities: 

Each organ only gets one, if any and they cost
Active abilities 
  - influenced by the nervous system by the organ "firing"
  - can receive input (like a query). one number can be interpreted as an index into a list of local organisms sorted by distance (big cost)
  - always produce output
  - collect info from the world in real time and remember it  (sensory memory) to be  provided as input to nervous system when "firing" occurs
  - pulled from a set of swappable implementations :
  - limb can be kicked
  - current position query
  - current velocity query
  - current orientation query
  - current energy query
  - current energy change momentum query (track energy changes and make them bleed between values according to a curve)
  - my object query
  - nearest objects query (parameterized)
  - am touched
  - suck leftover energy from dead thing 
  - receive energy from environment
  - reward gland, causes back propagation on all processing units for the last X amount of time.  X relates to genetic cost

Inactive abilities
  - not influenced by the organ firing
  - might be triggered when another body collides,  like damage them or transfering information
  - transfer the output of a processor organ
  - transfer genome (penis organ teehee)

Each sensory organ must  have a processor in front of it's input, and also behind it's output
 - Or at least give this high fitness score/incentive

         * 
         * */

        #endregion

        public virtual bool HasAbility { get { return false; } }

        public BodyPart Part { get; private set; }
        abstract public OrganType OrganType { get; }
        public Organ(BodyPart bodyPart)
        {
            Part = bodyPart;
        }

        public abstract void Update(NervousSystem nervousSystem);

        public Signal LastInput { get; private set; }
        public Signal OutputSignal { get; protected set; }

        public int Band { get { return LastInput.Value.Count(); } }

        public event Action OnReceive;

        public virtual IForceGenerator ForceGenerator { get { return null; } }

        public virtual void ReceiveSignal(NervousSystem nervousSystem, Signal signal)
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
    public abstract class IOOrgan : Organ
    {

        public ChannelReader InputReader { get; set; }
        public ChannelWriter OutputWriter { get; set; }

        abstract public int NumInputs { get; }
        abstract public int NumOutputs { get; }

        public IOOrgan(BodyPart bodyPart) : base(bodyPart) { }

        
        public override void Update(NervousSystem nervousSystem)
        {
            var readSignal = InputReader.Read();
            if (readSignal.Band > 0)
            {
                ReceiveSignal(nervousSystem, readSignal);
                OutputWriter.Write(OutputSignal);

            }


        }
    }



}
