using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Aquarium.GA.Genomes;
using Aquarium.GA.Bodies;
using Microsoft.Xna.Framework;
using Aquarium.GA.Headers;

namespace Aquarium.GA.Phenotypes
{
    public class BodyPheno
    {
       
    }

    public abstract class ComponentPheno
    {
       
    }

    public class ChanneledSignalPhenotype : ComponentPheno, IChanneledSignalPhenotype
    {
        public int InstanceId { get; set; }
    }




    public class NeuralInputSocketPhenotype : ComponentPheno
    {
        public int Channel { get; set; }
        public IChanneledSignalPhenotype ChanneledSignalGenome { get; set; }

    }

    public class NeuralOutputSocketPhenotype : ComponentPheno
    {
        public int Channel { get; set; }
        public IChanneledSignalPhenotype ChanneledSignalGenome { get; set; }
    }
    


    public class BodyPhenotype : BodyPheno, IBodyPhenotype
    {
        public List<IBodyPartPhenotype> BodyPartPhenos { get; set; }
        public List<IOrganPhenotype> OrganPhenos { get; set; }
        public List<INeuralNetworkPhenotype> NeuralNetworkPhenos { get; set; }
        public BodyPhenotype()
        {
            BodyPartPhenos = new List<IBodyPartPhenotype>();
            OrganPhenos = new List<IOrganPhenotype>();
            NeuralNetworkPhenos = new List<INeuralNetworkPhenotype>();
        }

        public int NumBodyParts
        {
            get { return BodyPartPhenos.Count(); }
        }


    }

 
    public class InstancePointer : IInstancePointer
    {

        public int InstanceId { get; set; }

        public InstancePointer(int id)
        {
            InstanceId = id;
        }
    }

    public class BodyPartPointerPhenotype : ComponentPheno, IInstancePointer
    {
        public int InstanceId { get; set; }
    }




}
