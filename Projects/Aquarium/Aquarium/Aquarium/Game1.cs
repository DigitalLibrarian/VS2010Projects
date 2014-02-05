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

            SpawnBodyFromGenePool();

            Body = GetBestHitter();
        }

        private Body GenerateNewSpecimen()
        {
            var genome = RandomGenome(1 * 9);
            // this takes the genome  out of the equation
            // var bodyPheno = CreateHandCraftedGenome();


            GenomeReader gR = new GenomeReader();

            var body = gR.ProduceBody(GenomeToPheno(genome));
            RegisterBodyGenome(genome, body);
         
            
            
            return body;
        }


        private void GenerateRandomPopulation(int popSize)
        {
            int geneSize = 1 * 9;

            int generated = 0;
            while (generated < popSize)
            {
                var genome = RandomGenome(geneSize);

                GenomeReader gR = new GenomeReader();

                var body = gR.ProduceBody(GenomeToPheno(genome));

                RegisterBodyGenome(genome, body);
                generated++;
            }
        }

        public void SpawnBodyFromGenePool()
        {

            int popSize = PopGenomes.Count();
            var parent1Gen = Random.NextElement(BestGenomes.GetRange(0, Math.Min(10, BestBodies.Count())));
            //var parent2Gen = Random.NextElement(BestGenomes.GetRange(0, Math.Min(10, BestBodies.Count())));

            var strangeList = PopGenomes;
            if (!strangeList.Any()) strangeList = BestGenomes;

            var parent2Gen = Random.NextElement(strangeList);
            //parent2Gen is  some strange from general pop

            int wiggle = 9;
            int parent1Snip = (-wiggle + Random.Next(wiggle*2)) + (parent1Gen.Genes.Count / 2);
            int parent2Snip = (-wiggle + Random.Next(wiggle*2)) + (parent2Gen.Genes.Count / 2);


            var parent1Prefix = parent1Gen.Genes.Take(parent1Snip);
            var parent1Suffix = parent1Gen.Genes.Skip(parent1Snip);
            var parent2Prefix = parent2Gen.Genes.Take(parent2Snip);
            var parent2Suffix = parent2Gen.Genes.Skip(parent2Snip);


            var offspring1Genes = parent1Prefix.Concat(parent2Suffix).ToList();
            var offspring2Genes = parent2Prefix.Concat(parent1Suffix).ToList();

            if (Random.Next(100) == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    var name =  offspring1Genes.Count() +  -i/2 + Random.Next(i);
                    var value = Random.NextDouble();
                    offspring1Genes.Add(new Gene<double> { Name = name, Value = value });
                    offspring2Genes.Add(new Gene<double> { Name = name, Value = value });

                }
            }


            GenomeReader gR = new GenomeReader();
            foreach (var genes in new[] { offspring1Genes, offspring2Genes })
            {
                var genome = new BodyGenome(genes);
                var body = gR.ProduceBody(GenomeToPheno(genome));
                RegisterBodyGenome(genome, body);
            }

        }

        int MaxPop = 1000;
        int NumBest = 50;
        List<Body> BestBodies = new List<Body>();
        List<BodyGenome> BestGenomes = new List<BodyGenome>();

        List<BodyGenome> PopGenomes = new List<BodyGenome>();

        private void RegisterBodyGenome(BodyGenome genome, Body body)
        {
            int numParts = body.Parts.Count();
            bool foundFit = false;
            for (int i = 0; i < BestGenomes.Count(); i++)
            {
                if (BestBodies[i].Parts.Count() <= numParts)
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

        }

        private Body GetBestHitter()
        {
            return BestBodies.First();
        }

        BodyGenome RandomGenome(int length)
        {
            var gContents = new List<Gene<double>>();

            for (int i = 0; i < length; i++)
            {
                var v = Random.NextDouble();
                gContents.Add(
                        new Gene<double> { Name = i, Value = v }
                    );
            };
            return new BodyGenome(gContents);
            
        }

        private IBodyPhenotype CreateRandomRawGenome(int numObjects  = 20)
        {
            var g = RandomGenome(9 * numObjects);
            return GenomeToPheno(g);
        }

        private IBodyPhenotype GenomeToPheno(BodyGenome g)
        {
            var t = new RandomDoubleGenomeTemplate(Random);

            var p = new BodyPartGeneParser();
            var headers = p.ReadBodyPartHeaders(g, t);


            BodyPhenotype bodyP = new BodyPhenotype();
            headers.ForEach(header =>
            {

                var partOne = new BodyPartPhenotype();
                partOne.Color = header.Color;
                partOne.BodyPartGeometryIndex = header.GeomIndex;
                partOne.AnchorPart = new InstancePointer(header.AnchorInstance);
                partOne.PlacementPartSocket = new InstancePointer(header.PlacementSocket);
                partOne.Scale = header.Scale;

                bodyP.BodyPartPhenos.Add(partOne);

            });
            return bodyP;
        }

        
        private IBodyPhenotype CreateHandCraftedPhenotype()
        {
            var g = new BodyPhenotype();

            var chanSig0 = new ChanneledSignalGenome();
            chanSig0.InstanceId = 0;


            var chanSig1 = new ChanneledSignalGenome();
            chanSig1.InstanceId = 1;


            var chanSig2 = new ChanneledSignalGenome();
            chanSig2.InstanceId = 2;


            var partOne = new BodyPartPhenotype();
            partOne.ChanneledSignalGenome = chanSig0;
            partOne.Color = Color.Green;

            g.BodyPartPhenos.Add(partOne);


            var nog = new NeuralOrganGenome();
            nog.NeuralNetworkGenome = new NeuralNetworkGenome { NumInputs = 4, NumHidden = 1, NumOutputs = 4 };
            nog.InputGenome = new NeuralInputSocketGenome { Channel = 0, ChanneledSignalGenome = chanSig1 };
            nog.OutputGenome = new NeuralOutputSocketGenome { Channel = 0, ChanneledSignalGenome = chanSig0 };
            var tNet = new NeuralNetwork(nog.NeuralNetworkGenome.NumInputs, nog.NeuralNetworkGenome.NumHidden, nog.NeuralNetworkGenome.NumOutputs);
            tNet.RandomizeWeights(Random);
            nog.NeuralNetworkGenome.Weights = tNet.GetWeights();

            partOne.OrganGenomes.Add(nog);
            partOne.Scale = new Vector3(3f, 3f,  3f);

            partOne.BodyPartGeometryIndex = 0;
            for (int i = 0; i < 6; i++)
            {
                var partTwo = new BodyPartPhenotype();
                partTwo.PlacementPartSocket = new InstancePointer(i);
                partTwo.AnchorPart = new InstancePointer(0);
                g.BodyPartPhenos.Add(partTwo);
                partTwo.ChanneledSignalGenome = chanSig0;
                partTwo.Color = Color.Blue;

                nog = new NeuralOrganGenome();
                nog.NeuralNetworkGenome = new NeuralNetworkGenome { NumInputs = 1, NumHidden = 1, NumOutputs = 1 };
                nog.InputGenome = new NeuralInputSocketGenome { Channel = 0, ChanneledSignalGenome = chanSig2 };
                nog.OutputGenome = new NeuralOutputSocketGenome { Channel = 0, ChanneledSignalGenome = chanSig0 };

                 tNet = new NeuralNetwork(nog.NeuralNetworkGenome.NumInputs, nog.NeuralNetworkGenome.NumHidden, nog.NeuralNetworkGenome.NumOutputs);
                tNet.RandomizeWeights(Random);
                nog.NeuralNetworkGenome.Weights = tNet.GetWeights();

                 partTwo.OrganGenomes.Add(nog);
                partTwo.Scale = new Vector3(0.8f, .2f, 0.8f);

                partTwo.BodyPartGeometryIndex = 1;
            }

            for (int i = 0; i < 6; i++)
            {
                var partTwo = new BodyPartPhenotype();
                partTwo.PlacementPartSocket = new InstancePointer(1);
                partTwo.AnchorPart = new InstancePointer(1 + i);
                g.BodyPartPhenos.Add(partTwo);
                partTwo.ChanneledSignalGenome = chanSig1;
                partTwo.Color = Color.Yellow;

                nog = new NeuralOrganGenome();
                nog.NeuralNetworkGenome = new NeuralNetworkGenome { NumInputs = 1, NumHidden = 1, NumOutputs = 1 };
                nog.InputGenome = new NeuralInputSocketGenome { Channel = 0, ChanneledSignalGenome = chanSig0 };
                nog.OutputGenome = new NeuralOutputSocketGenome { Channel = 0, ChanneledSignalGenome = chanSig1 };

                 tNet = new NeuralNetwork(nog.NeuralNetworkGenome.NumInputs, nog.NeuralNetworkGenome.NumHidden, nog.NeuralNetworkGenome.NumOutputs);
                tNet.RandomizeWeights(Random);
                nog.NeuralNetworkGenome.Weights = tNet.GetWeights();

                partTwo.OrganGenomes.Add(nog);

                partTwo.BodyPartGeometryIndex = 6;
            }


            for (int i = 0; i < 6; i++)
            {

                for (int j = 0; j < 7; j++)
                {
                    var partTwo = new BodyPartPhenotype();
                    partTwo.PlacementPartSocket = new InstancePointer(1 + j);
                    partTwo.AnchorPart = new InstancePointer(1 + 6 + i);
                    g.BodyPartPhenos.Add(partTwo);
                    partTwo.ChanneledSignalGenome = chanSig2;
                    partTwo.Color = Color.WhiteSmoke;

                    nog = new NeuralOrganGenome();
                    nog.NeuralNetworkGenome = new NeuralNetworkGenome { NumInputs = 1, NumHidden = 1, NumOutputs = 1 };
                    nog.InputGenome = new NeuralInputSocketGenome { Channel = 0, ChanneledSignalGenome = chanSig1 };
                    nog.OutputGenome = new NeuralOutputSocketGenome { Channel = 0, ChanneledSignalGenome = chanSig2 };
                     tNet = new NeuralNetwork(nog.NeuralNetworkGenome.NumInputs, nog.NeuralNetworkGenome.NumHidden, nog.NeuralNetworkGenome.NumOutputs);
                    tNet.RandomizeWeights(Random);
                    nog.NeuralNetworkGenome.Weights = tNet.GetWeights();

                    partTwo.OrganGenomes.Add(nog);
                    partTwo.Scale = new Vector3(0.8f, .2f, 0.8f);

                    partTwo.BodyPartGeometryIndex = 6;
                }
            }



            return g;
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

            GenerateRandomPopulation(MaxPop + NumBest);
            Body = GetBestHitter();

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
