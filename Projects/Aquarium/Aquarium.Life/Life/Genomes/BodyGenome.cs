using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.Life.Genomes
{
    public class BodyGenome
    {
        public List<int> Data { get; private set; }
        public BodyGenome(IEnumerable<int> data) : this(data.ToList()) { }
        public BodyGenome(List<int> data)
        {
            Data = data;
        }

        public int Size { get { return Data.Count(); } }
    }
}
