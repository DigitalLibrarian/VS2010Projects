using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.GA.Genomes
{
    public abstract class Genome
    {
        List<double> Chromosomes { get; set; }

        public Genome()
        {
            Chromosomes = new List<double>();
        }

        public Genome(List<double> chromosomes)
        {
            Chromosomes = chromosomes;
        }


    }

    public abstract class OrganGenome : Genome
    {
        int NumInputs { get; set; }
        int NumOutputs { get; set; }

        int abilityID { get; set; }

    }

    class NeuralProcessorOrganGenome : OrganGenome
    {
        int NumHidden { get; set; }
    }

    class RewardOrganGenome : OrganGenome
    {

    }

    public class BodyGenome : Genome
    {
        int NumBodyParts { get; set; }



        /*
         * 
         * 
 - body parts
 -- initial condition random number seed
 -- self value
 -- relative position, orientation 
 -- color/texture data  (identifiable by some sense organs)
 -- procedural mesh info (probably select from some algorithms we can collision detect)
 -- organ genome
*/
    }
}