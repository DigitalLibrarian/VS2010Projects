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
        public override int NumInputs
        {
            get { return 1; }
        }

        public override int NumOutputs
        {
            get { return 1; }
        }

        public ThrusterAbility(int param0)
            : base(param0)
        {
            SocketId = param0;
        }

        int SocketId { get; set; }


        public override Signal Fire(NervousSystem nervousSystem, Organ parent, Signal signal, MutableForceGenerator fg)
        {
            var num = signal.Value[0];
            float result = 0;
            if (num > 0.5)
            {
                if (num > 1) num = 1;
                var rigidBody = nervousSystem.Organism.RigidBody;
                var socket = Fuzzy.CircleIndex(parent.Part.Sockets, SocketId);

                var dir = socket.Normal;
                dir = Vector3.Transform(dir, rigidBody.Orientation);

                var mag = 0.000001f * ((float) num) * nervousSystem.Organism.RigidBody.Mass;
                var veloCap = 0.01f;
                var bodyPressurePoint = Vector3.Transform(parent.Part.LocalPosition + socket.LocalPosition, rigidBody.World);


                if (rigidBody.Velocity.Length() < veloCap)
                {
                    var force = dir * mag;
                    var point = bodyPressurePoint;

                    fg.Force = force;
                    fg.Position = bodyPressurePoint;

                    result = 1;
                }
            }

            if (result != 1)
            {
                fg.Force = Vector3.Zero;
                fg.Position = null;
            }

            return new Signal(SignalEncoding.Encode(result));
        }
    }
}
