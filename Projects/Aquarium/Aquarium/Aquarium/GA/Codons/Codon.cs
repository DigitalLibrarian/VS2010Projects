using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.GA.Codons
{
    public abstract class Codon<TCGene>
    {
        public abstract int FrameSize { get; }
        public abstract bool Recognize(List<TCGene> genes);
        public abstract List<TCGene> Example(); 
    }

    public abstract class BodyCodon : Codon<int>  
    {
        int Band { get; set; }
        int _frameSize;
        public override int FrameSize { get { return _frameSize; } }
        public int Range { get; set; }

        public BodyCodon(int band, int range = 100, int frameSize = 3)
        {
            Band = band;
            _frameSize = frameSize;
            Range = range;
        }

        public override bool Recognize(List<int> genes)
        {
            if (genes.Count() != FrameSize) return false;

            for (int i = 0; i < FrameSize; i++)
            {
                var diff = genes[i] - Band;
                if (diff > 0 && diff > Range) return false;
                if (diff < 0 && diff < Range) return false;
            }
            return true;
        }

        public override List<int> Example()
        {
            var l = new List<int>();
            for (int i = 0; i < FrameSize; i++)
            {
                l.Add(Band);
            }
            return l;
        }

    }
}
