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

namespace Aquarium
{
    class NuclexFrameworkScreen : DevScreen, IGraphicsDeviceService
    {
        GuiManager GuiManager { get; set; }
        RenderContext RenderContext { get; set; }
        DebugDrawer DebugDrawer { get; set; }

        public override void LoadContent()
        {
            base.LoadContent();
            var gd = ScreenManager.Game.GraphicsDevice;

            var input = new InputManager();

            GuiManager = new Nuclex.UserInterface.GuiManager(ScreenManager.GraphicsDeviceManager, input);
            ScreenManager.Game.Components.Add(GuiManager);
            ScreenManager.Game.Components.Add(input);

            RenderContext = new RenderContext(
                    new EyeCamera(gd),
                    gd
                );
            RenderContext.Camera.Position = Vector3.Backward * 10;
            DebugDrawer = new DebugDrawer(this);

            // Create a new screen. Screens manage the state of a GUI and its rendering
            // surface. If you have a GUI in your game window, you'd first create a screen
            // for that. If you have an in-game computer display where you want to use
            // a GUI, you can create another screen for that and thus cleanly separate
            // the state of the in-game computer from your game's main menu GUI :)
            Viewport viewport = GraphicsDevice.Viewport;
            Screen mainScreen = new Screen(viewport.Width, viewport.Height);
            this.GuiManager.Screen = mainScreen;

            // Each screen has a 'desktop' control. This invisible control by default
            // stretches across the whole screen (all controls are positioned using both
            // a percentual position/size and absolute position/size). We change this to
            // prevent GUI or HUD elements from appearing outside the title-safe area.
            mainScreen.Desktop.Bounds = new UniRectangle(
              new UniScalar(0.1f, 0.0f), new UniScalar(0.1f, 0.0f), // x and y
              new UniScalar(0.8f, 0.0f), new UniScalar(0.8f, 0.0f) // width and height
            );


            // Next, we add our demonstration dialog to the screen
            mainScreen.Desktop.Children.Add(new DemoDialog());
            mainScreen.Desktop.Children.Add(new DemoPanel());

        }

        public event EventHandler<EventArgs> DeviceCreated;

        public event EventHandler<EventArgs> DeviceDisposing;

        public event EventHandler<EventArgs> DeviceReset;

        public event EventHandler<EventArgs> DeviceResetting;

        public GraphicsDevice GraphicsDevice
        {
            get { return ScreenManager.Game.GraphicsDevice; }
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            DebugDrawer.ViewProjection = RenderContext.Camera.View * RenderContext.Camera.Projection;

            DebugDrawer.DrawSolidArrow(Vector3.Zero, Vector3.Up, Color.White);

            DebugDrawer.Draw(gameTime);
            base.Draw(gameTime);
        }
    }


    class DemoPanel : PanelControl
    {
        public DemoPanel()
        {
            this.Bounds = new UniRectangle(500, 500, 200, 200);
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
            this.Title = "Windows have titles";
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
            //
            // DemoDialog
            //
            this.Bounds = new UniRectangle(100.0f, 100.0f, 512.0f, 384.0f);

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

            this.closeWindowButton = new CloseWindowButtonControl()
            {
                Text = "X",
                Bounds = new UniRectangle(this.Bounds.Right - 125, 2, 20, 20),
                //Enabled = true
            };
            this.closeWindowButton.Pressed += new EventHandler(closeWindowButton_Pressed);

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

        void closeWindowButton_Pressed(object sender, EventArgs e)
        {
            Close();
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
        private Nuclex.UserInterface.Controls.Desktop.CloseWindowButtonControl closeWindowButton;
    }
}
