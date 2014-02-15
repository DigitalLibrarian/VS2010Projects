using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Aquarium.GA.Organs;
using Forever.Neural;
using Aquarium.GA.Signals;
using Aquarium.GA.Organs.OrganAbilities;

namespace Aquarium.GA.Bodies
{
    public class BodyGenerator
    {
        public static bool MoveToFitLocalSocket(Body body, BodyPartSocket socket, BodyPart test)
        {
            var tempSockets = test.Sockets;
            var tempUCT = test.UCTransform;
            var tempRot = test.Rotation;

            var part = socket.Part;
            // restore original part geometry
            test.UCTransform = tempUCT;


            // copy all the sockets over to fresh ones we can play with
            test.Sockets = new List<BodyPartSocket>();
            foreach (var fSocket in tempSockets)
            {
                var socketUCPos = fSocket.UnitCubePosition;
                var socketUCNormal = fSocket.UnitCubeNormal;
                test.Sockets.Add(new BodyPartSocket(test, socketUCPos, socketUCNormal));
            }

            for (int i = 0; i < test.Sockets.Count; i++)
            {
                // we are going to attempt  to connect  our local socket to this new one
                var testSocket = test.Sockets[i];

                if (float.IsNaN(testSocket.Normal.X)) throw new Exception();

                var fromLocalPart = socket.LocalPosition;
                var localSocketNormal = socket.Normal.Round(3);
                var foreignSocketNormal = testSocket.Normal.Round(3);
                var goalNormal = -localSocketNormal;

                if (float.IsNaN(testSocket.Normal.X)) throw new Exception();

                // figure  out how much rotation will be needed to align the contact normals
                Matrix rotate = Matrix.Identity;
                if (foreignSocketNormal == -localSocketNormal)
                    rotate = Matrix.Identity;
                else if (foreignSocketNormal == localSocketNormal)
                    rotate = Matrix.CreateFromAxisAngle(Vector3.Cross(foreignSocketNormal, -foreignSocketNormal), (float)Math.PI);
                else
                {
                    rotate = MathHelper.GetRotationAToB(foreignSocketNormal, goalNormal);
                    if (NaNny.IsNaN(rotate)) continue;
                }


                //now rotate the body part that much
                test.Rotation = rotate;

                if (float.IsNaN(testSocket.LocalPosition.X)) throw new Exception();

                var a = part.LocalPosition; // from body to part
                var b = socket.LocalPosition; // from part to socket
                var c = testSocket.LocalPosition; // from free part to it's socket
                if (float.IsNaN(testSocket.LocalPosition.X)) throw new Exception();


                test.LocalPosition = a + b - c;
                // we need to add a little so that there won't be overlap with the piece we are connecting with
                test.LocalPosition += socket.Normal * 0.001f;

                if (body.WillFit(socket, testSocket))
                {
                    return true;
                }

            }

            test.Rotation = tempRot;
            test.Sockets = tempSockets;
            test.UCTransform = tempUCT;
            return false;
        }



       

        public static bool AutoConnectPartSockets(Body body, BodyPart part)
        {
            if (!body.Parts.Any())
            {
                body.Parts.Add(part);
                return true;
            }

            foreach (var foreignSocket in part.Sockets)
            {
                if (!foreignSocket.HasAvailable) continue;
                foreach (var bPart in body.Parts)
                {
                    BodyPartSocket winner = bPart.Sockets.FirstOrDefault(socket => socket.HasAvailable && body.WillFit(socket, foreignSocket));
                    if (winner != null)
                    {
                        winner.ConnectSocket(foreignSocket);
                        body.Parts.Add(part);
                        return true;
                    }
                }
            }
            return false;

        }
        
      
        #region BodyPart strategies

        private  const float SCALE = 1f;

        public static BodyPart ChiefPart()
        {

            var chiefPart = new TestBodyPart();
            chiefPart.UCTransform = Matrix.CreateScale(SCALE * .5f);

            chiefPart.Sockets.Add(new BodyPartSocket(chiefPart, Vector3.UnitX, Vector3.UnitX));
            chiefPart.Sockets.Add(new BodyPartSocket(chiefPart, -Vector3.UnitX, -Vector3.UnitX));

            chiefPart.Sockets.Add(new BodyPartSocket(chiefPart, Vector3.UnitY, Vector3.UnitY));
            chiefPart.Sockets.Add(new BodyPartSocket(chiefPart, -Vector3.UnitY, -Vector3.UnitY));

            chiefPart.Sockets.Add(new BodyPartSocket(chiefPart, Vector3.UnitZ, Vector3.UnitZ));
            chiefPart.Sockets.Add(new BodyPartSocket(chiefPart, -Vector3.UnitZ, -Vector3.UnitZ));
            return chiefPart;
        }

