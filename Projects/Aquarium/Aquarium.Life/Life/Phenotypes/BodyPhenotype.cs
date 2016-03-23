using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Aquarium.Life.Genomes;
using Aquarium.Life.Bodies;
using Microsoft.Xna.Framework;
using Aquarium.Life.Spec;

namespace Aquarium.Life.Phenotypes
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

        public BodyPhenotype(OrganismSpec spec)
        {
            BodyPartPhenos = spec.BodyParts.Index.Collection.Select(x => new BodyPartPhenotype(x)).Cast<IBodyPartPhenotype>().ToList();
            OrganPhenos = spec.Organs.Index.Collection.Select(x => new OrganPhenotype(x)).Cast<IOrganPhenotype>().ToList();
            NeuralNetworkPhenos = spec.NeuralNetworks.Index.Collection.Select(x => new NeuralNetworkPhenotype(x)).Cast<INeuralNetworkPhenotype>().ToList();
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
