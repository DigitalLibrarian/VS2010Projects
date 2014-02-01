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
using Aquarium.GA.Bodies;
using Forever.Render.Cameras;
using Forever.Render;

using System.Timers;
using Aquarium.GA.Organs;
using Forever.Neural;
using Aquarium.GA.Signals;

namespace Aquarium
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Timer GenerateTimer = new Timer(30000);

        BodyGenerator BodyGen = new BodyGenerator();
       
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            GenerateTimer.AutoReset = true;
            GenerateTimer.Elapsed += new ElapsedEventHandler(genTimer_Elapsed);
        }

        void genTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Generate();
        }
        private void Generate()
        {
            var body = BodyGen.GenerateBody();

            var min = new Vector3();
            var max = new Vector3();
            foreach (var part in body.Parts)
            {
                var bsc = part.BodySpaceCorners();

                foreach (var vec in bsc)
                {
                    min = Vector3.Min(vec, min);
                    max = Vector3.Max(vec, max);
                }
            }
            var s = BoundingSphere.CreateFromBoundingBox(new BoundingBox(min, max));
            Camera.Position = Vector3.UnitZ * s.Radius * 3f;

            Body = body;
            /*
            var organs = new List<Organ>();

            body.Parts.ForEach(part =>
            {
                var network = new NeuralNetwork(2, 2, 2);
                network.RandomizeWeights(this.BodyGen.Random);
                var organ = new NeuralOrgan(network);
                organ.ReceiveSignal(new Signal(new List<double> { 0.0, 0.0 }));
                organs.Add(organ);
            });


            var network = new NeuralNetwork(2, 2, 2);
            network.RandomizeWeights(this.BodyGen.Random);
            var organ = new NeuralOrgan(network);
            organ.ReceiveSignal(new Signal(new List<double> { 0.0, 0.0 }));

          
            */
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
            base.Initialize();

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            SpriteFont spriteFont = Content.Load<SpriteFont>("TerminalFont");
            TerminalSkin skin = new TerminalSkin(TerminalThemeType.HALLOWEEN_TWO);

            Terminal.Init(this, spriteBatch, spriteFont, GraphicsDevice);
            Terminal.SetSkin(skin);



            SetupRenderContextAndCamera();

            Generate();


            GenerateTimer.Enabled = true;
        }


        RenderContext RenderContext;
        ICamera Camera;
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
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
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

            UpdateSimulation(gameTime);

            base.Update(gameTime);

        }

        float rot = 0;
        private void UpdateSimulation(GameTime gameTime)
        {
            Body.World = Matrix.CreateRotationY(rot += 0.01f)
                * Matrix.CreateRotationX(rot);
            Body.Update((float)gameTime.ElapsedGameTime.Milliseconds);
        }

        Body Body { get; set; }
      

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            Set3DRenderStates();
            Matrix world = Matrix.Identity;

            Body.Render(RenderContext);

            base.Draw(gameTime);

            Set2DRenderStates();
            Terminal.CheckDraw(true);
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
