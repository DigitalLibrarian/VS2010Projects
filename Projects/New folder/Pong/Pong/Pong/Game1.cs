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

using Pong.ArenaMatch;

namespace Pong
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        DrawSpriteBatch spriteBatch;

        Arena Arena { get; set; }

        NeuralPaddle N { get; set; }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new DrawSpriteBatch(GraphicsDevice);


            SpriteFont spriteFont = Content.Load<SpriteFont>("TerminalFont");
            TerminalSkin skin = new TerminalSkin(TerminalThemeType.HALLOWEEN_TWO);

            Terminal.Init(this, spriteBatch, spriteFont, GraphicsDevice);
            Terminal.SetSkin(skin);
            var centerLine = new Vector2(500, 200);
            var paddles = new List<Paddle>();
            var teach = new PerfectPaddle(new Vector2(50, centerLine.Y), 100f, 10f);
            N = new NeuralPaddle(teach, new Vector2(50, centerLine.Y), 100f, 10f, new Random());
            
            paddles.Add(N);
            paddles.Add(new PerfectPaddle(new Vector2(650, centerLine.Y), 100f, 10f));
            var ball = new Ball(centerLine, new Vector2(0.4f, -0.2f), 5f);
            Arena = new Arena(700, 475, paddles, ball);


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


            float duration = (float)gameTime.ElapsedGameTime.Milliseconds;
            Arena.Update( duration );

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            Arena.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);

            Terminal.CheckDraw(true);
        }
    }
}
