using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.Xna.Framework;
using Forever.Screens;
using Aquarium.GA.Population;
using Forever.Render;
using Forever.Physics;
using Aquarium.GA.SpacePartitions;
using Aquarium.GA.Environments;
using Aquarium.GA.Genomes;
using System.Collections.Concurrent;
using Microsoft.Xna.Framework.Graphics;
using Aquarium.UI;
using Microsoft.Xna.Framework.Content;
using Forever.SpacePartitions;
using Aquarium.UI.Steering;

namespace Aquarium
{
    public class Sim_V1_Screen : GameScreen
    {
        public RandomPopulation Pop { get; private set; }

        protected RenderContext RenderContext { get; private set; }
        public IRigidBody PlayerRigidBody { get; private set; }

        private int DrawRadius { get; set; }
        private int UpdateRadius { get; set; }

        public Space<PopulationMember> Coarse { get; private set; }
        public EnvironmentSpace Fine { get; private set; }

        public ActionBar ActionBar { get; private set; }

        Model HunterModel { get; set; }

        PartitionSphere<Hunter> Hunters { get; set; }
        PartitionSphere<PopulationMember> DrawSet { get; set; }
        PartitionSphere<IEnvMember> UpdateSet { get; set; }

        ControlledCraft User { get; set; }

        OdometerDashboard Odometer { get; set; }

        Thread GenerateThread;
        int SpawnThreadFreq = 500;
        int MaxPerSpawnPump = 50;
        int DefaultParts = 10;
        int BirthsPerUpdate = 1;
        int MaxBirthQueueSize = 200;
        int minPopSize = 50;
        int maxPopSize = 100;
        int spawnRange = 25;
        int geneCap = 10000;
        public Sim_V1_Screen(RenderContext renderContext)
        {
            RenderContext = renderContext;

            Coarse = new Space<PopulationMember>(500);
            Fine = new EnvironmentSpace(250, 250);

            DrawRadius = 5000;
            UpdateRadius = 2500;

            //DrawSet = new PartitionSphere<PopulationMember>(Coarse);
            //UpdateSet = new PartitionSphere<IEnvMember>(Fine);


            Pop = new RandomPopulation(minPopSize, maxPopSize, spawnRange, geneCap);
            Pop.OnAdd += new Population.OnAddEventHandler((mem) =>
            {
                Coarse.Register(mem, mem.Position);
                Fine.Register(mem as IEnvMember, mem.Position);
            });

            Pop.OnRemove += new Population.OnRemoveEventHandler((mem) =>
            {
                Coarse.UnRegister(mem);
                Fine.UnRegister(mem as IEnvMember);
            });

            
            var huntSpace = new Space<Hunter>(Fine.GridSize);
            //Hunters = new PartitionSphere<Hunter>(huntSpace);

            GenerateThread = new Thread(new ThreadStart(
                () => {
                    while (true)
                    {
                        UpdatePopMonitoring();
                        System.Threading.Thread.Sleep(SpawnThreadFreq);
                    }
                    
                }
                    ));

            while (Births.Count() < Pop.MinPop) UpdatePopMonitoring();
            
        }


        public override void LoadContent()
        {
            base.LoadContent();
            var content = ScreenManager.Game.Content;
            HunterModel = content.Load<Model>(Hunter.ModelAsset);

            SetupCamera();
            SetupUI(content);

            GenerateThread.IsBackground = true;
            GenerateThread.Start();
        }

        public override void UnloadContent()
        {
            GenerateThread.Abort();
            System.Threading.SpinWait.SpinUntil(() => 
                {
                    System.Threading.Thread.Sleep(100);
                    return !GenerateThread.IsAlive;
                }
                );
            base.UnloadContent();

        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            var context = RenderContext;
            var camPos = context.Camera.Position;
            var drawSphere = new BoundingSphere(camPos, DrawRadius);
            DrawSet.Sphere = drawSphere;
            Hunters.Sphere = drawSphere;
            foreach (var part in DrawSet.GetPartitions())
            {
                var members = part.Objects.ToList();
                foreach (var member in members)
                {
                    member.Specimen.Body.Render(RenderContext);
                }

                if (part.Box.Contains(camPos) != ContainmentType.Disjoint || part.Objects.Any())
                {
                    Renderer.Render(context, part.Box, Color.Red);
                }
            }

            foreach (var hunter in Hunters)
            {
                hunter.Draw(RenderContext);
            }


            DrawUI(gameTime);
        }



        public void Death(IEnumerable<PopulationMember> members)
        {
            foreach (var mem in members)
            {
                Pop.UnRegister(mem);
            }
        }

