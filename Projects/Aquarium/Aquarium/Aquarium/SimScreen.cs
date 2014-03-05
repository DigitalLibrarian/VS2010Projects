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

namespace Aquarium
{
    public class SimScreen : GameScreen
    {
        public RandomPopulation Pop { get; private set; }

        protected RenderContext RenderContext { get; private set; }
        public IRigidBody PlayerRigidBody { get; private set; }

        private int DrawRadius { get; set; }
        private int UpdateRadius { get; set; }


        public Space<PopulationMember> Coarse { get; private set; }
        public EnvironmentSpace Fine { get; private set; }

        public UserControls Controls { get; private set; }
        public MouseSteering Steering { get; private set; }
        public ActionBar ActionBar { get; private set; }


        Thread GenerateThread;
        int SpawnThreadFreq = 500;
        int MaxPerSpawnPump = 50;
        int DefaultParts = 10;
        int BirthsPerUpdate = 1;
        int MaxBirthQueueSize = 400;
        public SimScreen(RenderContext renderContext)
        {
            RenderContext = renderContext;

            Coarse = new Space<PopulationMember>(500);
            Fine = new EnvironmentSpace(250, 250);

            int minPopSize = 50;
            int maxPopSize = 150;
            int spawnRange = 25;
            int geneCap = 10000;

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

            DrawRadius = 50;
            UpdateRadius = 25;

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

            SetupCamera();
            SetupUI(ScreenManager.Game.Content);

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

        Partition<PopulationMember> CurrentDrawingPartition { get; set; }
        IEnumerable<Partition<PopulationMember>> CurrentDrawingPartitions { get; set; }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            var context = RenderContext;
            var camPos = context.Camera.Position;

            if (CurrentDrawingPartition != null)
            {
                if (CurrentDrawingPartition.Box.Contains(camPos) != ContainmentType.Contains)
                {

                    CurrentDrawingPartition = Coarse.GetOrCreate(camPos);
                    CurrentDrawingPartitions = Coarse.GetSpacePartitions(camPos, Coarse.GridSize * DrawRadius);
                }
            }
            else
            {
                CurrentDrawingPartition = Coarse.GetOrCreate(camPos);
                CurrentDrawingPartitions = Coarse.GetSpacePartitions(camPos, Coarse.GridSize * DrawRadius);
            }

            foreach (var part in CurrentDrawingPartitions)
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

            DrawUI(gameTime);
        }



        public void Death(IEnumerable<PopulationMember> members)
        {
            foreach (var mem in members)
            {
                Pop.UnRegister(mem);
            }
        }




        Partition<IEnvMember> CurrentUpdatingPartition { get; set; }
        IEnumerable<Partition<IEnvMember>> CurrentUpdatingPartitions { get; set; }
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            UpdatePlayerRigidBody(gameTime);
            UpdateCamera(gameTime);
          
            float duration = (float)gameTime.ElapsedGameTime.Milliseconds;

            var camPos = RenderContext.Camera.Position;

            if (CurrentUpdatingPartition != null)
            {
                if (CurrentUpdatingPartition.Box.Contains(camPos) != ContainmentType.Contains)
                {
                    CurrentUpdatingPartition = Fine.GetOrCreate(camPos);
                    CurrentUpdatingPartitions = Fine.GetSpacePartitions(camPos, Fine.GridSize * UpdateRadius);
                }
            }
            else
            {
                CurrentUpdatingPartition = Fine.GetOrCreate(camPos);
                CurrentUpdatingPartitions = Fine.GetSpacePartitions(camPos, Fine.GridSize * UpdateRadius);
            }

            var dead = new List<PopulationMember>();

