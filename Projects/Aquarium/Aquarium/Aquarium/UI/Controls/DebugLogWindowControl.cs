using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls.Desktop;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface;

namespace Aquarium.UI.Controls
{
    public partial class DebugLogWindowControl : WindowControl
    {
        public DebugLogWindowControl(int startX, int startY)
        {
            StartX = startX;
            StartY = startY;
            InitializeComponent();
        }

        public DebugLogWindowControl()
        {
            InitializeComponent();
        }
    }

    partial class DebugLogWindowControl
    {
        public float StartX { get; set; }
        public float StartY { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        private void InitializeComponent()
        {
            this.Title = "Debug Log";
            
            foreach (var control in CreateUi())
            {
                this.Children.Add(control);
            }
        }

        IEnumerable<Control> CreateUi()
        {
            this.Width = 1600;
            this.Height = 200;
            this.Bounds = new UniRectangle(StartX, StartY, Width, Height);

            var closeWindowButton = new CloseWindowButtonControl(this);

            return new List<Control>
                {
                    closeWindowButton
                };
        }
    }
}
