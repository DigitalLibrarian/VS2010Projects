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


        public Body ProduceBody(IBodyPhenotype genome)
        {
            if (!genome.BodyPartPhenos.Any())
            {
                return null;
            }
            var body = new Body();
            var partsToMake = genome.BodyPartPhenos.ToList();


            var gennedParts = genome.BodyPartPhenos.Select(partGenome => 
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
                    var anchorPart = Fuzzy.CircleIndex(body.Parts, partGenome.AnchorPart.InstanceId);
                    if (!ConnectPartFromAnchor(body, anchorPart, partGenome, part, autoTryOthers: false))
                    {
                        // don't care under GA, means a loss  of the benfit of the limb
                    }

                }

                /*
                // parts are placed, and anchor sockets have been taken
                foreach (var socketGenome in partGenome.SocketGenomes)
                {
                    var forBodyId = socketGenome.ForeignSocket.BodyPart.InstanceId;
                    var forBody = Fuzzy.CircleIndex(fittedParts, forBodyId);

                    var fsock = Fuzzy.CircleIndex(forBody.Sockets, socketGenome.ForeignSocket.InstanceId);
                    var lsock = Fuzzy.CircleIndex(part.Sockets, socketGenome.InstanceId);

                    if (!body.WillFit(lsock, fsock))
                    {
                        throw new Exception();
                    }
                    lsock.ConnectSocket(fsock);


                }
                 * */
               // body.Parts.AddRange(fittedParts);
                first = false;


            }

            /*
            for (int i = 0; i < genome.BodyPartGenomes.Count; i++)
            {
                var part = body.Parts[i];
                var partGenome = genome.BodyPartGenomes[i];
                foreach (var oG in partGenome.OrganGenomes)
                {
                    var organ = ProduceOrgan(oG, part);
                    part.Organs.Add(organ);
                }

            }
             * */

            // parts are placed and connected with sockets
            // now we need  to connect all the neural organs



            body.NervousSystem = new NervousSystem(body);
            
            return body;
        }

        private bool ConnectPartFromAnchor(Body body, BodyPart anchorPart, IBodyPartPhenotype partGenome, BodyPart part, bool autoTryOthers=true)
        {
            var placement = partGenome.PlacementPartSocket;
            var socket = Fuzzy.CircleIndex(anchorPart.Sockets, placement.InstanceId);
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

        

        public BodyPart ProduceBodyPart(IBodyPartPhenotype genome)
        {
            var part = NewPartFromIndex(genome.BodyPartGeometryIndex);
            part.Color = genome.Color;
            /*
            foreach (var oG in genome.OrganGenomes)
            {
                var organ = ProduceOrgan(oG, part);
                part.Organs.Add(organ);
            }


            */

            var sockets = part.Sockets;


            return part;
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
        /*
        public Organ ProduceOrgan(IOrganGenome genome, BodyPart part)
        {
            Organ organ = null;
            if (genome is NeuralOrganGenome)
            {
                var noG = genome as NeuralOrganGenome;
                organ = ProduceNeuralOrgan(noG, part);
            }
            return organ;
        }


        public NeuralNetwork ProduceNeuralNetwork(INeuralNetworkGenome genome)
        {
            var net = new NeuralNetwork(genome.NumInputs, genome.NumHidden, genome.NumOutputs);

            net.SetWeights(genome.Weights);

            return net;
        }

        public NeuralOrgan ProduceNeuralOrgan(NeuralOrganGenome genome, BodyPart part)
        {
            var net = ProduceNeuralNetwork(genome.NeuralNetworkGenome);
            var no = new NeuralOrgan(part, net);

            var isG = genome.InputGenome;
            var osG = genome.OutputGenome;

            var usedSockets = part.Sockets.Where(x => !x.HasAvailable).ToList();

            List<ChanneledSignal> set = new List<ChanneledSignal>();

            for (int i = 0; i < usedSockets.Count(); i++)
            {
                set.Add(usedSockets[i].Part.ChanneledSignal);
            }
             
            var cs = CircleIndex(set, isG.ChanneledSignalGenome.InstanceId);
            no.InputReader = cs.RegisterOutputChannel(net.NumInputs);

            cs = CircleIndex(set, osG.ChanneledSignalGenome.InstanceId);
            no.OutputWriter = cs.RegisterInputChannel(net.NumOutputs);


            return no;
        }

        
        public ChanneledSignal ProduceChanneledSignal(IChanneledSignalGenome genome, Dictionary<int, ChanneledSignal> dict)
        {
            return GetInstance(genome.InstanceId, dict, () => new ChanneledSignal(new List<double>()));
        }
        */
    }

   

}
