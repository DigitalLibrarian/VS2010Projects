using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls.Desktop;
using Nuclex.UserInterface;

namespace Aquarium.UI.Controls
{
    public partial class CloseWindowButtonControl : ButtonControl
    {
        public CloseWindowButtonControl(WindowControl windowControl)
        {
            WindowControl = windowControl;
            InitializeComponent();
        }
    }

    partial class CloseWindowButtonControl
    {
        WindowControl WindowControl { get; set; }

        void InitializeComponent()
        {
            Text = "X";
            Bounds = new UniRectangle(WindowControl.Bounds.Size.X - 25, 2, 21, 21);

            Pressed += new EventHandler(CloseWindowButton_Pressed);
        }
        void CloseWindowButton_Pressed(object sender, EventArgs e)
        {
            WindowControl.Close();
        }
    }
}
