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

namespace Aquarium
{
    class SparseVoxelOctTreeScreen : FlyAroundGameScreen
    {
        SparseVoxelOctTree Tree { get; set; }
        int MaxTreeDepth { get; set; }
        int RenderDepth { get; set; }

        LabelUiElement FaceSizeLabel { get; set; }

        public override void LoadContent()
        {
            base.LoadContent();

            var n = 1024;
            var treeBox = new BoundingBox(new Vector3(-n, -n, -n), new Vector3(n, n, n));

            MaxTreeDepth = 5;
            Tree = SparseVoxelOctTree.CreatePreSubdivided(MaxTreeDepth, treeBox);

            RenderDepth = 0;

            var diff = treeBox.Max - treeBox.Min;
            var startPos = Vector3.Backward * (diff.Length());
            RenderContext.Camera.Position = startPos;
            User.Body.Position = startPos;
            Ui.Elements.AddRange(CreateUILayout());

        }

        List<IUiElement> CreateUILayout()
        {
            var spriteFont = ScreenManager.Font;
            FaceSizeLabel = new LabelUiElement(RenderContext, spriteFont, DebugLabelStrip());

            return new List<IUiElement>{
                FaceSizeLabel
            };
        }

        public override void HandleInput(InputState input)
        {
            if(input.IsNewKeyPress(Keys.OemPlus))
            {
                if(RenderDepth < MaxTreeDepth) RenderDepth++;
            }

            if(input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.OemMinus))
            {
                if(RenderDepth > 0) RenderDepth--;
            }

            base.HandleInput(input);
        }

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

            FaceSizeLabel.Label = string.Format("Depth: {0} Size: {1}", RenderDepth, smallest);
            base.Draw(gameTime);
        }
    }

}
