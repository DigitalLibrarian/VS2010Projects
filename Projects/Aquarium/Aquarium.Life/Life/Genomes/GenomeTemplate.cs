using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.Life.Genomes
{
    /*
     * Cool idea 
     *  A codon that can be used to refer to a previous ancestor's genome to be used
     *  as a template for the current one.
     *  
     * We'd have to keep a list of genomic templates.  Best to keep track of how many 
     * ancestral template codons are alive in the gene pool so that we can let them go 
     * from memory when they aren't used any more.
     * 
     * 
     * One way to trigger the creation of such a codon could be by measuring genetic variance
     * along lineage lines.  If the organim's genome doesn't really change that much over 
     * very very long periods of time (statistically determined), then it can become an ancestor
     * genome template, and it's offspring will be implanted with an ancestral template codon that
     * wont change the genome's read at all for the current generation.
     * 
     */


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



    public class RandomIntGenomeTemplate : GenomeTemplate<int>
    {
        Random R;
        public RandomIntGenomeTemplate(Random r)
        {
            R = r;
        }


        public override Gene<int> ByName(int name)
        {
            return new Gene<int> { Name = name, Value = R.Next() };
        }
    }
}
