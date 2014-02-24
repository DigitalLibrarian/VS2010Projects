using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Signals;
using Aquarium.GA.Bodies;
using Microsoft.Xna.Framework;

namespace Aquarium.GA.Organs.OrganAbilities
{
    public class ThrusterAbility : OrganAbility
    {
        public ThrusterAbility(int param0)
            : base(param0)
        {
            SocketId = param0;
        }

        int SocketId { get; set; }

        public override int NumInputs
        {
            get { return 1; }
        }

        public override int NumOutputs
        {
            get { return 1; }
        }

        public override Signal Fire(NervousSystem nervousSystem, Organ parent, Signal signal)
        {
            var num = signal.Value[0];
            var result = 0;
            if (num > 0.5)
            {
                var rigidBody = nervousSystem.Organism.RigidBody;
                var socket = Fuzzy.CircleIndex(parent.Part.Sockets, SocketId);


                var body = nervousSystem.Organism.Body;
                var part = parent.Part;

                var dir = part.LocalPosition;
                if (dir.LengthSquared() == 0)
                {
                    dir = socket.Normal; 
                    

                    var mag = 0.0001f * nervousSystem.Organism.RigidBody.Mass;
                    var veloCap = 0.01f;

                    if (rigidBody.Velocity.Length() < veloCap)
                    {
                        rigidBody.addForce(dir * mag, body.Position + Vector3.Forward);
                        result = 1;
                    }
                }
                else
                {
                 
                    var mag = 0.001f * nervousSystem.Organism.RigidBody.Mass;
                    var veloCap = 0.01f;

                    if (rigidBody.Velocity.Length() < veloCap)
                    {
                        rigidBody.addForce(dir * mag);
                    }
                }
            }

            return new Signal(new List<double> { result });
        }
    }
}
