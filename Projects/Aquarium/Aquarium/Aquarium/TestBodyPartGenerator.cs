using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Bodies;
using Microsoft.Xna.Framework;

namespace Aquarium
{
    public class TestBodyPartGenerator
    {
        Random Random = new Random();

        List<Func<BodyPart>> Samples { get; set; }
        public TestBodyPartGenerator()
        {
            Samples = new List<Func<BodyPart>>();

            RegisterSamples();
        }

        public BodyPart Generate(Body body)
        {
            int epoch = 0;
            int maxEpoch = 500;

            while (true || epoch < maxEpoch)
            {
                int index = Random.Next(Samples.Count());

              
                if (!body.Parts.Any()) return Samples[index]();

                foreach (var part in body.Parts)
                {
                    foreach(var socket in part.Sockets)
                    {
                        if (!socket.HasAvailable) continue;

                        var test = Samples[index]();
                        var tempSockets = test.Sockets;
                        var tempUCT = test.UCTransform;


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
                            test.Rotation *= rotate;

                            if (float.IsNaN(testSocket.LocalPosition.X)) throw new Exception();

                            var a = part.LocalPosition; // from body to part
                            var b = socket.LocalPosition; // from part to socket
                            var c = testSocket.LocalPosition; // from free part to it's socket
                            if (float.IsNaN(testSocket.LocalPosition.X)) throw new Exception();

                            test.LocalPosition = a + b - c;
                            
                            if (body.WillFit(socket, testSocket))
                            {
                                // update all the socket positions and normals, in case this one works
                                test.Sockets.ForEach(x => x.RotateWithBody(rotate));
                                return test;
                            }

                        }


                        test.Sockets = tempSockets;
                        test.UCTransform = tempUCT;
                    }
                }

                epoch++;
            }
            throw new Exception("There is no fit");
        }

      

        private Color RandomColor()
        {
            var R = Random;
            return new Color((float)R.NextDouble(), (float)R.NextDouble(), (float)R.NextDouble(), (float)R.NextDouble());
        }

        float SCALE = 1f;

        private BodyPart ChiefPart()
        {

            var chiefPart = new TestBodyPart();
            chiefPart.UCTransform = Matrix.CreateScale(SCALE * .5f);
            chiefPart.Color = RandomColor();

            chiefPart.Sockets.Add(new BodyPartSocket(chiefPart, Vector3.UnitX, Vector3.UnitX));
            chiefPart.Sockets.Add(new BodyPartSocket(chiefPart, -Vector3.UnitX, -Vector3.UnitX));

            chiefPart.Sockets.Add(new BodyPartSocket(chiefPart, Vector3.UnitY, Vector3.UnitY));
            chiefPart.Sockets.Add(new BodyPartSocket(chiefPart, -Vector3.UnitY, -Vector3.UnitY));

            chiefPart.Sockets.Add(new BodyPartSocket(chiefPart, Vector3.UnitZ, Vector3.UnitZ));
            chiefPart.Sockets.Add(new BodyPartSocket(chiefPart, -Vector3.UnitZ, -Vector3.UnitZ));
            return chiefPart;
        }

        private BodyPart BarShape()
        {
            var part = new TestBodyPart();

            var scale = SCALE * 0.75f;
            part.UCTransform = Matrix.CreateScale(scale / 10f, scale, scale / 10f);
            part.Color = RandomColor();

            part.Sockets.Add(new BodyPartSocket(part, Vector3.UnitY, Vector3.UnitY));
            part.Sockets.Add(new BodyPartSocket(part, -Vector3.UnitY, -Vector3.UnitY));

            return part;
        }


        private BodyPart FlatShape()
        {
            var part = new TestBodyPart();

            var scale = SCALE * 1f;
            part.UCTransform = Matrix.CreateScale(scale, scale / 10f, scale);
            part.Color = RandomColor();
            var up = Vector3.Up;
            var down = Vector3.Down;

            var left = Vector3.Left;
            var right = Vector3.Right;

            part.Sockets.Add(new BodyPartSocket(part, left, left));
            part.Sockets.Add(new BodyPartSocket(part, right, right));
            
            return part;
        }

        private BodyPart BoxWithEdges()
        {

            var part = new TestBodyPart();

            var scale = SCALE * 1f;
            part.UCTransform = Matrix.CreateScale(scale, scale, scale);
            part.Color = RandomColor();
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


        private BodyPart OblongBlock()
        {
            var part = new TestBodyPart();

            var scale = SCALE * 0.75f;

            part.UCTransform = Matrix.CreateScale(scale / 4f, scale, scale / 4f);
            part.Color = RandomColor();


            part.Sockets.Add(new BodyPartSocket(part, Vector3.UnitY, Vector3.UnitY));
            part.Sockets.Add(new BodyPartSocket(part, -Vector3.UnitY, -Vector3.UnitY));


            part.Sockets.Add(new BodyPartSocket(part, Vector3.UnitX, Vector3.UnitX));
            part.Sockets.Add(new BodyPartSocket(part, -Vector3.UnitX, -Vector3.UnitX));


            part.Sockets.Add(new BodyPartSocket(part, Vector3.UnitZ, Vector3.UnitZ));
            part.Sockets.Add(new BodyPartSocket(part, -Vector3.UnitZ, -Vector3.UnitZ));
            return part;
        }

        private BodyPart SpikeyThingLeft()
        {
            var part = new TestBodyPart();

            var scale = SCALE * 0.35f;

            AddAllCorners(part);
            part.UCTransform = Matrix.CreateScale(scale) * new Matrix(
                1f, 0f, 1f, 1f,
                0f, 1f, 0.5f, 1f,
                0f, 0f, 0.5f, 0,
                0f, 0f, 0f, 1f
                );
            

            part.Color = RandomColor();
            return part;
        }

        private BodyPart SpikeyThingRight()
        {
            var part = new TestBodyPart();
            var scale = SCALE * 0.35f;

            AddAllCorners(part);

            part.UCTransform =  Matrix.CreateScale(scale) * new Matrix(
                1f, 0f, -1f, -1f,
                0f, 1f, -0.5f, -1f,
                0f, 0f, 1f, 0,
                0f, 0f, 0f, -1f
                );
            part.Color = RandomColor();
            return part;
        }


        private void AddAllCorners(TestBodyPart part)
        {
            var corners = part.UnitCubeBB().GetCorners();
            foreach (var corner in corners)
            {
                var normal = corner;
                normal.Normalize();
                part.Sockets.Add(new BodyPartSocket(part, corner, normal));
            }
        }

        private BodyPart BoxWithCorners()
        {
            var part = new TestBodyPart();
            part.Color = RandomColor();
            var scale = SCALE * 0.35f;
            part.UCTransform = Matrix.Identity;

            AddAllCorners(part);
            part.UCTransform = Matrix.CreateScale(scale);
            return part;

        }

        private void RegisterSamples()
        {

            
            Samples.Add(BarShape);
            
            Samples.Add(FlatShape);
            
            Samples.Add(ChiefPart);
            
            Samples.Add(BoxWithEdges);
            Samples.Add(BoxWithCorners);
            
            Samples.Add(OblongBlock);

            Samples.Add(SpikeyThingLeft);
            Samples.Add(SpikeyThingRight);
        }

    }
}
