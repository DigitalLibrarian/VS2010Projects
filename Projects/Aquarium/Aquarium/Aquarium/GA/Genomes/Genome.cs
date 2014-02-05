using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;

namespace Aquarium.GA.Genomes
{
    public class Gene<T>
    {
        public int Name;
        public T Value;
    }

    public class BodyGenome : Genome<double>
    {
        public BodyGenome(List<Gene<double>> genes) : base(genes) { }
    }

    public class Genome<T>
    {
        // ((1 1) (2 1) (3 0) (4 0) (5 0) (6 1) (7 1))
        public List<Gene<T>> Genes { get; private set; }

        public int Size { get { return Genes.Count(); } }

        public Genome(List<Gene<T>> genes)
        {
            Genes = genes;
        }

        public Gene<T> ByName(int name)
        {
            // positional ordering wins
            return Genes.FirstOrDefault(g => g.Name == name);
        }


        public Gene<T> ByName(int name, GenomeTemplate<T> template)
        {
            // positional ordering wins
            var gene = Genes.FirstOrDefault(g => g.Name == name);
            if (gene == null) gene = template.ByName(name);
            return gene;
        }

        public List<T> ReadDataSequence(int index, int num, GenomeTemplate<T> template)
        {
            return CondenseSequence(index, num, template).Select(g => g.Value).ToList();
        }

        /// <summary>
        /// Removes duplicates, adds in missing info from template.  List returned is same length
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        internal List<Gene<T>> Condense(GenomeTemplate<T> template)
        {
            var result = new List<Gene<T>>();
            int visited = 0;
            int index = 0;
            while(visited < Genes.Count())
            {
                var gene = ByName(index);
                result.Add(gene);

                index++;
                visited++;
            }
            return result;
        }


        /// <summary>
        /// Removes duplicates, adds in missing info from template.  List returned is same length
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public List<Gene<T>> CondenseSequence(int index, int num, GenomeTemplate<T> template)
        {
            var result = new List<Gene<T>>();
            int i = 0;
            while (i < num)
            {
                var gene = ByName(index + i, template);
                result.Add(gene);

                i++;
            }
            return result;
        }
    }

    public abstract class GenomeTemplate<T>
    {
        public abstract Gene<T> ByName(int name);
    }

    public class ZeroIntGenomeTemplate : GenomeTemplate<int>
    {
        public override Gene<int> ByName(int name)
        {
            return new Gene<int> { Name = name, Value = 0 };
        }
    }
    public class ZeroDoubleGenomeTemplate : GenomeTemplate<double>
    {
        public override Gene<double> ByName(int name)
        {
            return new Gene<double> { Name = name, Value = 0 };
        }
    }


    public class RandomDoubleGenomeTemplate : GenomeTemplate<double>
    {
        Random R;
        public RandomDoubleGenomeTemplate(Random r)
        {
            R = r;
        }


        public override Gene<double> ByName(int name)
        {
            return new Gene<double> { Name = name, Value = R.NextDouble() };
        }
    }


    
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

            Chromosomes[index] =  value;
        }
    }

    public class ChanneledSignalGenome : ComponentPheno, IChanneledSignalGenome
    {
        public int InstanceId { get; set; }
    }

    public class OrganGenome : ComponentPheno, IOrganGenome
    {
        public IInstancePointer BodyPointer { get; set; }

        public int NumInputs { get; set; }

        public int NumOutputs { get; set; }

        public IOrganAbilityGenome OrganAbilityGenome { get; set; }
    }

    public class OrganAbilityGenome : ComponentPheno, IOrganAbilityGenome
    {
        int AbilityId  { get; set; }
    }

     public class NeuralInputSocketGenome : ComponentPheno
     {
         public int Channel { get; set; }
         public IChanneledSignalGenome ChanneledSignalGenome { get; set; }

     }

     public class NeuralOutputSocketGenome : ComponentPheno
     {
         public int Channel { get; set; }
         public IChanneledSignalGenome ChanneledSignalGenome { get; set; }
     }

    public class NeuralOrganGenome : OrganGenome
    {
        public NeuralInputSocketGenome InputGenome { get; set; }
        public NeuralOutputSocketGenome OutputGenome { get; set; }

        public NeuralNetworkGenome NeuralNetworkGenome { get; set; }

    }

    public class NeuralNetworkGenome : ComponentPheno, INeuralNetworkGenome
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
        public List<IOrganGenome> OrganGenomes { get; set; }
        public List<IBodyPartSocketGenome> SocketGenomes { get; set; }

        public IChanneledSignalGenome ChanneledSignalGenome { get; set; }

        public IInstancePointer AnchorPart { get; set; }
        public IInstancePointer PlacementPartSocket { get; set; }

        public BodyPartPhenotype()
        {
            SocketGenomes = new List<IBodyPartSocketGenome>();
            OrganGenomes = new List<IOrganGenome>();
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

    public class BodyPartPointerGenome : ComponentPheno, IInstancePointer
    {
        public int InstanceId { get; set; }
    }

    public class BodyPartSocketPointerGenome : ComponentPheno, IBodyPartSocketGenome
    {
        public int InstanceId { get; set; }


        public IForeignBodyPartSocketGenome ForeignSocket
        {
            get;
            set;
        }


    }
}