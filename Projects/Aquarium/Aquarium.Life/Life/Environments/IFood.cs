using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Aquarium.Life.Environments
{
    public interface IFood
    {
        Vector3 Position { get; }

        float ConsumableEnergy { get; }

        /// <summary>
        /// The food gets bitten, triggering whatever state change the implementation needs.
        /// </summary>
        /// <param name="biteSize">Max amount of energy that is being requested</param>
        /// <returns>Amount of energy released from the bite.</returns>
        float BeConsumed(float biteSize);
    }
}
