using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Aquarium.GA.Phenotypes
{
    public interface IBodyPhenotype
    {
        List<IBodyPartPhenotype> BodyPartPhenos { get; set; }
    }

    public interface IBodyPartPhenotype
    {
        int BodyPartGeometryIndex { get; set; }
        Color Color { get; set; }

        IInstancePointer AnchorPart { get; set; }
        IInstancePointer PlacementPartSocket { get; set; }

        List<IOrganPhenotype> OrganGenomes { get; set; }
        List<IBodyPartSocketPhenotype> SocketGenomes { get; set; }
        IChanneledSignalPhenotype ChanneledSignalGenome { get; set; }

        Vector3 Scale { get; set; }

    }

    public interface IChanneledSignalPhenotype : IInstancePointer { }

    public interface IBodyPartSocketPhenotype : IInstancePointer
    {
        IForeignBodyPartSocketPhenotype ForeignSocket { get; set; }
    }
    public interface IForeignBodyPartSocketPhenotype : IInstancePointer
    {
        IInstancePointer BodyPart { get; set; }
    }

    /// <summary>
    /// This is a  "safe" index into some list during fabrication time.
    /// 
    /// It's safe in that you can count on it to return something.
    /// </summary>
    public interface IInstancePointer
    {
        int InstanceId { get; set; }
    }


    public interface IOrganPhenotype
    {
        IInstancePointer BodyPointer { get; set; }
        IOrganAbilityPhenotype OrganAbilityGenome { get;  }
    }

    public interface IOrganAbilityPhenotype
    {

    }

    public interface INeuralNetworkPhenotype
    {
        int NumHidden { get; set; }

        int NumInputs { get; set; }

        int NumOutputs { get; set; }

        double[] Weights { get; set; }
    }

}
