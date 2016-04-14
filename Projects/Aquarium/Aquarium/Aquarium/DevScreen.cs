using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.Screens;
using Nuclex.UserInterface;
using Nuclex.Input;
using Forever.Render;
using Nuclex.Graphics.Debugging;
using Forever.Render.Cameras;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Nuclex.UserInterface.Visuals.Flat;

namespace Aquarium
{
    public class DevScreen : GameScreen, IGraphicsDeviceService
    {
        protected InputManager InputManager { get; set; }

        protected GuiManager GuiManager { get; set; }
        protected Random Random { get; private set; }

        protected RenderContext RenderContext { get; set; }
        protected DebugDrawer DebugDrawer { get; set; }
        public DevScreen(Random random)
        {
            Random = random;
        }

        public DevScreen()
        {
            Random = new Random();
        }

        public override void LoadContent()
        {
            PropagateInput = true;
            PropagateDraw = true;
            base.LoadContent();
            var gd = ScreenManager.GraphicsDevice;
            RenderContext = new RenderContext(
                    new EyeCamera(gd),
                    gd
                );

            InputManager = new InputManager();
            GuiManager = new Nuclex.UserInterface.GuiManager(ScreenManager.GraphicsDeviceManager, InputManager);
            GuiManager.Visualizer = FlatGuiVisualizer.FromResource(ScreenManager.Game.Services, Resources.ResourceManager, "Suave_skin");
            GuiManager.Initialize();
            DebugDrawer = new DebugDrawer(this);

            // Create a new screen. Screens manage the state of a GUI and its rendering
            // surface. If you have a GUI in your game window, you'd first create a screen
            // for that. If you have an in-game computer display where you want to use
            // a GUI, you can create another screen for that and thus cleanly separate
            // the state of the in-game computer from your game's main menu GUI :)
            Viewport viewport = GraphicsDevice.Viewport;
            Screen mainScreen = new Screen(viewport.Width, viewport.Height);
            this.GuiManager.Screen = mainScreen;
        }
        public override void UnloadContent()
        {
            // TODO - probably should dispose of that visualizer
            GuiManager.Dispose();
            GuiManager = null;

            InputManager.Dispose();
            InputManager = null;

            DebugDrawer.Dispose();
            DebugDrawer = null;
            base.UnloadContent();
        }

        public override void HandleInput(InputState input)
        {
            base.HandleInput(input);

            InputManager.Update();
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            DebugDrawer.Reset();
            DebugDrawer.ViewProjection = RenderContext.Camera.View * RenderContext.Camera.Projection;
            GuiManager.Draw(gameTime);

            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (!otherScreenHasFocus && !coveredByOtherScreen)
            {
                GuiManager.Update(gameTime);
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        #region IGraphicsDeviceService
        public event EventHandler<EventArgs> DeviceCreated;

        public event EventHandler<EventArgs> DeviceDisposing;

        public event EventHandler<EventArgs> DeviceReset;

        public event EventHandler<EventArgs> DeviceResetting;

        public GraphicsDevice GraphicsDevice
        {
            get { return ScreenManager.Game.GraphicsDevice; }
        }
        #endregion
    }
}
