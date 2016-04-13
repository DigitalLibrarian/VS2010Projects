using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls.Desktop;
using Nuclex.UserInterface.Controls;

namespace Aquarium.UI.Controls
{
    public partial class GarbageCollectionWindowControl : WindowControl
    {
        int StartX { get; set; }
        int StartY { get; set; }
        int Width { get; set; }
        int Height { get; set; }

        public GarbageCollectionWindowControl(int x, int y)
        {
            StartX = x;
            StartY = y;
            InitializeComponent();
        }
    }

    partial class GarbageCollectionWindowControl
    {
        void InitializeComponent()
        {
            this.Title = "Garbage Collection";
            
            var pad = 5;
            
            int infoLabelWidth = 300;
            int infoLabelHeight =  (25 * 6);
            InfoLabelControl = new LabelControl()
            {
                Text = BuildInfoLabelText(),
                //Bounds = new Nuclex.UserInterface.UniRectangle(pad, 20 + (infoLabelHeight / 2), infoLabelWidth, 40 + infoLabelHeight)
                Bounds = new Nuclex.UserInterface.UniRectangle(0, 40, infoLabelWidth, infoLabelHeight)
            };

            var labelPanel = new Control()
            {
                Bounds = new Nuclex.UserInterface.UniRectangle(0, 20-(infoLabelHeight/2), infoLabelWidth, 40 + infoLabelHeight)
            };
            labelPanel.Children.Add(InfoLabelControl);

            int buttonWidth = 75;
            int buttonHeight = 25;

            RefreshButtonControl = new ButtonControl
            {
                Text = "Refresh",
                Bounds = new Nuclex.UserInterface.UniRectangle(pad + infoLabelWidth, 40 + pad, buttonWidth, buttonHeight),
            };

            RefreshButtonControl.Pressed += new EventHandler(RefreshButtonControl_Pressed);

            CollectButtonControl = new ButtonControl
            {
                Text = "Collect",
                Bounds = new Nuclex.UserInterface.UniRectangle(pad + infoLabelWidth, 40 + pad + buttonHeight + pad, buttonWidth, buttonHeight),
            };

            CollectButtonControl.Pressed += new EventHandler(CollectButton_Pressed);


            this.Width = pad + infoLabelWidth + pad + buttonWidth + pad;
            this.Height = pad + infoLabelHeight + pad;
            this.Bounds = new Nuclex.UserInterface.UniRectangle(StartX, StartY, Width, Height);

            foreach(var c in new Control[] {
                CloseWindowButtonControl = new CloseWindowButtonControl(this),
                labelPanel,
                RefreshButtonControl,
                CollectButtonControl
            }){
                this.Children.Add(c);
            }
        }

        void RefreshButtonControl_Pressed(object sender, EventArgs e)
        {
            Update();
        }

        void CollectButton_Pressed(object sender, EventArgs e)
        {
            for (int i = 0; i < GC.MaxGeneration; i++)
            {
                GC.Collect(i, GCCollectionMode.Forced);
            }
            Update();
        }

        void Update()
        {
            InfoLabelControl.Text = BuildInfoLabelText();
        }

        StringBuilder InfoTextStringBuilder = new StringBuilder();
        private string BuildInfoLabelText()
        {
            InfoTextStringBuilder.Clear();
            InfoTextStringBuilder.AppendFormat("Allocated: {0}\r\n", GC.GetTotalMemory(forceFullCollection: true));
            InfoTextStringBuilder.AppendFormat("Max Generations: {0}\r\n", GC.MaxGeneration);
            for (int i = 0; i < GC.MaxGeneration; i++)
            {
                InfoTextStringBuilder.AppendFormat(" {0} Gen Collects {1}\r\n", i, GC.CollectionCount(i));
            }
            return InfoTextStringBuilder.ToString();
        }

        private LabelControl InfoLabelControl { get; set; }
        private ButtonControl RefreshButtonControl { get; set; }
        private ButtonControl CollectButtonControl { get; set; }
        private ButtonControl CloseWindowButtonControl { get; set; }
        public event EventHandler OnCloseButtonPress { add { CloseWindowButtonControl.Pressed += value; } remove { CloseWindowButtonControl.Pressed -= value; } }
    }
}