        const int RefetchFrequency = 10000;
        int timeLeft = 0;
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            timeLeft -= gameTime.ElapsedGameTime.Milliseconds;
            if (timeLeft <= 0)
            {
                DrawSet.Refetch();
                UpdateSet.Refetch();
                timeLeft = RefetchFrequency;
            }

            UpdatePlayerRigidBody(gameTime);
            UpdateCamera(gameTime);
          
            float duration = (float)gameTime.ElapsedGameTime.Milliseconds;

            var camPos = RenderContext.Camera.Position;

            UpdateSet.Sphere = new BoundingSphere(camPos, UpdateRadius);

            var dead = new List<PopulationMember>();

            foreach (var envMember in UpdateSet)
            {
                var member = envMember.Member;
                member.Specimen.Update(duration);
                if (member.Specimen.IsDead)
                {
                    dead.Add(member);
                }
                else
                {
                    var rigidBody = member.Specimen.RigidBody;
                       
                    if (rigidBody.Velocity.LengthSquared() > 0 && rigidBody.Awake)
                    {
                        Coarse.Update(member, member.Position);
                        Fine.Update(member, member.Position);
                    }
                }
            }

            foreach (var hunter in Hunters)
            {
                hunter.Update(duration);
            }

            Death(dead); 

            UpdateBirths();

            UpdateUI(gameTime);

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        
        ConcurrentQueue<PopulationMember> Births = new ConcurrentQueue<PopulationMember>();

        private void UpdateBirths()
        {
            if (Pop.Size > Pop.MaxPop) return;
            int totalBirths = 0;

            PopulationMember localValue;
            while (totalBirths < BirthsPerUpdate && Births.TryPeek(out localValue))
            {
                Births.TryDequeue(out localValue);
                
                if (localValue != null)
                {
                    if (Pop.Register(localValue))
                    {
                        totalBirths++;
                    }
                }
            }
        }

        #region Camera Controls

            //TODO - break out into class
        protected ICamera Camera { get { return RenderContext.Camera; } }
        private void SetupCamera()
        {
            var cam = Camera;
            PlayerRigidBody = new RigidBody(cam.Position);
            PlayerRigidBody.Awake = true;
            PlayerRigidBody.CanSleep = false;
            PlayerRigidBody.LinearDamping = 0.9f;
            PlayerRigidBody.AngularDamping = 0.7f;
            PlayerRigidBody.Mass = 0.1f;
            PlayerRigidBody.InertiaTensor = InertiaTensorFactory.Sphere(PlayerRigidBody.Mass, 1f);
        }

        /// <summary>
        /// Update the camera to follow the player
        /// </summary>
        /// <param name="gameTime"></param>
        protected void UpdateCamera(GameTime gameTime)
        {
            //TODO - make this a follow cam
            Camera.Position = PlayerRigidBody.Position;
            Camera.Rotation = PlayerRigidBody.Orientation;
        }



        #endregion

        #region Player Rigid Body

        protected void UpdatePlayerRigidBody(GameTime gameTime)
        {
          
            User.Update(gameTime);
        }

        #endregion


        #region UI

        public Reticule CurrentReticule { get; private set; }
        public CursorReticule Cursor { get; private set; }

        public Reticule EmptyCircleReticule { get; private set; }
        public Reticule FilledCircleReticule { get; private set; }

        private void SetupUI(ContentManager content)
        {
            var actionBarSlotHeight = 40;

            //TODO - pick better font
            var spriteFont = ScreenManager.Font;
            ActionBar = new ActionBar(RenderContext, 50, actionBarSlotHeight, spriteFont);
            ActionBar.Slots[0].Action = new ActionBarAction(SpawnHunterOnCamera);

            var emptyCircle = content.Load<Texture2D>("Reticules/SimpleCircleReticule");
            EmptyCircleReticule = new Reticule(emptyCircle, 0, 200, 200, 35, 35);

            var filledCircle = content.Load<Texture2D>("Reticules/SimpleConcentricReticule");
            FilledCircleReticule = new Reticule(filledCircle, 0, 200, 200, 35, 35);
            
            Cursor = new CursorReticule(emptyCircle, 0, 200, 200, 30, 30);
            var mouseSteering = new MouseSteering(RenderContext.GraphicsDevice, PlayerRigidBody, 0.000000001f);
            
            var analogSteering = new AnalogSteering(PlayerIndex.One, 0.000015f, 0.0025f, 0.0003f, 0.001f, PlayerRigidBody);

            var controlForces = new SteeringControls(mouseSteering, analogSteering);
            controlForces.MaxAngular = 0.025f;
            controlForces.MaxLinear = 0.1f;
            

            User = new ControlledCraft(PlayerRigidBody, controlForces);

            Odometer = new OdometerDashboard(User, ScreenManager.Game.GraphicsDevice, new Vector2(0, -actionBarSlotHeight + -15f), 300, 17);

            CurrentReticule = FilledCircleReticule;
        }

