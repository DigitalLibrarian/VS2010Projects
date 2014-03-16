using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.Life.Codons
{
    

    public class BodyPartStartCodon : BodyCodon
    {
        public BodyPartStartCodon() : this(-2000, 100) { }
        public BodyPartStartCodon(int band, int range) : base(band, range) { }

    }

    public class BodyPartEndCodon : BodyCodon
    {   
        public BodyPartEndCodon() : this(-3000, 100) { }
        public BodyPartEndCodon(int band, int range) : base(band, range) { }
    }

    public class BodyEndCodon : BodyCodon
    {
        public BodyEndCodon() : this(-1000, 100) { }
        public BodyEndCodon(int band, int range) : base(band, range) { }
    }


    public class OrganStartCodon : BodyCodon
    {
        public OrganStartCodon() : this(-4200, 100) { }
        public OrganStartCodon(int band, int range) : base(band, range) { }
    }


    public class OrganEndCodon : BodyCodon
    {
        public OrganEndCodon() : this(-4400, 100) { }
        public OrganEndCodon(int band, int range) : base(band, range) { }
    }


    public class NeuralNetworkStartCodon : BodyCodon
    {
        public NeuralNetworkStartCodon() : this(-8200, 100) { }
        public NeuralNetworkStartCodon(int band, int range) : base(band, range) { }
    }


    public class NeuralNetworkEndCodon : BodyCodon
    {
        public NeuralNetworkEndCodon() : this(-8400, 100) { }
        public NeuralNetworkEndCodon(int band, int range) : base(band, range) { }
    }

}
