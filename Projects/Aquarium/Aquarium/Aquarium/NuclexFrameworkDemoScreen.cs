using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Nuclex.Graphics.Debugging;
using Microsoft.Xna.Framework.Graphics;
using Forever.Render.Cameras;
using Forever.Render;
using Microsoft.Xna.Framework;
using Nuclex.UserInterface;
using Nuclex.Input;
using Nuclex.UserInterface.Controls.Desktop;
using Nuclex.UserInterface.Controls.Arcade;
using Nuclex.Graphics.SpecialEffects.Water;
using Aquarium.UI.Controls;
using Nuclex.Graphics.Batching;
using Aquarium.Targeting;

namespace Aquarium
{

    public class DumbTarget : ITarget
    {

        string ITarget.Label
        {
            get { return "I am a target label woo hoo!"; }
        }

        BoundingBox ITarget.TargetBB
        {
            get { return new BoundingBox(); }
        }
    }
    class NuclexFrameworkDemoScreen : FlyAroundGameScreen
    {
        Model SkySphere;
        Effect SkySphereEffect;
        public override void LoadContent()
        {
            base.LoadContent();

            var targetWindow = new TargetWindowControl(1310, 10);
            targetWindow.SetNewTarget(new DumbTarget());
            this.GuiManager.Screen.Desktop.Children.Add(targetWindow);
            this.GuiManager.Screen.Desktop.Children.Add(new PopulationWindowControl(10, 50));
            this.GuiManager.Screen.Desktop.Children.Add(new DemoDialog());
            this.GuiManager.Screen.Desktop.Children.Add(new DebugLogWindowControl(100, 750));
            this.GuiManager.Screen.Desktop.Children.Add(new GarbageCollectionWindowControl(600, 200));
            
            User.Body.Position = Vector3.Backward * 10;
            var content = ScreenManager.Game.Content;
            SkySphereEffect = content.Load<Effect>("Effects//SkySphere");
            TextureCube SkyboxTexture =
                content.Load<TextureCube>("TextureCube//Space");
            SkySphere = content.Load<Model>("Models//SphereHighPoly");
            SkySphereEffect.Parameters["SkyboxTexture"].SetValue(SkyboxTexture);
            foreach (ModelMesh mesh in SkySphere.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = SkySphereEffect;
                }
            }
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //DebugDrawer.DrawSolidArrow(Vector3.Zero, Vector3.Up, Color.White);
            //DebugDrawer.Draw(gameTime);
            var view = RenderContext.Camera.View;
            var proj = RenderContext.Camera.Projection;
            SkySphereEffect.Parameters["ViewMatrix"].SetValue(view);
            SkySphereEffect.Parameters["ProjectionMatrix"].SetValue(proj);
            // Draw the sphere model that the effect projects onto
            for(int i = 0; i < SkySphere.Meshes.Count;i++)
            {
                SkySphere.Meshes[i].Draw();
            }

