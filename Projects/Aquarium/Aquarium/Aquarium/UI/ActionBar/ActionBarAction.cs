using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.UI
{
    public interface IActionBarAction
    {
        void OnFire();
        string Label { get; }
    }

    public class ActionBarAction : IActionBarAction
    {
        Action Action { get; set; }
        public string Label { get; set; }

        public ActionBarAction(Action action) : this("", action)
        {
        }
        public ActionBarAction(string label, Action action)
        {
            Label = label;
            Action = action;
        }
        public void OnFire()
        {
            Action();
        }


    }
}
