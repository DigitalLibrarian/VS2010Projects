using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Aquarium.GA.Genomes;
using Aquarium.GA.Bodies;
using Microsoft.Xna.Framework;

namespace Aquarium.GA.Phenotypes
{
    public class BodyPheno
    {

        protected List<double> Chromosomes { get; set; }

        internal BodyPheno()
        {
            Chromosomes = new List<double>();
        }

        public BodyPheno(List<double> chromosomes)
        {
            Chromosomes = chromosomes;
        }
    }

    public abstract class ComponentPheno : BodyPheno
    {

        int StartIndex { get; set; }
        int ComponentLength { get; set; }

        protected double At(int relIndex)
        {
            var index = StartIndex + relIndex;

            return Chromosomes[index];
        }

        protected void At(int relIndex, double value)
        {
            var index = StartIndex + relIndex;

            Chromosomes[index] = value;
        }
    }

    public class ChanneledSignalPhenotype : ComponentPheno, IChanneledSignalPhenotype
    {
        public int InstanceId { get; set; }
    }

    public class OrganPhenotype : ComponentPheno, IOrganPhenotype
    {
        public IInstancePointer BodyPointer { get; set; }

        public int NumInputs { get; set; }

        public int NumOutputs { get; set; }

        public IOrganAbilityPhenotype OrganAbilityGenome { get; set; }
    }

    public class OrganAbilityPhenotype : ComponentPheno, IOrganAbilityPhenotype
    {
        int AbilityId { get; set; }
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

    public class NeuralOrganGenome : OrganPhenotype
    {
        public NeuralInputSocketPhenotype InputGenome { get; set; }
        public NeuralOutputSocketPhenotype OutputGenome { get; set; }

        public NeuralNetworkPhenotype NeuralNetworkGenome { get; set; }

    }

    public class NeuralNetworkPhenotype : ComponentPheno, INeuralNetworkPhenotype
    {

        public int NumHidden { get; set; }

        public int NumInputs { get; set; }

        public int NumOutputs { get; set; }

        public double[] Weights { get; set; }

    }


    public class BodyPhenotype : BodyPheno, IBodyPhenotype
    {
        public List<IBodyPartPhenotype> BodyPartPhenos { get; set; }

        public BodyPhenotype()
        {
            BodyPartPhenos = new List<IBodyPartPhenotype>();
        }

        public int NumBodyParts
        {
            get { return BodyPartPhenos.Count(); }
        }


    }

    public class BodyPartPhenotype : ComponentPheno, IBodyPartPhenotype
    {
        public int BodyPartGeometryIndex { get; set; }
        public Color Color { get; set; }
        public List<IOrganPhenotype> OrganGenomes { get; set; }
        public List<IBodyPartSocketPhenotype> SocketGenomes { get; set; }

        public IChanneledSignalPhenotype ChanneledSignalGenome { get; set; }

        public IInstancePointer AnchorPart { get; set; }
        public IInstancePointer PlacementPartSocket { get; set; }

        public BodyPartPhenotype()
        {
            SocketGenomes = new List<IBodyPartSocketPhenotype>();
            OrganGenomes = new List<IOrganPhenotype>();
            Scale = new Vector3(1f, 1f, 1f);
        }




        public Vector3 Scale
        {
            get;
            set;
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

    public class BodyPartSocketPointerPhenotype : ComponentPheno, IBodyPartSocketPhenotype
    {
        public int InstanceId { get; set; }


        public IForeignBodyPartSocketPhenotype ForeignSocket
        {
            get;
            set;
        }


    }



}
