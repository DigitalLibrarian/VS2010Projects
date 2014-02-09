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
using Aquarium.GA.Genomes;
using Aquarium.GA.Phenotypes;
using Aquarium.GA.GeneParsers;
using Aquarium.GA.Codons;

namespace Aquarium
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Timer GenerateTimer = new Timer(1000);

        BodyGenerator BodyGen = new BodyGenerator();
        Random Random = new Random();
        RenderContext RenderContext;
        ICamera Camera;

        int MaxPop = 100;
        int NumBest = 50;
        List<Body> BestBodies = new List<Body>();
        List<BodyGenome> BestGenomes = new List<BodyGenome>();
        List<BodyGenome> PopGenomes = new List<BodyGenome>();

        long Births = 0;

        float rot = 0;
        Body Body { get; set; }
        BodyGenome Genome { get; set; }
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            GenerateTimer.AutoReset = true;
            GenerateTimer.Elapsed += new ElapsedEventHandler(genTimer_Elapsed);
        }

        #region Timer

        void genTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Generate();
        }

        bool goingOff = false;
        private void Generate()
        {
            if (goingOff) return;
            goingOff = true;
            
            for (int i = 0; i < NumBest; i++)
            {
                SpawnBodyFromGenePool();
            }

            Body = GetBestHitterBody();
            Genome = GetBestHitterGenome();
            goingOff = false;
        }

        #endregion

        #region Population

        private void GenerateRandomPopulation(int popSize)
        {
            int numParts =  9;

            int generated = 0;
            while (generated < popSize)
            {
                if (InsertRandom(numParts))
                {
                    generated++;
                }
            }
        }

        private bool InsertRandom(int numParts)
        {
            var genome = RandomGenome(numParts);

            PhenotypeReader gR = new PhenotypeReader();
            var pheno = GenomeToPheno(genome);
            if (pheno != null)
            {
                var body = gR.ProduceBody(pheno);
                if (body != null)
                {
                    RegisterBodyGenome(genome, body);
                    return true;
                }
            }
            return false;
        }

        public void SpawnBodyFromGenePool()
        {
            int popSize = PopGenomes.Count();
            var parent1Gen = Random.NextElement(BestGenomes);
            
            var strangeList = PopGenomes;
            if (!strangeList.Any() || Random.Next(4) == 0) strangeList = BestGenomes;

            var parent2Gen = Random.NextElement(strangeList);

            if (strangeList == PopGenomes)
            {
                //parent2Gen is  some strange from general pop
                parent2Gen = new BodyGenome(parent2Gen.Genes.Select(g => new Gene<int> { Name = g.Name, Value = g.Value }).ToList());

                parent2Gen.Mutate(Random);
            }

            
            int minCount = Math.Min(parent1Gen.Size, parent2Gen.Size);
            int wiggle = Random.Next(minCount/8);
            int snip = (-wiggle + Random.Next(wiggle*2)) + (minCount / 2);


            var parent1Prefix = parent1Gen.Genes.Take(snip);
            var parent1Suffix = parent1Gen.Genes.Skip(snip);
            var parent2Prefix = parent2Gen.Genes.Take(snip);
            var parent2Suffix = parent2Gen.Genes.Skip(snip);


            var offspring1Genes = parent1Prefix.Concat(parent2Suffix).Select(g=> new Gene<int> { Name = g.Name, Value = g.Value} ).ToList();
            var offspring2Genes = parent2Prefix.Concat(parent1Suffix).Select(g => new Gene<int> { Name = g.Name, Value = g.Value }).ToList();
            
            PhenotypeReader gR = new PhenotypeReader();
            foreach (var genes in new[] { offspring1Genes, offspring2Genes })
            {
                var genome = new BodyGenome(genes);
                var pheno = GenomeToPheno(genome);
                if (pheno != null)
                {
                    var body = gR.ProduceBody(pheno);
                    if (body != null)
                    {
                        RegisterBodyGenome(genome, body);
                    }
                }
            }
        }

        private bool AsFit(BodyGenome g1, Body b1, BodyGenome g2, Body b2)
        {
            var c1 = b1.Parts.Count();
            var c2 = b2.Parts.Count();
            if (c2 > c1) return true;
            if (c2 < c1) return false;

            return true;
        }

        private void RegisterBodyGenome(BodyGenome genome, Body body)
        {
            if (genome.Size > 2000) return;
            if (!body.Parts.Any()) return;
            int numParts = body.Parts.Count();
            bool foundFit = false;
            for (int i = 0; i < BestGenomes.Count(); i++)
            {
                if(AsFit(BestGenomes[i], BestBodies[i], genome, body))
                {
                    BestBodies[i] = body;
                    BestGenomes[i] = genome;
                    foundFit = true;
                }
            }

            if (!foundFit && BestGenomes.Count() < NumBest)
            {
                BestBodies.Add(body);
                BestGenomes.Add(genome);

                foundFit = true;
            }

            if (!foundFit)
            {
                if (PopGenomes.Count() < MaxPop)
                {
                    PopGenomes.Add(genome);
                }
                else
                {
                    var index = Random.Next(PopGenomes.Count());
                    PopGenomes[index] = genome;
                }
            }

            Births++;

        }

        private Body GetBestHitterBody()
        {
            return BestBodies.First();
        }

        private BodyGenome GetBestHitterGenome()
        {
            return BestGenomes.First();
        }


        BodyGenome RandomGenome(int length)
        {
            var gContents = new List<Gene<int>>();

            List<int> codonContents;
            int name = 0;
            for (int i = 0; i < length; i++)
            {
                codonContents = new BodyPartStartCodon().Example();
                codonContents.ForEach(v => gContents.Add(
                            new Gene<int> { Name = name++, Value = v }
                            ));

                for (int j = 0; j < 9; j++)
                {
                    var v = Random.Next();
                    gContents.Add(
                            new Gene<int> { Name = name++, Value = v }
                        );
                };

                
                codonContents = new BodyPartEndCodon().Example();


                codonContents = new OrganStartCodon().Example();
                codonContents.ForEach(v => gContents.Add(
                            new Gene<int> { Name = name++, Value = v }
                            ));

                for (int j = 0; j < 15; j++)
                {
                    var v = Random.Next();
                    gContents.Add(
                            new Gene<int> { Name = name++, Value = v }
                        );
                };


                codonContents = new OrganEndCodon().Example();
                codonContents.ForEach(v => gContents.Add(
                            new Gene<int> { Name = name++, Value = v }
                            ));

                codonContents.ForEach(v => gContents.Add(
                            new Gene<int> { Name = name++, Value = v }
                            ));

            }


            codonContents = new BodyEndCodon().Example();

            codonContents.ForEach(v => gContents.Add(
                        new Gene<int> { Name = name++, Value = v }
                        ));
            return new BodyGenome(gContents);
            
        }


        private IBodyPhenotype GenomeToPheno(BodyGenome g)
        {
            var t = new RandomIntGenomeTemplate(Random);
            var parser = new BodyCodonParser();

            return parser.ParseBodyPhenotype(g, t);
        }

        #endregion

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
        SpriteFont spriteFont;
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

            GenerateRandomPopulation(MaxPop + NumBest);
            Body = GetBestHitterBody();
            Genome = GetBestHitterGenome();

            GenerateTimer.Enabled = true;
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

        private void CamTrackBody(Body body)
        {
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

            CamTrackBody(Body);
        }

        private void UpdateSimulation(GameTime gameTime)
        {
            Body.World = Matrix.CreateRotationY(rot += 0.01f)
                * Matrix.CreateRotationX(rot);

            Body.Update((float)gameTime.ElapsedGameTime.Milliseconds);


        }

      

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

            spriteBatch.Begin();


            var labels = new string[] {
                string.Format("Best Hitters: {0}", BestGenomes.Count()),
                string.Format("Gen.  Pop.: {0}", PopGenomes.Count()),
                string.Format("Births : {0}", Births),
                string.Format("GenomeSize : {0}", Genome.Size),
                string.Format("Parts : {0}", Body.Parts.Count)
            };

            int y = 0;
            int change = 16;
            foreach (var text in labels)
            {
                spriteBatch.DrawString(spriteFont, text, new Vector2(0, y), Color.Yellow);
                y += change;
            }

            spriteBatch.End();
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
