using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.Life.Bodies;
using Forever.Neural;
using Forever.Physics;
using Aquarium.Life.Organs;
using Aquarium.Life.Signals;
using Microsoft.Xna.Framework;
using Aquarium.Life.Phenotypes;
using Aquarium.Life.Organs.OrganAbilities;

namespace Aquarium.Life.Phenotypes
{
       
    public class PhenotypeReader
    {
        BodyPartGenerator BodyGenerator { get; set; }
        public PhenotypeReader()
        {
            BodyGenerator = new BodyPartGenerator();
        }

        public Body ProduceBody(IBodyPhenotype bodyPheno)
        {
            //TODO - make so there is a pool of Networks and each neural organ point at it's nn

            if (!bodyPheno.BodyPartPhenos.Any())
            {
                return null;
            }
            var body = new Body();
            var partsToMake = bodyPheno.BodyPartPhenos.ToList();

            //Pre generate all the individual part geometry
            var gennedParts = bodyPheno.BodyPartPhenos.Select(partGenome => 
            {
                 var part = BodyGenerator.NewPartFromIndex(partGenome.BodyPartGeometryIndex);
                 part.UCTransform = Matrix.CreateScale(partGenome.Scale) * part.UCTransform;
                part.Color = partGenome.Color;
                part.Scale = partGenome.Scale;
                part.ChanneledSignal = new ChanneledSignal(new List<double>());
                return part;
            }).ToList();

            bool first = true;
            
            //connect body together
            foreach(var part in gennedParts)
            {
                var partGenome = partsToMake.First();
                partsToMake.Remove(partGenome);


                if (first)
                {
                    body.Parts.Add(part);
                }
                else
                {
                    var anchorPart = Fuzzy.ScaledCircleIndex(body.Parts, partGenome.AnchorPart.InstanceId);
                    if (!ConnectPartFromAnchor(body, anchorPart, partGenome, part, autoTryOthers: true))
                    {
                        // don't care under GA, means a loss  of the benfit of the limb
                        // todo - this should affect score
                    }

                }

            
                first = false;
            }
            var goodOrganPhenos = new List<IOrganPhenotype>();

            //connect organ phenos to parts
            foreach (var organPheno in bodyPheno.OrganPhenos)
            {
                var partId = organPheno.BodyPartPointer.InstanceId;
                var part = Fuzzy.ScaledCircleIndex(body.Parts, partId);
                goodOrganPhenos.Add(organPheno);
            }

            //need list of organs that will require channels
            var ioOrgans = new Dictionary<IOrganPhenotype, Organ>();
            //dictionary of type to list
            var typedOrganPhenos = ClassifyOrgans(goodOrganPhenos);




            if (typedOrganPhenos.ContainsKey(OrganType.Neural) && bodyPheno.NeuralNetworkPhenos.Any())
            {
                goodOrganPhenos = typedOrganPhenos[OrganType.Neural];


                foreach (var organPheno in goodOrganPhenos)
                {
                    var networkId = organPheno.ForeignId.InstanceId;
                    var partId = organPheno.BodyPartPointer.InstanceId;
                    var part = Fuzzy.ScaledCircleIndex(body.Parts, partId);
                    var nnPheno = Fuzzy.ScaledCircleIndex(bodyPheno.NeuralNetworkPhenos, networkId);

                    var network = new NeuralNetwork(nnPheno.NumInputs, nnPheno.NumHidden, nnPheno.NumOutputs);
                    network.SetWeights(nnPheno.Weights);


                    var organ = new NeuralOrgan(part, network);
                    part.AddOrgan(organ);
                    ioOrgans.Add(organPheno, organ);
                }

               

            } // do all neurals

            if (typedOrganPhenos.ContainsKey(OrganType.Ability))
            {
                goodOrganPhenos = typedOrganPhenos[OrganType.Ability];

                foreach (var organPheno in goodOrganPhenos)
                {
                    var rawAbilityId = organPheno.ForeignId.InstanceId;
                    var partId = organPheno.BodyPartPointer.InstanceId;
                    var part = Fuzzy.ScaledCircleIndex(body.Parts, partId);
                    var abilityParam0 = organPheno.AbilityParam0.InstanceId;
                    var abilityParam1 = organPheno.AbilityParam1.InstanceId;

                    var organAbility = GetOrganAbility(rawAbilityId, abilityParam0, abilityParam1);
                    var organ = new AbilityOrgan(part, organAbility);

                    part.AddOrgan(organ);
                    ioOrgans.Add(organPheno, organ);

                }
            }


            if (typedOrganPhenos.ContainsKey(OrganType.Timer))
            {
                goodOrganPhenos = typedOrganPhenos[OrganType.Timer];

                foreach (var organPheno in goodOrganPhenos)
                {
                    var rawAbilityId = organPheno.ForeignId.InstanceId;
                    var partId = organPheno.BodyPartPointer.InstanceId;
                    var part = Fuzzy.ScaledCircleIndex(body.Parts, partId);
                    var abilityParam0 = organPheno.AbilityParam0.InstanceId;
                    var abilityParam1 = organPheno.AbilityParam1.InstanceId;


                    int lowHz = 1;
                    int highHz = 10;
                    var hz  = Fuzzy.InRange(abilityParam0, lowHz, highHz);
                    int lowBand = 1;
                    int highBand = 20;

                    var outputBand = Fuzzy.InRange(abilityParam1, lowBand, highBand);


                    var organ = new TimerOrgan(part, hz, outputBand);

                    part.AddOrgan(organ);
                    ioOrgans.Add(organPheno, organ);

                }
            }


            // each one of the io organs must be connected to the neural grid
            foreach (var noPheno in ioOrgans.Keys)
            {
                var inputSignalId = noPheno.InputSignal.InstanceId;
                var outputSignalId = noPheno.OutputSignal.InstanceId;

                var organ = ioOrgans[noPheno] as IOOrgan;
                var part = organ.Part;


                var connectedSockets = part.Sockets.Where(s => !s.HasAvailable).ToList();
                var max = connectedSockets.Count() + 1;
                // 0 means me

                inputSignalId = Fuzzy.InRange(inputSignalId, 0, max);
                outputSignalId = Fuzzy.InRange(outputSignalId, 0, max);


                var signal = part.ChanneledSignal;
                if (connectedSockets.Any() && inputSignalId != 0)
                {
                    signal = Fuzzy.ScaledCircleIndex(connectedSockets, inputSignalId - 1).ForeignSocket.Part.ChanneledSignal;
                }
                var reader = signal.RegisterOutputChannel(organ.NumInputs);
                organ.InputReader = reader;

                signal = part.ChanneledSignal;
                if (connectedSockets.Any() && outputSignalId != 0)
                {
                    signal = Fuzzy.ScaledCircleIndex(connectedSockets, outputSignalId - 1).ForeignSocket.Part.ChanneledSignal;
                }
                var writer = signal.RegisterInputChannel(organ.NumOutputs);
                organ.OutputWriter = writer;
            }

 
            return body;
        }

