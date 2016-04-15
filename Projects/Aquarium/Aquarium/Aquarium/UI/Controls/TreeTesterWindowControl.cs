using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls.Desktop;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Forever.Voxel.SVO;
using Microsoft.Xna.Framework;

using Forever.Extensions;
using Microsoft.Xna.Framework.Input;

namespace Aquarium.UI.Controls
{
    public partial class TreeTesterWindowControl<T> : WindowControl
    {
        int StartX { get; set; }
        int StartY { get; set; }
        int TitleBar = 20;
        int Pad = 15;

        int ButtonHeight = 25;
        int ButtonWidth = 30;
        public TreeTesterWindowControl(int x, int y)
        {
            StartX = x;
            StartY = y;

            InitializeComponent();
        }
    }

    partial class TreeTesterWindowControl<T>
    {
        void InitializeComponent()
        {
            this.Title = "Tree Tester";

            CreateAxisAlignedWalkerPanel(0, TitleBar);
            CreateNodeInfoPanel(0, TitleBar + AxisAlignedPanelControl.Bounds.Size.Y);
            CreateTreeWalkerPanel(0, TitleBar + AxisAlignedPanelControl.Bounds.Size.Y + NodeInfoPanel.Bounds.Size.Y);

            var width = TreeWalkerPanel.Bounds.Size.X;
            var height = TitleBar + AxisAlignedPanelControl.Bounds.Size.Y + NodeInfoPanel.Bounds.Size.Y + TreeWalkerPanel.Bounds.Size.Y;
            
            this.Bounds = new Nuclex.UserInterface.UniRectangle(StartX, StartY, width, height);

            foreach (var c in new Control[]{
                AxisAlignedPanelControl,
                NodeInfoPanel,
                TreeWalkerPanel,
            })
            {
                this.Children.Add(c);
            }
        }

        #region Control Tools
        UniRectangle ButtonBoundsGrid(UniScalar startX, UniScalar startY, UniScalar x, UniScalar y)
        {
            return new UniRectangle(startX + x * ButtonWidth + (Pad * x - 1), startY + y * ButtonHeight + (Pad * y - 1), ButtonWidth, ButtonHeight);
        }
        #endregion

        #region Axis Aligned Walker

        private Control AxisAlignedPanelControl { get; set; }
        private ButtonControl UpButtonControl { get; set; }
        private ButtonControl DownButtonControl { get; set; }
        private ButtonControl LeftButtonControl { get; set; }
        private ButtonControl RightButtonControl { get; set; }
        private ButtonControl ForwardButtonControl { get; set; }
        private ButtonControl BackwardButtonControl { get; set; }

        void CreateAxisAlignedWalkerPanel(int startX, int startY)
        {
            int labelWidth = 60;
            int labelHeight = 15;
            var label = new LabelControl
            {
                Text = "Axis Aligned Walker",
                Bounds = new UniRectangle(Pad + 30, Pad, labelWidth, labelHeight)
            };

            int buttonGridStartX = Pad;
            int buttonGridStartY = Pad + labelHeight + Pad;

            /*
             *  _   _   _   _   _   _   _
             *  |           5           |
             *  |   1   2   x   3   4   |
             *  |           6           |
            */

            // 5
            UpButtonControl = new ButtonControl
            {
                Text = "+y",
                Bounds = ButtonBoundsGrid(buttonGridStartX, buttonGridStartY, 2, 0)
            };
            UpButtonControl.Pressed += new EventHandler(UpButtonControl_Pressed);
            // 6
            DownButtonControl = new ButtonControl
            {
                Text = "-y",
                Bounds = ButtonBoundsGrid(buttonGridStartX, buttonGridStartY, 2, 2)
            };
            DownButtonControl.Pressed += new EventHandler(DownButtonControl_Pressed);
            // 1
            LeftButtonControl = new ButtonControl
            {
                Text = "-x",
                Bounds = ButtonBoundsGrid(buttonGridStartX, buttonGridStartY, 0, 1)
            };
            LeftButtonControl.Pressed += new EventHandler(LeftButtonControl_Pressed);
            // 4
            RightButtonControl = new ButtonControl
            {
                Text = "+x",
                Bounds = ButtonBoundsGrid(buttonGridStartX, buttonGridStartY, 4, 1)
            };
            RightButtonControl.Pressed += new EventHandler(RightButtonControl_Pressed);
            // 3
            ForwardButtonControl = new ButtonControl
            {
                Text = "+z",
                Bounds = ButtonBoundsGrid(buttonGridStartX, buttonGridStartY, 3, 1)
            };
            ForwardButtonControl.Pressed += new EventHandler(ForwardButtonControl_Pressed);
            // 2
            BackwardButtonControl = new ButtonControl
            {
                Text = "-z",
                Bounds = ButtonBoundsGrid(buttonGridStartX, buttonGridStartY, 1, 1)
            };
            BackwardButtonControl.Pressed += new EventHandler(BackwardButtonControl_Pressed);

            var focusHoleRect = ButtonBoundsGrid(buttonGridStartX, buttonGridStartY, 2, 1);
            focusHoleRect = new UniRectangle(focusHoleRect.Left + 3, focusHoleRect.Top, focusHoleRect.Size.X, focusHoleRect.Size.Y);
            var focusHoleChoice = new ChoiceControl
            {
                Text = "",
                Bounds = focusHoleRect
            };

            int numRows = 3;
            int numColumns = 5;
            int width = Pad + (ButtonWidth * numColumns) + (Pad * (numColumns - 1)) + Pad;
            int height =  Pad + labelHeight + Pad + (ButtonHeight * numRows) + (Pad * (numRows - 1)) + Pad;

            AxisAlignedPanelControl = new Control
            {
                Bounds = new UniRectangle(startX, startY, width, height)
            };

            foreach (var c in new Control[]{
                label,
                UpButtonControl,
                DownButtonControl,
                LeftButtonControl,
                RightButtonControl,
                ForwardButtonControl,
                BackwardButtonControl,
                focusHoleChoice
            })
            {
                AxisAlignedPanelControl.Children.Add(c);
            }
        }

