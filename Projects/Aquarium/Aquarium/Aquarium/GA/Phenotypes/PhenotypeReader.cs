using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Bodies;
using Forever.Neural;
using Aquarium.GA.Organs;
using Aquarium.GA.Signals;
using Microsoft.Xna.Framework;
using Aquarium.GA.Phenotypes;

namespace Aquarium.GA.Phenotypes
{
       
    public class PhenotypeReader
    {
        List<Func<BodyPart>> PartIndex { get; set; }
        public PhenotypeReader()
        {
            PartIndex = BodyGenerator.ProduceLibraryOfParts();
        }


        public Body ProduceBody(IBodyPhenotype bodyPheno)
        {
            if (!bodyPheno.BodyPartPhenos.Any())
            {
                return null;
            }
            var body = new Body();
            var partsToMake = bodyPheno.BodyPartPhenos.ToList();

            var gennedParts = bodyPheno.BodyPartPhenos.Select(partGenome => 
            {
                 var part = NewPartFromIndex(partGenome.BodyPartGeometryIndex);
                 part.UCTransform = Matrix.CreateScale(partGenome.Scale) * part.UCTransform;
                part.Color = partGenome.Color;

                part.ChanneledSignal = new ChanneledSignal(new List<double>());
                return part;
            }).ToList();

            bool first = true;
            
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
            var organFab = new OrganFab();

            var goodOrganPhenos = new List<IOrganPhenotype>();

            foreach (var organPheno in bodyPheno.OrganPhenos)
            {
                var partId = organPheno.BodyPartPointer.InstanceId;
                var part = Fuzzy.ScaledCircleIndex(body.Parts, partId);
                goodOrganPhenos.Add(organPheno);
            }

            var organNNPhenos = new Dictionary<IOrganPhenotype, INeuralNetworkPhenotype>();

            foreach (var neuPheno in bodyPheno.NeuralNetworkPhenos)
            {
                var partId = neuPheno.BodyPartPointer.InstanceId;
                var part = Fuzzy.ScaledCircleIndex(body.Parts, partId);
                var organId = neuPheno.OrganPointer.InstanceId;
                if (goodOrganPhenos.Any())
                {
                    var organPheno = Fuzzy.ScaledCircleIndex(goodOrganPhenos, organId);
                    if (!organNNPhenos.ContainsKey(organPheno))
                    {
                        organNNPhenos.Add(organPheno, neuPheno);
                        goodOrganPhenos.Remove(organPheno);
                    }
                }
            }

            var neurals = new Dictionary<IOrganPhenotype, Organ>();

            //now we can add the neural organs
            foreach (var noPheno in organNNPhenos.Keys)
            {
                
                var partId = noPheno.BodyPartPointer.InstanceId;
                var part = Fuzzy.ScaledCircleIndex(body.Parts, partId);
                var nnPheno = organNNPhenos[noPheno];

                var network = new NeuralNetwork(nnPheno.NumInputs, nnPheno.NumHidden, nnPheno.NumOutputs);
                network.SetWeights(nnPheno.Weights);

                var organ = new NeuralOrgan(part, network);
                part.AddOrgan(organ); 

                neurals.Add(noPheno, organ);

            }

            foreach (var noPheno in neurals.Keys)
            {
                var inputSignalId = noPheno.InputSignal.InstanceId;
                var outputSignalId = noPheno.OutputSignal.InstanceId;

                var organ = neurals[noPheno] as IOOrgan;
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


            body.NervousSystem = new NervousSystem(body);
            
            return body;
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
                if (BodyGenerator.MoveToFitLocalSocket(body, socket, part))
                {
                    return BodyGenerator.AutoConnectPartSockets(body, part);
                }
            }
            return false;
        }

        private BodyPart NewPartFromIndex(int index)
        {
            var max = PartIndex.Count();

            return PartIndex[index % max]();
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
 
    }

   

}