        private void SpawnHunterOnCamera()
        {
            var pos = Camera.Position;
            var body = new RigidBody(pos);

            body.Awake = true;
            body.CanSleep = true;
            body.LinearDamping = 0.999f;
            body.AngularDamping = 0.999f;
            body.Mass = 1f;
            body.InertiaTensor = InertiaTensorFactory.Sphere(body.Mass, 1f);
            body.Orientation = PlayerRigidBody.Orientation;

            var hunter = new Hunter(HunterModel, body);
            Hunters.Space.Register(hunter, pos);
            
        }

        public override void HandleInput(InputState input)
        {
            base.HandleInput(input);

            User.HandleInput(input);

            ActionBar.HandleInput(input);
            Cursor.HandleInput(input);
            var mouseSteering = User.ControlForces.MouseSteeringEngaged;
            if (mouseSteering)
            {
                CurrentReticule = EmptyCircleReticule;
            }
            else
            {
                CurrentReticule = FilledCircleReticule;
            }
            this.ScreenManager.Game.IsMouseVisible = !mouseSteering;
        }

     
        public void DrawUI(GameTime gameTime)
        {
            RenderContext.Set2DRenderStates();

            var batch = ScreenManager.SpriteBatch;
            var center = RenderContext.GraphicsDevice.Viewport.Bounds.Center;
            batch.Begin();
            CurrentReticule.Draw(batch, center);

            if (User.ControlForces.MouseSteeringEngaged)
            {
                Cursor.DrawOnCursor(batch);
            }

            ActionBar.Draw(gameTime, batch);

            Odometer.Draw(gameTime, batch, ScreenManager.Font);

            batch.End();

            RenderContext.Set3DRenderStates();
        }

        private void UpdateUI(GameTime gameTime)
        {
            ActionBar.Update(gameTime);
        }

        #endregion


        #region Population Monitoring

        protected void UpdatePopMonitoring()
        {

            var random = new Random();
            var members = Pop.ToList();

            var spawnRange = Pop.SpawnRange;

            if (Births.Count() > MaxBirthQueueSize) return;
            int maxPer = MaxPerSpawnPump;
            
            Action<BodyGenome> birther = (BodyGenome off) =>
            {
                Pop.Splicer.Mutate(off);

                var spawn = Population.SpawnFromGenome(off);
                if (spawn != null)
                {
                    spawn.Position = random.NextVector() * spawnRange;
                    spawn.RigidBody.Orientation = Quaternion.CreateFromYawPitchRoll((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
                    var mem = new PopulationMember(off, spawn);
                    Births.Enqueue(mem);
                }
            };

            int numQueued = Births.Count();
            if (numQueued + Pop.Size < Pop.MinPop)
            {
                var numMinLeft = Pop.MinPop - (numQueued);
                while (numMinLeft > 0)
                {
                    var mem = Pop.RandomMember(DefaultParts);
                    if (mem != null)
                    {
                        mem.Specimen.Position = random.NextVector() * spawnRange;
                        Births.Enqueue(mem);
                        numMinLeft--;
                        numQueued++;
                    }
                }
            }



            int total = 0;
            while(members.Any() && numQueued < MaxBirthQueueSize && total < maxPer)
            {
                var p1 = random.NextElement(members);
                var p2 = random.NextElement(members);

                var a1 = p1.Specimen.Age;
                var a2 = p2.Specimen.Age;


                var offspring1 = Pop.Splicer.Meiosis(p1.Genome, p2.Genome);
                birther(offspring1[0]);
                birther(offspring1[1]);
                numQueued += 2;
                total += 2;
                if (a1 > a2)
                {
                    p2 = random.NextElement(members);
                }
                else if (a1 < a2)
                {
                    p1 = random.NextElement(members);
                }
                var offspring2 = Pop.Splicer.Meiosis(p1.Genome, p2.Genome);
                birther(offspring2[0]);
                birther(offspring2[1]);
                numQueued += 2;
                total += 2;
            }

        }

        Vector3 Halfway(Vector3 v1, Vector3 v2)
        {
            return v1.HalfwayTo(v2);
        }
        #endregion
    }
}