        void BackwardButtonControl_Pressed(object sender, EventArgs e)
        {
            MoveDirection(Vector3.Backward);
        }

        void ForwardButtonControl_Pressed(object sender, EventArgs e)
        {
            MoveDirection(Vector3.Forward);
        }

        void RightButtonControl_Pressed(object sender, EventArgs e)
        {
            MoveDirection(Vector3.Right);
        }

        void LeftButtonControl_Pressed(object sender, EventArgs e)
        {
            MoveDirection(Vector3.Left);
        }

        void DownButtonControl_Pressed(object sender, EventArgs e)
        {
            MoveDirection(Vector3.Down);
        }
        
        void UpButtonControl_Pressed(object sender, EventArgs e)
        {
            MoveDirection(Vector3.Up);
        }

        void MoveDirection(Vector3 dir)
        {
            float size = GetDeltaSize();
            var delta = dir * size;

            var newNode = Tree.GetLeafContaining(Node.Box.GetCenter() + delta);
            if (newNode != null)
            {
                Node = newNode;
                UpdateViewState();
            }
        }

        float GetDeltaSize()
        {
            return Node.Box.Max.X - Node.Box.Min.X;
        }

        #endregion

        #region Tree Walker
        private Control TreeWalkerPanel { get; set; }
        private ButtonControl ParentButtonControl { get; set; }
        private List<ButtonControl> ChildButtons { get; set; }

        void CreateTreeWalkerPanel(UniScalar startX, UniScalar startY)
        {
            int labelWidth = 60;
            int labelHeight = 15;
            var label = new LabelControl
            {
                Text = "Tree Walker",
                Bounds = new UniRectangle(Pad + 40, Pad, labelWidth, labelHeight)
            };


            ParentButtonControl = new ButtonControl
            {
                Text = "Up",
                Bounds = new UniRectangle(Pad, Pad + labelHeight + Pad, ButtonWidth, ButtonHeight)
            };
            ParentButtonControl.Pressed += new EventHandler(ParentButtonControl_Pressed);

            ChildButtons = new List<ButtonControl>();
            for (int i = 0; i < OctTreeNode<T>.Subdivisions; i++)
            {
                var childButton = new ButtonControl
                    {
                        Text = i.ToString(),
                        Bounds = ButtonBoundsGrid(Pad, Pad + labelHeight + Pad + ButtonHeight + Pad, i, 0)
                    };
                childButton.Pressed += new EventHandler(childButton_Pressed);
                ChildButtons.Add(childButton);
            }

            int numRows = 2;
            int numColumns = 8;
            int width = Pad + (ButtonWidth * numColumns) + (Pad * (numColumns - 1)) + Pad;
            int height = Pad + labelHeight + Pad + (ButtonHeight * numRows) + (Pad * (numRows - 1)) + Pad;

            TreeWalkerPanel = new Control{
                Bounds = new UniRectangle(startX, startY, width, height)
            };

            TreeWalkerPanel.Children.Add(label);
            this.ChildButtons.Add(ParentButtonControl);
            foreach (var childButton in ChildButtons)
            {
                TreeWalkerPanel.Children.Add(childButton);
            }
        }

