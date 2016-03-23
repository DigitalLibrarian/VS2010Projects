using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.Life.Spec
{
    public class OrganSpec
    {
        public int OrganType { get; set; }
        public int BodyPartId { get; set; }
        public int InputSignal { get; set; }
        public int OutputSignal { get; set; }
        public int AbilityId { get; set; }
        public int AbilityParam0 { get; set; }
        public int AbilityParam1 { get; set; }

        public static OrganSpec FromGenome(IEnumerator<int> g)
        {
            return new OrganSpec
            {
                OrganType = Fuzzy.PositiveInteger(g.Next()),
                BodyPartId = Fuzzy.PositiveInteger(g.Next()),
                InputSignal = Fuzzy.PositiveInteger(g.Next()),
                OutputSignal = Fuzzy.PositiveInteger(g.Next()),
                AbilityId = Fuzzy.PositiveInteger(g.Next()),
                AbilityParam0 = g.Next(),
                AbilityParam1 = g.Next()
            };
        }
    }
}
