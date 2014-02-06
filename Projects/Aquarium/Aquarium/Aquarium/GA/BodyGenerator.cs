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
        public Random Random { get; private set; }

        List<Func<BodyPart>> PartFactories { get; set; }
        public BodyGenerator()
        {
            Random =  new System.Random();
            PartFactories = ProduceLibraryOfParts();
        }

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

        public BodyPart GenerateBodyPart(Body body)
        {
            int epoch = 0;
            int maxEpoch = 25;

            while (epoch < maxEpoch)
            {
                int index = Random.Next(PartFactories.Count());
                if (!body.Parts.Any()) return PartFactories[index]();

                foreach (var part in body.Parts)
                {
                    foreach(var socket in part.Sockets)
                    {
                        if (!socket.HasAvailable) continue;

                        var test = PartFactories[index]();
                        test.Color = RandomColor();

                        if (MoveToFitLocalSocket(body, socket, test)) return test;

                    }
                }

                epoch++;
            }
            throw new Exception("There is no fit");
        }

      



        public Body GenerateBody(int numParts = 20)
        {
            var body = new Body();
            
            for (int i = 0; i < numParts; i++)
            {
                var test = GenerateBodyPart(body);
                if (!AutoConnectPartSockets(body, test))
                {
                    throw new Exception("It won't fit");
                }
            }
            
            if (body.Parts.Count() != numParts)
            {
                throw new Exception();
            }

            body.Parts.ForEach(part =>
            {
                for (int x = 0; x < 5; x++)
                {
                    int minBand = 3;
                    int maxBand = 16;
                    int numInputs = minBand + Random.Next(maxBand - minBand);
                    int numHidden = 10;
                    int numOutputs = minBand + Random.Next(maxBand - minBand);


                    var network = new NeuralNetwork(numInputs, numHidden, numOutputs);
                    List<double> input = new List<double>();
                    for (int i = 0; i < numInputs; i++)
                    {
                        input.Add(Random.NextDouble());
                    }
                    network.RandomizeWeights(Random);

                    var organ = new NeuralOrgan(part, network);
                    organ.ReceiveSignal(new Signal(input));
                    part.AddOrgan(organ);

                    part.AddOrgan(new AbilityOrgan(part, new QueryPositionAbility(body)));
                }
                
            });


            body.NervousSystem = ConstructNS(body);
            
            return body;
        }

        private NervousSystem ConstructNS(Body body)
        {
            var nSystem = new NervousSystem(body);
            foreach (var part in body.Parts)
            {
                foreach (var organ in part.Organs)
                {
                    if (organ is IOOrgan)
                    {
                        var no = organ as IOOrgan;

                        if (no.OutputWriter == null)
                        {
                            var socket = Random.NextElement(part.Sockets.Where(s => !s.HasAvailable).ToList());
                            if (socket != null)
                            {
                                if (Random.Next(2) == 0) socket = socket.ForeignSocket;
                                var writer = socket.Part.ChanneledSignal.RegisterInputChannel(no.NumOutputs);
                                no.OutputWriter = writer;
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                        
                        if (no.InputReader == null)
                        {

                            var socket = Random.NextElement(part.Sockets.Where(s => !s.HasAvailable).ToList());
                            if (socket != null)
                            {
                                if (Random.Next(2) == 0) socket = socket.ForeignSocket;

                                var reader = socket.Part.ChanneledSignal.RegisterOutputChannel(no.NumInputs);
                                no.InputReader = reader;
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                       
                    }
                }
            }

            return nSystem;
        }

       

        public  static bool AutoConnectPartSockets(Body body, BodyPart part)
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
        
        private Color RandomColor()
        {
            var R = Random;
            return new Color((float)R.NextDouble(), (float)R.NextDouble(), (float)R.NextDouble(), (float)R.NextDouble());
        }

        #region BodyPart strategies

        private  const float SCALE = 1f;

         public static  BodyPart ChiefPart()
        {

            var chiefPart = new TestBodyPart();
            chiefPart.UCTransform = Matrix.CreateScale(SCALE * .5f);
           // chiefPart.Color = RandomColor();

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
            //part.Color = RandomColor();

            part.Sockets.Add(new BodyPartSocket(part, Vector3.UnitY, Vector3.UnitY));
            part.Sockets.Add(new BodyPartSocket(part, -Vector3.UnitY, -Vector3.UnitY));

            return part;
        }


         public static  BodyPart FlatShape()
        {
            var part = new TestBodyPart();

            var scale = SCALE * 1f;
            part.UCTransform = Matrix.CreateScale(scale, scale / 10f, scale);
            //part.Color = RandomColor();
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
           // part.Color = RandomColor();

            part.Sockets.Add(new BodyPartSocket(part, Vector3.UnitY, Vector3.UnitY));
            part.Sockets.Add(new BodyPartSocket(part, -Vector3.UnitY, -Vector3.UnitY));

            return part;
        }


         public static  BodyPart BigFlatShape()
        {
            var part = new TestBodyPart();

            var scale = SCALE * 2f;
            part.UCTransform = Matrix.CreateScale(scale, scale / 10f, scale);
            //part.Color = RandomColor();
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
           // part.Color = RandomColor();
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
            //part.Color = RandomColor();


            part.Sockets.Add(new BodyPartSocket(part, Vector3.UnitY, Vector3.UnitY));
            part.Sockets.Add(new BodyPartSocket(part, -Vector3.UnitY, -Vector3.UnitY));


            part.Sockets.Add(new BodyPartSocket(part, Vector3.UnitX, Vector3.UnitX));
            part.Sockets.Add(new BodyPartSocket(part, -Vector3.UnitX, -Vector3.UnitX));


            part.Sockets.Add(new BodyPartSocket(part, Vector3.UnitZ, Vector3.UnitZ));
            part.Sockets.Add(new BodyPartSocket(part, -Vector3.UnitZ, -Vector3.UnitZ));
            return part;
        }

         public static  BodyPart SpikeyThingLeft()
        {
            var part = new TestBodyPart();

            var scale = SCALE * 0.5f;

            AddAllCorners(part);
            part.UCTransform = Matrix.CreateScale(scale) * new Matrix(
                1f, 0f, 1f, 1f,
                0f, 1f, 0.5f, 1f,
                0f, 0f, 0.5f, 0,
                0f, 0f, 0f, 1f
                );
            

            return part;
        }

         public static  BodyPart SpikeyThingRight()
        {
            var part = new TestBodyPart();
            var scale = SCALE * 0.5f;

            AddAllCorners(part);

            part.UCTransform =  Matrix.CreateScale(scale) * new Matrix(
                1f, 0f, -1f, -1f,
                0f, 1f, -0.5f, -1f,
                0f, 0f, 1f, 0,
                0f, 0f, 0f, -1f
                );
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

           // PartFactories.Add(SpikeyThingLeft);
           // PartFactories.Add(SpikeyThingRight);

            return lib;
        }
        #endregion



    }
}
