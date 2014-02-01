using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Aquarium.GA.Organs;
using Forever.Neural;

namespace Aquarium.GA.Bodies
{
    public class NervousSystem
    {
        public List<Organ> Organs { get; private set; }
        public RootNeuralOrgan RootNeuralOrgan { get; private set; }
        public Body Body { get; private set; }


        public NervousSystem(Body body, RootNeuralOrgan rootNeuralOrgan, List<Organ> organs)
        {
            Body = body;
            RootNeuralOrgan = rootNeuralOrgan;
            Organs = organs;
        }



        public void Update()
        {

            RootNeuralOrgan.Update(this);
            Organs.ForEach(organ => organ.Update(this));
        }

        public List<BodyPartSocket> GetSocketsInUse()
        {
            var sockets = new List<BodyPartSocket>();
            Body.Parts.ForEach(p => sockets.AddRange(p.Sockets.Where(x => !x.HasAvailable)));
            return sockets;
        }
    }
}
