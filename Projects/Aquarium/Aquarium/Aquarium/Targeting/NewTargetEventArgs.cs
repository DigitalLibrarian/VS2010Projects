using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.Targeting
{
    public class NewTargetEventArgs : EventArgs
    {
        public ITarget Target { get; set; }
    }
}
