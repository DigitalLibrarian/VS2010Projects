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
using Aquarium.GA.Organs;
using Aquarium.GA.Signals;
using Aquarium.GA.Genomes;
using Aquarium.GA.Phenotypes;
using Aquarium.GA.Headers;
using Aquarium.GA.Codons;
using Aquarium.GA.Bodies;
using System.Threading;

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
        RenderContext RenderContext;
        public ICamera Camera { get; set; }
        ScreenManager ScreenManager { get; set; }

        Random Random = new Random();

        
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            ScreenManager = new ScreenManager(this);
        }

       

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            this.Window.AllowUserResizing = true;
            this.IsMouseVisible = true;
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

            SetupRenderContextAndCamera();

            Components.Add(ScreenManager);

            ScreenManager.AddScreen(new SimScreen(RenderContext));

        }



        protected void SetupRenderContextAndCamera()
        {
            Camera = new EyeCamera();
            Camera.Position = new Vector3(-1f, 0f, 10f);
            RenderContext = new RenderContext(
                Camera,
                GraphicsDevice
                );
        }

      


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                 || Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            Terminal.CheckOpen(Keys.OemTilde, Keyboard.GetState(PlayerIndex.One));


            base.Update(gameTime);

        }


      

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            base.Draw(gameTime);
            
            Set2DRenderStates();
            Terminal.CheckDraw(true);
            Set3DRenderStates();
        }

        void Set2DRenderStates()
        {
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
        }
        void Set3DRenderStates()
        {
            // Set suitable renderstates for drawing a 3D model
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
        }
    }
}
