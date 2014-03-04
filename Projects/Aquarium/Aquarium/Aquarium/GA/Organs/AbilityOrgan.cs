using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Bodies;
using Aquarium.GA.Signals;

using Forever.Physics;
using Aquarium.GA.Organs.OrganAbilities;

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

        MutableForceGenerator _mutaFG;
        public override IForceGenerator ForceGenerator
        {
            get
            {
                return _mutaFG;
            }
        }

        public AbilityOrgan(BodyPart bodyPart, OrganAbility ability) : base(bodyPart) 
        { 
            Ability = ability;

            _mutaFG = new MutableForceGenerator();
        }

        public override void ReceiveSignal(NervousSystem nervousSystem, Signal signal)
        {
            if (HasAbility)
            {
                OutputSignal = Ability.Fire(nervousSystem, this, signal, _mutaFG);
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
