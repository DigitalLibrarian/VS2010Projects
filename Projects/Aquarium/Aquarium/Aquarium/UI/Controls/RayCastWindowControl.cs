using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls.Desktop;
using Nuclex.UserInterface.Controls;

namespace Aquarium.UI.Controls
{
    public partial class RayCastWindowControl : WindowControl
    {
        int StartX { get; set; }
        int StartY { get; set; }

        public RayCastWindowControl(int x, int y)
        {
            StartX = x;
            StartY = y;

            InitializeComponent();
        }
    }

    partial class RayCastWindowControl
    {
        void InitializeComponent()
        {
            int windowTitleBar = 25;

            int pad = 10;
            int buttonHeight = 25;
            int buttonWidth = 140;
            ClearButtonControl = new ButtonControl()
            {
                Bounds = new Nuclex.UserInterface.UniRectangle(pad, windowTitleBar + pad, buttonWidth, buttonHeight),
                Text = "Clear"
            };

            NextLeafButtonControl = new ButtonControl()
            {
                Bounds = new Nuclex.UserInterface.UniRectangle(pad, windowTitleBar + pad + buttonHeight + pad, buttonWidth, buttonHeight),
                Text = "Next Leaf"
            };

            int width = pad + buttonWidth + pad;
            int numButtons = 2;
            int totalButtonPadding = (numButtons-1) * pad;
            int height = windowTitleBar + pad + (2 * buttonHeight) + totalButtonPadding + pad;
            this.Bounds = new Nuclex.UserInterface.UniRectangle(StartX, StartY, width, height);
            this.Title = "Ray Casting";

            foreach (var control in new Control[]{
                ClearButtonControl,
                NextLeafButtonControl
            })
            {
                this.Children.Add(control);
            }

        }

        private ButtonControl ClearButtonControl { get; set; }
        public event EventHandler OnClear { add { ClearButtonControl.Pressed += value; } remove { ClearButtonControl.Pressed -= value; } }
        private ButtonControl NextLeafButtonControl { get; set; }
        public event EventHandler OnNextLeaf { add { NextLeafButtonControl.Pressed += value; } remove { NextLeafButtonControl.Pressed -= value; } }
    }
}
