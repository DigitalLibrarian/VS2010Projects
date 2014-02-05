using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Aquarium.GA.Signals;

namespace Aquarium.GA.Bodies
{
    public class BodyPartSocket 
    {
        public BodyPartSocket(BodyPart part, Vector3 bodyPartUCPosition, Vector3 bodyUCNormal) : base()
        {
            Part = part;
            _ucPos = bodyPartUCPosition;
            _ucNorm = bodyUCNormal;
        }


        public BodyPartSocket ForeignSocket
        {
            get;
            private set;
        }

        public void ConnectSocket(BodyPartSocket socket)
        {
            if (ForeignSocket != null) throw new Exception();
            var sPart = socket.Part;
            ForeignSocket = socket;
            socket.ForeignSocket = this;
        }
               

        public BodyPart Part { get; private set; }

        Vector3 _ucPos;
        public Vector3 LocalPosition { 
            get 
            { 
                return Vector3.Transform(_ucPos, Part.BodyWorld);
            } 
        }

        public Vector3 UnitCubePosition { get { return _ucPos; } set { _ucNorm = value; } }

        Vector3 _ucNorm;
        public Vector3 Normal { get { return Vector3.Transform(_ucNorm, Part.BodyWorld); } }
        public Vector3 UnitCubeNormal { get { return _ucNorm; } set { _ucNorm = value; } }

        public bool Blocked { get; set; }

        public bool HasAvailable { get { return ForeignSocket == null && !Blocked; } }
     

    }
}
