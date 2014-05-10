using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Aquarium.Life.Phenotypes
{
    public interface IBodyPhenotype
    {
        List<IBodyPartPhenotype> BodyPartPhenos { get; set; }
        List<IOrganPhenotype> OrganPhenos { get; set; }
        List<INeuralNetworkPhenotype> NeuralNetworkPhenos { get; set; }
    }

    public interface IBodyPartPhenotype
    {
        int BodyPartGeometryIndex { get; set; }
        Color Color { get; set; }

        IInstancePointer AnchorPart { get; set; }
        IInstancePointer PlacementPartSocket { get; set; }

        IChanneledSignalPhenotype ChanneledSignalGenome { get; set; }

        Vector3 Scale { get; set; }

    }

    public interface IChanneledSignalPhenotype : IInstancePointer { }


    /// <summary>
    /// This is a "safe" index into some list during fabrication time.
    /// 
    /// It's safe in that you can count on it to return something.
    /// </summary>
    public interface IInstancePointer
    {
        int InstanceId { get; set; }
    }


    public interface IOrganPhenotype
    {
        IInstancePointer OrganType { get; set; }
        IInstancePointer BodyPartPointer { get; set; }
        IInstancePointer InputSignal { get; set; }
        IInstancePointer OutputSignal { get; set; }
        IInstancePointer ForeignId { get; set; }

        IInstancePointer AbilityParam0 { get; set; }
        IInstancePointer AbilityParam1 { get; set; }

    }

    public interface INeuralNetworkPhenotype
    {


        int NumHidden { get; set; }

        int NumInputs { get; set; }

        int NumOutputs { get; set; }

        double[] Weights { get; set; }
    }

}
