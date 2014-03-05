using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.UI
{
    public enum SlotState
    {
        /// <summary>
        /// This slot isn't activated or cooling down.  It's ready to be become activated.
        /// </summary>
        Inactive,
        /// <summary>
        /// This slot is in the state of being fired.
        /// </summary>
        Fired,
        /// <summary>
        /// This slot is on cool down.
        /// </summary>
        CoolDown,
    }  
}
