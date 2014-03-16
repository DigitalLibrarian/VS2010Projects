﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Bodies;
using Forever.Neural;
using Aquarium.GA.Organs;
using Aquarium.GA.Signals;
using Microsoft.Xna.Framework;
using Aquarium.GA.Phenotypes;
using Aquarium.GA.Organs.OrganAbilities;

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
                 var part = NewPartFromIndex(partGenome.BodyPartGeometryIndex);
                 part.UCTransform = Matrix.CreateScale(partGenome.Scale) * part.UCTransform;
                part.Color = partGenome.Color;

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

                    var organAbility = GetOrganAbility(rawAbilityId, abilityParam0);
                    var organ = new AbilityOrgan(part, organAbility);

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


        public OrganAbility GetOrganAbility(int rawAbilityId, int abilityParam0)
        {
            var list = new List<Func<OrganAbility>>
            {
                () => new ThrusterAbility(abilityParam0),
                () => new SpinnerAbility(abilityParam0),
                () => new FoodBitterAbility(abilityParam0),
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
