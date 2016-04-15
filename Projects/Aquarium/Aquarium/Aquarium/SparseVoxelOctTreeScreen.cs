using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Forever.Extensions;
using Microsoft.Xna.Framework.Graphics;
using Forever.Voxel.SVO;
using Forever.Screens;
using Forever.Render;
using Microsoft.Xna.Framework.Input;
using Aquarium.Ui;
using Aquarium.UI;
using Aquarium.UI.Controls;

namespace Aquarium
{
    class SparseVoxelOctTreeScreen : FlyAroundGameScreen
    {
        OctTree<int> Tree { get; set; }
        int MaxTreeDepth { get; set; }
        int RenderDepth { get; set; }

        LabelUiElement FaceSizeLabel { get; set; }
        TreeTesterWindowControl<int> TreeTesterWindowControl { get; set; }

        public override void LoadContent()
        {
            PropagateInput = true;
            base.LoadContent();

            var n = 1024f / 200f;
            var treeBox = new BoundingBox(new Vector3(-n, -n, -n), new Vector3(n, n, n));

            MaxTreeDepth = 8;
            Tree = OctTree<int>.CreatePreSubdivided(MaxTreeDepth, treeBox);

            RenderDepth = 0;

            var diff = treeBox.Max - treeBox.Min;
            var startPos = Vector3.Forward * (diff.Length());
            RenderContext.Camera.Position = startPos;
            User.Body.Position = startPos;
            User.Body.Orientation = Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(startPos, Tree.Root.Box.GetCenter(), Vector3.Up));
            Ui.Elements.AddRange(CreateUILayout());

            TreeTesterWindowControl = new TreeTesterWindowControl<int>(1500, 300);
            this.GuiManager.Screen.Desktop.Children.Add(TreeTesterWindowControl);

            TreeTesterWindowControl.Bind(this.Tree, this.Tree.Root);
        }

        List<IUiElement> CreateUILayout()
        {
            // TODO - move this to a window control TreeViewer that can be reused as a debugging tool
            var spriteFont = ScreenManager.Font;
            FaceSizeLabel = new LabelUiElement(RenderContext, spriteFont, DebugLabelStrip());

            return new List<IUiElement>{
                FaceSizeLabel
            };
        }

        IEnumerable<OctTreeNode<int>> RayPathSequence { get; set; }

        public override void HandleInput(InputState input)
        {
            base.HandleInput(input);
            if(input.IsNewKeyPress(Keys.OemPlus))
            {
                if(RenderDepth < MaxTreeDepth) RenderDepth++;
            }

            if(input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.OemMinus))
            {
                if(RenderDepth > 0) RenderDepth--;
            }

            if (input.IsMouseLeftClick())
            {
                var r = RenderContext.GetScreenRay(input.CurrentMousePoint.ToVector2());
                Ray = new Ray(r.Position + r.Direction, r.Direction);
                bool intersects = Tree.Root.Box.Intersects(Ray.Value).HasValue;
                if (intersects)
                {
                    RayColor = Color.Blue;
                    // TODO - dispose 
                    RayPathSequence = Tree.RayCast(r);
                }
                else
                {
                    // TODO - dispose 
                    RayPathSequence = null;
                    RayColor = Color.SlateGray;
                }
            }
        }

        Ray? Ray { get; set; }
        Color RayColor { get; set; }

        public override void Draw(GameTime gameTime)
        {
            float smallest = float.MaxValue;
            Tree.VisitAtDepth(node =>
            {
                if (node == null) return;
                Renderer.Render(RenderContext, node.Box, Color.Red);

                // get box edge length
                float l = Math.Abs(node.Box.Max.X - node.Box.Min.X);
                if (l < smallest)
                {
                    smallest = l;
                }


            }, RenderDepth);

            var testerNode = TreeTesterWindowControl.CurrentNode;
            if (testerNode != null)
            {
                Renderer.Render(RenderContext, testerNode.Box, Color.White);
            }

            if (Ray.HasValue)
            {
                DebugDrawer.DrawLine(Ray.Value.Position, Ray.Value.Position + (Ray.Value.Direction*100), RayColor);
                DebugDrawer.Draw(gameTime);
            }

            if (RayPathSequence != null)
            {
                foreach (var n in RayPathSequence)
                {
                    Renderer.Render(RenderContext, n.Box, Color.YellowGreen);
                }
            }

            FaceSizeLabel.Label = string.Format("Depth: {0} Size: {1}", RenderDepth, smallest);
            base.Draw(gameTime);
        }
    }

}
