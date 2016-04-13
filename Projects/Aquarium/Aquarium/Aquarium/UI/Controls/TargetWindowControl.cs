using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls.Desktop;
using Nuclex.UserInterface.Controls;
using Aquarium.Ui.Targets;
using Microsoft.Xna.Framework;
using Aquarium.Targeting;
using Nuclex.UserInterface;

namespace Aquarium.UI.Controls
{
    public partial class TargetWindowControl : WindowControl
    {
        public TargetWindowControl(int x, int y)
        {
            StartX = x;
            StartY = y;
            this.Title = string.Format("Target Window");
            InitializeComponent();
        }

        public void SetNewTarget(ITarget target)
        {
            Target = target;
            UpdateTarget();
        }

        public void UpdateTarget()
        {
            if (Target != null)
            {
                this.Title = string.Format("Target: {0} ", Target.GetType().ToString());
                TargetLabel.Text = Target.Label;
            }
        }
    }

    partial class TargetWindowControl
    {
        public ITarget Target { get; set; }
        public bool HasTarget { get { return Target != null; } }
        public event EventHandler OnCloseButtonPress { add { CloseWindowButtonControl.Pressed += value; } remove { CloseWindowButtonControl.Pressed -= value; } }

        int StartX { get; set; }
        int StartY { get; set; }
        int Width { get; set; }
        int Height { get; set; }

        void InitializeComponent()
        {
            foreach (var c in CreateUi())
            {
                this.Children.Add(c);
            }
        }

        IEnumerable<Control> CreateUi()
        {
            Width = 600;
            Height = 200;

            this.Bounds = new UniRectangle (
                StartX, StartY, Width, Height
            );

            var pad = 10;
            TargetLabel = new LabelControl()
            {
                Bounds = new UniRectangle(pad, 30, Width-(pad), Height-(pad)),
            };
            CloseWindowButtonControl = new CloseWindowButtonControl(this);
            return new List<Control>{
                TargetLabel,
                CloseWindowButtonControl,
            };
        }

        private LabelControl TargetLabel { get; set; }

        private CloseWindowButtonControl CloseWindowButtonControl { get; set; }

    }
}
