using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Bodies;
using Aquarium.GA.Signals;

namespace Aquarium.GA.Organs
{
    public class AbilityOrgan : IOOrgan
    {
        public override OrganType OrganType
        {
            get { return OrganType.Ability; }
        }
        public OrganAbility Ability { get; private set; }

        public override bool HasAbility { get { return Ability != null; } }

        public AbilityOrgan(BodyPart bodyPart, OrganAbility ability) : base(bodyPart) { Ability = ability; }

        public override void ReceiveSignal(NervousSystem nervousSystem, Signal signal)
        {
            if (HasAbility)
            {
                OutputSignal = Ability.Fire(nervousSystem, this, signal);
            }

            base.ReceiveSignal(nervousSystem, signal);
        }




        public override int NumInputs
        {
            get { return Ability.NumInputs; }
        }

        public override int NumOutputs
        {
            get { return Ability.NumOutputs; }
        }
    }
}
