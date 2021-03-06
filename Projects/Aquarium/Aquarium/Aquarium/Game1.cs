using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using DebugTerminal;
using Forever.Render.Cameras;
using Forever.Render;
using Forever.Screens;

using Forever.Neural;
using Aquarium.Life.Organs;
using Aquarium.Life.Signals;
using Aquarium.Life.Genomes;
using Aquarium.Life.Phenotypes;
using Aquarium.Life.Bodies;

using Ruminate.GUI.Framework;
using Ruminate.GUI.Content;
using System.IO;
using System.Windows.Forms;


namespace Aquarium
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;

        ScreenManager ScreenManager { get; set; }
        Random Random = new Random();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            ScreenManager = new ScreenManager(this, graphics);

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            GC.CancelFullGCNotification();
            this.Window.AllowUserResizing = true;
            this.IsMouseVisible = true;

            var form = (Form)Form.FromHandle(Window.Handle);
            form.WindowState = FormWindowState.Maximized;

            base.Initialize();

            ScreenManager.Initialize();

        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("TerminalFont");
            TerminalSkin skin = new TerminalSkin(TerminalThemeType.HALLOWEEN_TWO);

            Terminal.Init(this, spriteBatch, spriteFont, GraphicsDevice);
            Terminal.SetSkin(skin);

            Reset();

            Components.Add(ScreenManager);
        }

        void Reset()
        {
            ScreenManager.ExitAll();
            var startScreen = new DevScreenSwitchLauncher();
            ScreenManager.AddScreen(startScreen);
            startScreen.BringUpSwitcherScreen();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed
                || !ScreenManager.GetScreens().Any())
                this.Exit();

            Terminal.CheckOpen(Microsoft.Xna.Framework.Input.Keys.OemTilde, Keyboard.GetState(PlayerIndex.One));

            base.Update(gameTime);
        }
      
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            RenderContext.Set3DRenderStates(GraphicsDevice);
            base.Draw(gameTime);

            RenderContext.Set2DRenderStates(GraphicsDevice);
            Terminal.CheckDraw(true);
        }
    }
}
