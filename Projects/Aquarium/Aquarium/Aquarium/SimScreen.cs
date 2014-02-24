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

namespace Aquarium
{


    public class SimScreen : GameScreen
    {
        public RandomPopulation Pop { get; private set; }

        protected RenderContext RenderContext { get; private set; }
        public IRigidBody CamBody { get; private set; }
        public UserControls CamControls { get; private set; }

        private int DrawRadius { get; set; }
        private int UpdateRadius { get; set; }


        public Space<PopulationMember> Coarse { get; private set; }
        public EnvironmentSpace Fine { get; private set; }


        Thread GenerateThread;

        public SimScreen(RenderContext renderContext)
        {

            RenderContext = renderContext;

            Coarse = new Space<PopulationMember>(500);
            Fine = new EnvironmentSpace(250, 250);

            int minPopSize = 150;
            int maxPopSize = 500;
            int spawnRange = 100;
            int geneCap = 2000;

            var rPop = new RandomPopulation(minPopSize, maxPopSize, spawnRange, geneCap);
            rPop.OnAdd += new Population.OnAddEventHandler((mem) =>
            {
                Coarse.Register(mem, mem.Position);
                Fine.Register(mem as IEnvMember, mem.Position);
            });

            rPop.OnRemove += new Population.OnRemoveEventHandler((mem) =>
            {

                Coarse.UnRegister(mem);
                Fine.UnRegister(mem as  IEnvMember);
            });

            rPop.GenerateUntilSize(minPopSize / 2, rPop.SpawnRange, 3);
            rPop.GenerateUntilSize(maxPopSize, rPop.SpawnRange * 2, 10);

            Pop = rPop;
            DrawRadius = 5;
            UpdateRadius = 50;


            GenerateThread = new Thread(new ThreadStart(
                () => {
                    while (true)
                    {
                        UpdatePopMonitoring();
                        System.Threading.Thread.Sleep(500);
                    }
                    
                }
                    ));
        }


        public override void LoadContent()
        {
            base.LoadContent();

            SetupCamera();


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
        IEnumerable<Partition<IEnvMember>> CurrentUpdatingPartitions { get; set; }

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
                    CurrentUpdatingPartitions = Fine.GetSpacePartitions(camPos, Fine.GridSize * UpdateRadius);
                }
            }
            else
            {
                CurrentDrawingPartition = Coarse.GetOrCreate(camPos);
                CurrentDrawingPartitions = Coarse.GetSpacePartitions(camPos, Coarse.GridSize * DrawRadius);
                CurrentUpdatingPartitions = Fine.GetSpacePartitions(camPos, Fine.GridSize * UpdateRadius);
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
 
        }

        public void Death(IEnumerable<PopulationMember> members)
        {
            foreach (var mem in members)
            {
                Pop.UnRegister(mem);
            }
        }

       

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
           // if (!otherScreenHasFocus && !coveredByOtherScreen)
            {
                UpdateCamera(gameTime);
            }

           // if (!otherScreenHasFocus)
            {
                float duration = (float)gameTime.ElapsedGameTime.Milliseconds;

                var camPos = RenderContext.Camera.Position;
                if (CurrentUpdatingPartitions == null)
                {
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
                                Fine.Update(member, member.Position);
                            }
                        }
                    }
                }


                Death(dead); //death to the dead

                
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        #region Camera Controls

        protected ICamera Camera { get { return RenderContext.Camera; } }
        private void SetupCamera()
        {
            var cam = Camera;
            CamBody = new RigidBody(cam.Position);
            CamBody.Awake = true;
            CamBody.LinearDamping = 0.9f;
            CamBody.AngularDamping = 0.7f;
            CamBody.Mass = 0.1f;
            CamBody.InertiaTensor = InertiaTensorFactory.Sphere(CamBody.Mass, 1f);
            CamControls = new UserControls(PlayerIndex.One, 0.000015f, 0.0025f, 0.0003f, 0.001f);
        }

        protected void UpdateCamera(GameTime gameTime)
        {
            Vector3 actuatorTrans = CamControls.LocalForce;
            Vector3 actuatorRot = CamControls.LocalTorque;


            float forwardForceMag = -actuatorTrans.Z;
            float rightForceMag = actuatorTrans.X;
            float upForceMag = actuatorTrans.Y;

            Vector3 force =
                (Camera.Forward * forwardForceMag) +
                (Camera.Right * rightForceMag) +
                (Camera.Up * upForceMag);


            if (force.Length() != 0)
            {
                CamBody.addForce(force);
            }


            Vector3 worldTorque = Vector3.Transform(CamControls.LocalTorque, CamBody.Orientation);

            if (worldTorque.Length() != 0)
            {
                CamBody.addTorque(worldTorque);
            }
            
            CamBody.integrate((float)gameTime.ElapsedGameTime.Milliseconds);
            Camera.Position = CamBody.Position;
            Camera.Rotation = CamBody.Orientation;
            
        }


        public override void HandleInput(InputState input)
        {
            base.HandleInput(input);
            CamControls.HandleInput(input);
        }
        #endregion


        #region Population Monitoring
        protected void UpdatePopMonitoring()
        {
            if (!Pop.NeedsSpawn) return;

            var random = new Random();
            var members = Pop.ToList();

            var spawnRange = 1000;

            if (members.Count < Pop.MinPop)
            {
                Pop.GenerateUntilSize(Pop.MinPop, spawnRange, numPartsEach: 2);
            }
            while (Pop.Size < Pop.MaxPop)
            {
                var p1 = random.NextElement(members);
                var p2 = random.NextElement(members);

                var a1 = p1.Specimen.Age;
                var a2 = p2.Specimen.Age;


                var offspring = Pop.Splicer.Meiosis(p1.Genome, p2.Genome);

                if (a1 > a2)
                {
                    p2 = random.NextElement(members);
                }
                else if (a1 < a2) ;
                {
                    p1 = random.NextElement(members);
                }
                offspring.Concat(Pop.Splicer.Meiosis(p1.Genome, p2.Genome));


                foreach (var off in offspring)
                {
                    Pop.Splicer.Mutate(off);

                    var spawn = Population.SpawnFromGenome(off);
                    if (spawn != null)
                    {

                        spawn.Position = random.NextVector() * spawnRange;

                        var mem = new PopulationMember(off, spawn);
                        Pop.Register(mem);
                    }
                }
            }
        }
        #endregion
    }
}