        public static  BodyPart BarShape()
        {
            var part = new TestBodyPart();

            var scale = SCALE * 0.75f;
            part.UCTransform = Matrix.CreateScale(scale / 10f, scale * 2f, scale / 10f);

            part.Sockets.Add(new BodyPartSocket(part, Vector3.UnitY, Vector3.UnitY));
            part.Sockets.Add(new BodyPartSocket(part, -Vector3.UnitY, -Vector3.UnitY));

            return part;
        }


        public static  BodyPart FlatShape()
        {
            var part = new TestBodyPart();

            var scale = SCALE * 1f;
            part.UCTransform = Matrix.CreateScale(scale, scale / 10f, scale);
            var up = Vector3.Up;
            var down = Vector3.Down;

            var left = Vector3.Left;
            var right = Vector3.Right;

            part.Sockets.Add(new BodyPartSocket(part, left, left));
            part.Sockets.Add(new BodyPartSocket(part, right, right));
            
            return part;
        }

        public static  BodyPart BigBarShape()
        {
            var part = new TestBodyPart();

            var scale = SCALE * 2f;
            part.UCTransform = Matrix.CreateScale(scale / 10f, scale, scale / 10f);

            part.Sockets.Add(new BodyPartSocket(part, Vector3.UnitY, Vector3.UnitY));
            part.Sockets.Add(new BodyPartSocket(part, -Vector3.UnitY, -Vector3.UnitY));

            return part;
        }


        public static  BodyPart BigFlatShape()
        {
            var part = new TestBodyPart();

            var scale = SCALE * 2f;
            part.UCTransform = Matrix.CreateScale(scale, scale / 10f, scale);
            var up = Vector3.Up;
            var down = Vector3.Down;

            var left = Vector3.Left;
            var right = Vector3.Right;

            part.Sockets.Add(new BodyPartSocket(part, left, left));
            part.Sockets.Add(new BodyPartSocket(part, right, right));

            return part;
        }

        public static  BodyPart BoxWithEdges()
        {

            var part = new TestBodyPart();

            var scale = SCALE * 1f;
            part.UCTransform = Matrix.CreateScale(scale, scale, scale);
            var up = Vector3.Up;
            var down = Vector3.Down;

            var left = Vector3.Left;
            var right = Vector3.Right;


            var normal = up + left;
            part.Sockets.Add(new BodyPartSocket(part, up + left, normal));
            normal = up + right;
            part.Sockets.Add(new BodyPartSocket(part, up + right, normal));

            normal  = down + left;
            part.Sockets.Add(new BodyPartSocket(part, down + left, normal));
            normal = down + right;
            part.Sockets.Add(new BodyPartSocket(part, down + right, normal));

            left = Vector3.Forward;
            right = Vector3.Backward;

            normal = up + left;
            part.Sockets.Add(new BodyPartSocket(part, up + left, normal));
            normal = up + right;
            part.Sockets.Add(new BodyPartSocket(part, up + right, normal));

            normal = down + left;
            part.Sockets.Add(new BodyPartSocket(part, down + left, normal));
            normal = down + right;
            part.Sockets.Add(new BodyPartSocket(part, down + right, normal));


            return part;
        }


        public static  BodyPart OblongBlock()
        {
            var part = new TestBodyPart();

            var scale = SCALE * 0.75f;

            part.UCTransform = Matrix.CreateScale(scale / 4f, scale, scale / 4f);

            part.Sockets.Add(new BodyPartSocket(part, Vector3.UnitY, Vector3.UnitY));
            part.Sockets.Add(new BodyPartSocket(part, -Vector3.UnitY, -Vector3.UnitY));


            part.Sockets.Add(new BodyPartSocket(part, Vector3.UnitX, Vector3.UnitX));
            part.Sockets.Add(new BodyPartSocket(part, -Vector3.UnitX, -Vector3.UnitX));


            part.Sockets.Add(new BodyPartSocket(part, Vector3.UnitZ, Vector3.UnitZ));
            part.Sockets.Add(new BodyPartSocket(part, -Vector3.UnitZ, -Vector3.UnitZ));
            return part;
        }



        public static  void AddAllCorners(TestBodyPart part)
        {
            var corners = part.UnitCubeBB().GetCorners();
            foreach (var corner in corners)
            {
                var normal = corner;
                normal.Normalize();
                part.Sockets.Add(new BodyPartSocket(part, corner, normal));
            }
        }

        public static BodyPart BoxWithCorners()
        {
            var part = new TestBodyPart();
            var scale = SCALE * 0.5f;
            part.UCTransform = Matrix.Identity;

            AddAllCorners(part);
            part.UCTransform = Matrix.CreateScale(scale);
            return part;

        }

        public static List<Func<BodyPart>> ProduceLibraryOfParts()
        {
            var lib = new List<Func<BodyPart>>();


            lib.Add(ChiefPart);
            lib.Add(BarShape);
            lib.Add(FlatShape);
            lib.Add(BigBarShape);
            lib.Add(BigFlatShape);
            
            
            lib.Add(BoxWithEdges);
            lib.Add(BoxWithCorners);
            
            lib.Add(OblongBlock);

            return lib;
        }
        #endregion



    }
}
