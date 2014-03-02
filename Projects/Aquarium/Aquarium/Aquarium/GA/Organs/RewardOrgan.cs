using Aquarium.GA.Signals;
using Aquarium.GA.Bodies;
using System.Collections.Generic;
using Forever.Neural;
namespace Aquarium.GA.Organs
{
    public class RewardOrgan : IOOrgan
    {
        public IRewardable Rewardable { get; private set; }
        public List<IMoment> RecordedMoments { get; private set;}
        int MaxMoments = 2;

        public RewardOrgan(BodyPart part, IRewardable organ) : base(part) 
        {
            RecordedMoments = new List<IMoment>();
            Rewardable = organ;
        }


        public void Reward()
        {
            if (RecordedMoments.Count == 0) return;

            foreach (var moment in RecordedMoments)
            {
                var inputs = moment.Input.Value;
                var outputs = moment.Output.Value;

                Rewardable.Network.Train(inputs, outputs, learnRate: 0.0001, momentum: 0.001);
            }
        }

        public void Remember(IMoment moment)
        {
            if (RecordedMoments.Count > MaxMoments)
            {
                RecordedMoments.RemoveAt(0);
            }

            RecordedMoments.Add(moment);
        }

        public override OrganType OrganType
        {
            get { return OrganType.Reward; }
        }

        int Tick = 0;
        public override void Update(NervousSystem nervousSystem)
        {
            if (Tick++ % 4 == 0)
            {
                var moment = nervousSystem.GetCurrentMoment(Rewardable);
                if (moment != null)
                {
                    Remember(moment);
                }
            }

            base.Update(nervousSystem);
        }


        public override void ReceiveSignal(NervousSystem nervousSystem, Signal signal)
        {

            var num = signal.Value[0];
            if (num > 0.5)
            {
                //conduct
                Reward();
            }

            base.ReceiveSignal(nervousSystem, signal);
        }

        public override int NumInputs
        {
            get { return 1; }
        }

        public override int NumOutputs
        {
            get { return 0; }
        }
    }

    public interface IRewardable
    {
        int NumInputs { get; }
        int NumOutputs { get; }
        
        NeuralNetwork Network { get; }
    }

    public interface IMoment
    {
        Signal Input { get; }
        Signal Output { get; }
    }
}
