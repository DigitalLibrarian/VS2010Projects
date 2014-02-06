using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.GA.Genomes
{
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
}