            base.Draw(gameTime);
        }
    }

    /// <summary>Dialog that demonstrates the capabilities of the GUI library</summary>
    public partial class DemoDialog : WindowControl
    {
        /// <summary>Initializes a new GUI demonstration dialog</summary>
        public DemoDialog()
        {
            InitializeComponent();
        }
    }

    partial class DemoDialog
    {

        #region NOT Component Designer generated code

        /// <summary> 
        ///   Required method for user interface initialization -
        ///   do modify the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            float TotalWidth = 512f;
            this.Title = "Windows have titles";
            this.Bounds = new UniRectangle(770.0f, 100.0f, TotalWidth, 384.0f);
            this.helloWorldLabel = new Nuclex.UserInterface.Controls.LabelControl();
            this.horSliderLabel = new Nuclex.UserInterface.Controls.LabelControl();
            this.verSliderLabel = new Nuclex.UserInterface.Controls.LabelControl();
            this.okButton = new Nuclex.UserInterface.Controls.Desktop.ButtonControl();
            this.okButton.Text = "Okay";
            this.cancelButton = new Nuclex.UserInterface.Controls.Desktop.ButtonControl();
            this.cancelButton.Text = "Cancel";
            //
            // helloWorldLabel
            //
            this.helloWorldLabel.Text = "Hello World! This is a label.";
            this.helloWorldLabel.Bounds = new UniRectangle(10.0f, 25.0f, 110.0f, 30.0f);
            //
            // okButton
            //
            this.okButton.Bounds = new UniRectangle(
              new UniScalar(1.0f, -180.0f), new UniScalar(1.0f, -40.0f), 80, 24
            );
            //
            // cancelButton
            //
            this.cancelButton.Bounds = new UniRectangle(
              new UniScalar(1.0f, -90.0f), new UniScalar(1.0f, -40.0f), 80, 24
            );

            this.horSliderLabel.Text = "Horizontal Slider";
            this.horSliderLabel.Bounds = new UniRectangle(10, 80, 100, 20);
            this.horSlider = new HorizontalSliderControl
            {
                Bounds = new UniRectangle(10, 100, 100, 20),
                ThumbSize = 0.33f,
                ThumbPosition = 0.25f,
            };
            this.verSliderLabel.Text = "Vertical Slider";
            this.verSliderLabel.Bounds = new UniRectangle(180, 140, 100, 20);
            this.verSlider = new VerticalSliderControl
            {
                Bounds = new UniRectangle(150, 100, 20, 100),
                ThumbSize = 0.33f,
                ThumbPosition = 0.25f,
            };

            this.choice1 = new ChoiceControl { 
                Text = "Choice #1",
                Bounds = new UniRectangle(10, 150, 100, 20)
            };
            this.choice2 = new ChoiceControl {
                Text = "Choice #2",
                Bounds = new UniRectangle(10, 180, 100, 20) 
            };
            this.choice3 = new ChoiceControl {
                Text = "Choice #3",
                Bounds = new UniRectangle(10, 210, 100, 20)
            };

            this.list = new ListControl
            {
                Bounds = new UniRectangle(300, 30, 200, 200),
                SelectionMode = ListSelectionMode.Multi
            };

            foreach (var n in Enumerable.Range(0, 200))
            {
                list.Items.Add(n.ToString());
            }

            this.option = new OptionControl
            {
                Bounds = new UniRectangle(300, 250, 100, 20)
            };
            this.option.Text = "Do you like chicken?";

            this.closeWindowButton = new Aquarium.UI.Controls.CloseWindowButtonControl(this);

            Children.Add(this.helloWorldLabel);
            Children.Add(this.okButton);
            Children.Add(this.cancelButton);
            Children.Add(this.horSlider);
            Children.Add(this.verSlider);
            Children.Add(this.horSliderLabel);
            Children.Add(this.verSliderLabel);
            Children.Add(this.choice1);
            Children.Add(this.choice2);
            Children.Add(this.choice3);
            Children.Add(this.list);
            Children.Add(this.option);
            Children.Add(closeWindowButton);
        }


        #endregion // NOT Component Designer generated code

        /// <summary>A label used to display a 'hello world' message</summary>
        private Nuclex.UserInterface.Controls.LabelControl helloWorldLabel;
        private Nuclex.UserInterface.Controls.LabelControl horSliderLabel;
        private Nuclex.UserInterface.Controls.LabelControl verSliderLabel;
        /// <summary>Button which exits the dialog and takes over the settings</summary>
        private Nuclex.UserInterface.Controls.Desktop.ButtonControl okButton;
        /// <summary>Button which exits the dialog and discards the settings</summary>
        private Nuclex.UserInterface.Controls.Desktop.ButtonControl cancelButton;

        private Nuclex.UserInterface.Controls.Desktop.HorizontalSliderControl horSlider;
        private Nuclex.UserInterface.Controls.Desktop.VerticalSliderControl verSlider;

        private Nuclex.UserInterface.Controls.Desktop.ChoiceControl choice1;
        private Nuclex.UserInterface.Controls.Desktop.ChoiceControl choice2;
        private Nuclex.UserInterface.Controls.Desktop.ChoiceControl choice3;

        private Nuclex.UserInterface.Controls.Desktop.ListControl list;
        private Nuclex.UserInterface.Controls.Desktop.OptionControl option;
        private Aquarium.UI.Controls.CloseWindowButtonControl closeWindowButton;
    }
}