            foreach (var part in CurrentUpdatingPartitions)
            {
                var members = part.Objects.ToList();

                foreach (var envMember in members)
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
                // new life to the living
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
            PlayerRigidBody.LinearDamping = 0.9f;
            PlayerRigidBody.AngularDamping = 0.7f;
            PlayerRigidBody.Mass = 0.1f;
            PlayerRigidBody.InertiaTensor = InertiaTensorFactory.Sphere(PlayerRigidBody.Mass, 1f);
            Controls = new UserControls(PlayerIndex.One, 0.000015f, 0.0025f, 0.0003f, 0.001f);
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
            Vector3 actuatorTrans = Controls.LocalForce;
            Vector3 actuatorRot = Controls.LocalTorque;

            float forwardForceMag = -actuatorTrans.Z;
            float rightForceMag = actuatorTrans.X;
            float upForceMag = actuatorTrans.Y;

            Vector3 force =
                (Camera.Forward * forwardForceMag) +
                (Camera.Right * rightForceMag) +
                (Camera.Up * upForceMag);

            PlayerRigidBody.addForce(force);

            Vector3 worldTorque = Vector3.Transform(Controls.LocalTorque, PlayerRigidBody.Orientation);

            if (worldTorque.Length() != 0)
            {
                PlayerRigidBody.addTorque(worldTorque);
            }

            var steeringTorque = GetMouseSteeringTorque(gameTime, PlayerRigidBody);
            PlayerRigidBody.addTorque(steeringTorque);

            PlayerRigidBody.integrate((float)gameTime.ElapsedGameTime.Milliseconds);
        }

        #endregion


        #region UI

        public Reticule CurrentReticule { get; private set; }
        public CursorReticule Cursor { get; private set; }

        public Reticule EmptyCircleReticule { get; private set; }
        public Reticule FilledCircleReticule { get; private set; }

        private void SetupUI(ContentManager content)
        {
            //TODO - pick better font
            var spriteFont = ScreenManager.Font;
            ActionBar = new ActionBar(RenderContext, new Vector2(0, 440), 50, 40, spriteFont);

            var emptyCircle = content.Load<Texture2D>("Reticules/SimpleCircleReticule");
            EmptyCircleReticule = new Reticule(emptyCircle, 0, 200, 200, 35, 35);

            var filledCircle = content.Load<Texture2D>("Reticules/SimpleConcentricReticule");
            FilledCircleReticule = new Reticule(filledCircle, 0, 200, 200, 35, 35);
            
            Cursor = new CursorReticule(emptyCircle, 0, 200, 200, 30, 30);
            Steering = new MouseSteering();

            CurrentReticule = FilledCircleReticule;
        }


        public override void HandleInput(InputState input)
        {
            base.HandleInput(input);
            Controls.HandleInput(input);
            ActionBar.HandleInput(input);
            Cursor.HandleInput(input);
            Steering.HandleInput(input);
            if (Controls.ControlSchemeToggle)
            {
                Steering.Engaged = !Steering.Engaged;
                if (Steering.Engaged)
                {
                    CurrentReticule = EmptyCircleReticule;
                }
                else
                {
                    CurrentReticule = FilledCircleReticule;
                }
                this.ScreenManager.Game.IsMouseVisible = !Steering.Engaged;
            }
        }


        private Vector3 GetMouseSteeringTorque(GameTime gameTime, IRigidBody body)
        {
            var cp = RenderContext.GraphicsDevice.Viewport.Bounds.Center;

            return Steering.GetTorqueForBodyAndAnchor(gameTime, body, cp);
        }


        public void DrawUI(GameTime gameTime)
        {
            RenderContext.Set2DRenderStates();

            var batch = ScreenManager.SpriteBatch;
            var center = RenderContext.GraphicsDevice.Viewport.Bounds.Center;
            batch.Begin();
            CurrentReticule.Draw(batch, center);

            if (Steering.Engaged)
            {
                Cursor.DrawOnCursor(batch);
            }

            ActionBar.Draw(gameTime, batch);
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
            
            Action<BodyGenome, Vector3> birther = (BodyGenome off, Vector3 pos) =>
            {
                Pop.Splicer.Mutate(off);

                var spawn = Population.SpawnFromGenome(off);
                if (spawn != null)
                {
                    spawn.Position = pos;

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
                var center = (p1.Position * 0.5f) + (p2.Position * 0.5f);
                
                birther(offspring1[0], center + random.NextVector() * spawnRange);
                birther(offspring1[1], center + random.NextVector() * spawnRange);
                numQueued += 2;
                total += 2;
                if (a1 > a2)
                {
                    p2 = random.NextElement(members);
                }
                else if (a1 < a2) ;
                {
                    p1 = random.NextElement(members);
                }
                var offspring2 = Pop.Splicer.Meiosis(p1.Genome, p2.Genome);
                center = (p1.Position * 0.5f) + (p2.Position * 0.5f);
                birther(offspring2[0], center + random.NextVector() * spawnRange);
                birther(offspring2[1], center + random.NextVector() * spawnRange);
                numQueued += 2;
                total += 2;
            }

        }

        #endregion
    }
}
