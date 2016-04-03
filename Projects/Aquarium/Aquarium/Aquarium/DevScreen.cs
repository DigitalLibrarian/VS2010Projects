using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.Screens;

namespace Aquarium
{
    public class DevScreen : GameScreen
    {
        protected Random Random { get; private set; }
        public DevScreen(Random random)
        {
            Random = random;
        }

        public DevScreen()
        {
            Random = new Random();
        }

    }
}