        void ParentButtonControl_Pressed(object sender, EventArgs e)
        {
            Node = Node.Parent;
            UpdateViewState();
        }

        void childButton_Pressed(object sender, EventArgs e)
        {
            // TODO - fix this
            int i = int.Parse((sender as ButtonControl).Text);

            Node = Node.Children[i];

            UpdateViewState();
        }

        void UpdateTreeWalker()
        {
            ParentButtonControl.Enabled = Node != null && Node.Parent != null;
            for (int i = 0; i < OctTreeNode<T>.Subdivisions; i++)
            {
            }
        }
        #endregion

        #region Node Info
        private Control NodeInfoPanel { get; set; }
        private LabelControl NodeInfoLabel { get; set; }
        private ButtonControl PruneButtonControl { get; set; }
        void CreateNodeInfoPanel(UniScalar startX, UniScalar startY)
        {
            int labelWidth = 200;
            int labelHeight = 15;
            var label = new LabelControl
            {
                Text = "Node Info",
                Bounds = new UniRectangle(Pad + 40, Pad, labelWidth, labelHeight)
            };

            int nodeInfoLabelWidth = 100;
            int nodeInfoLabelHeight = 30;
            NodeInfoLabel = new LabelControl
            {
                Bounds = new UniRectangle(Pad, Pad + labelHeight + Pad, nodeInfoLabelWidth, nodeInfoLabelHeight)
            };

            PruneButtonControl = new ButtonControl
            {
                Text = "Prune",
                Bounds = new UniRectangle(Pad + nodeInfoLabelWidth + Pad, Pad + labelHeight + Pad, ButtonWidth + ButtonWidth, ButtonHeight)
            };
            PruneButtonControl.Pressed += new EventHandler(PruneButtonControl_Pressed);
            UpdateNodeInfo();

            int width = Pad + Math.Max(nodeInfoLabelWidth, labelWidth) + Pad;
            int height = Pad + labelHeight + Pad + nodeInfoLabelHeight + Pad;
            NodeInfoPanel = new Control
            {
                Bounds = new UniRectangle(startX, startY, width, height)
            };

            NodeInfoPanel.Children.Add(label);
            NodeInfoPanel.Children.Add(NodeInfoLabel);
            NodeInfoPanel.Children.Add(PruneButtonControl);
        }

        void PruneButtonControl_Pressed(object sender, EventArgs e)
        {
            Node.Prune();
            UpdateViewState();
        }

        StringBuilder NodeInfoBuilder = new StringBuilder();
        void UpdateNodeInfo()
        {
            NodeInfoBuilder.Clear();
            if (Tree == null)
            {
                NodeInfoBuilder.AppendLine("No Tree Attached");
            }
            else
            {

            }

            if (Node == null)
            {
                PruneButtonControl.Enabled = false;
                NodeInfoBuilder.AppendLine("No Node Attached");
            }
            else
            {
                PruneButtonControl.Enabled = !Node.IsLeaf;
                NodeInfoBuilder.AppendFormat("Parent: {0}{1}", Node.Parent == null ? "NO" : "YES", Environment.NewLine);
                var t = Node.Value;
                NodeInfoBuilder.AppendFormat("Value: {0}{1}", t == null ? "NULL" : t.ToString(), Environment.NewLine);
            }

            NodeInfoLabel.Text = NodeInfoBuilder.ToString();

        }
        #endregion

        #region Bindings
        bool IsTreeBound { get; set; }
        OctTree<T> Tree { get; set; }
        OctTreeNode<T> Node { get; set; }
        public void Bind(OctTree<T> tree, OctTreeNode<T> node)
        {
            Tree = tree;
            Node = node;
            IsTreeBound = true;
            UpdateViewState();
        }

        void UpdateViewState()
        {
            UpdateNodeInfo();
            UpdateTreeWalker();
        }

        public void UnBind()
        {
            Tree = null;
            Node = null;
            IsTreeBound = false;
        }
        #endregion

        public OctTreeNode<T> CurrentNode { get { return Node; } }
    }
}
