using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.GA.Organs.OrganAbilities
{
    public abstract class SimpleFireAbility : OrganAbility
    {

        public override int NumInputs
        {
            get { return 1; }
        }

        public override int NumOutputs
        {
            get { return 1; }
        }


        public SimpleFireAbility(int param0) : base(param0) { }
    }
}