        public Dictionary<OrganType, List<IOrganPhenotype>> ClassifyOrgans(List<IOrganPhenotype> phenos)
        {
            var dict = new Dictionary<OrganType, List<IOrganPhenotype>>();
            foreach (var pheno in phenos)
            {
                int rawTypeId = pheno.OrganType.InstanceId;

                var array = Enum.GetValues(typeof(OrganType));
                var list = new List<OrganType>();
                foreach(var ele in array) list.Add((OrganType)ele);
                var organType = Fuzzy.CircleIndex(list, rawTypeId);
                if (!dict.ContainsKey(organType)) dict.Add(organType, new List<IOrganPhenotype>());

                dict[organType].Add(pheno);

            }

            return dict;
        }


        public OrganAbility GetOrganAbility(int rawAbilityId, int abilityParam0, int abilityParam1)
        {
            var list = new List<Func<OrganAbility>>
            {
                () => new ThrusterAbility(abilityParam0),
                () => new SpinnerAbility(abilityParam0),
                () => new FoodBiterAbility(abilityParam0),
                () => new QueryClosestFoodAbility(abilityParam0),
                () => new QueryPositionAbility(abilityParam0),
                () => new QueryVelocityAbility(abilityParam0),
                () => new QueryEnergyRemainingAbility(abilityParam0)
            };

            return Fuzzy.CircleIndex(list, rawAbilityId)();
        }
        

        private bool ConnectPartFromAnchor(Body body, BodyPart anchorPart, IBodyPartPhenotype partGenome, BodyPart part, bool autoTryOthers=true)
        {
            var placement = partGenome.PlacementPartSocket;
            var socket = Fuzzy.ScaledCircleIndex(anchorPart.Sockets, placement.InstanceId);
            if (!socket.HasAvailable)
            {
                if(autoTryOthers)
                    socket = socket.Part.Sockets.FirstOrDefault(x => x.HasAvailable);
                else
                {
                    return false;
                }

            }

            if (socket != null)
            {
                if (MoveToFitLocalSocket(body, socket, part))
                {
                    return AutoConnectPartSockets(body, part);
                }
            }
            return false;
        }


        private BodyPartSocket FromSocketId(BodyPart  part, int index)
        {
            var max = part.Sockets.Count();
            return part.Sockets[index % max];
        }

        private T GetInstance<T>(int Id, Dictionary<int, T> dict, Func<T> fact)
        {
            if (dict.ContainsKey(Id))
            {
                return dict[Id];
            }
            else
            {
                var cs = fact();
                dict.Add(Id, cs);
                return cs;
            }
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
        
    }

   

}
