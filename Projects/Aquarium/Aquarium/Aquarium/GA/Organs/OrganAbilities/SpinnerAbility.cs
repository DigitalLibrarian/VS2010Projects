using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Signals;
using Aquarium.GA.Bodies;
using Microsoft.Xna.Framework;

namespace Aquarium.GA.Organs.OrganAbilities
{
    public class SpinnerAbility : SimpleFireAbility
    {
          public SpinnerAbility(int param0)
            : base(param0)
        {
            SocketId = param0;
        }

        int SocketId { get; set; }



        public override Signal Fire(NervousSystem nervousSystem, Organ parent, Signal signal)
        {
            var num = signal.Value[0];
            var result = 0;
            if (num > 0.5)
            {
                var rigidBody = nervousSystem.Organism.RigidBody;
                var socket = Fuzzy.CircleIndex(parent.Part.Sockets, SocketId);

                var body = nervousSystem.Organism.Body;
                var dir = socket.Normal;

                var mag = 0.0009f * nervousSystem.Organism.RigidBody.Mass;
                var veloCap = 0.001f;

                var torque = dir * mag;
                

                if (rigidBody.Rotation.Length() < veloCap)
                {
                    rigidBody.addTorque(torque);
                    result = 1;
                }
            }

            return new Signal(new List<double> { result });
        }
    }
}
