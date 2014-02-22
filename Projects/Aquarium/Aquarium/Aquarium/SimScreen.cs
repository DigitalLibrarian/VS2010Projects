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

namespace Aquarium
{
    public class SimScreen : GameScreen
    {

        public Population Pop { get; private set; }

        protected RenderContext RenderContext { get; private set; }
        public IRigidBody CamBody { get; private set; }
        public UserControls CamControls { get; private set; }

        private int DrawRadius { get; set; }
        private int UpdateRadius { get; set; }


        public Space<PopulationMember> Coarse { get; private set; }
        public Space<PopulationMember> Fine { get; private set; }


        public SimScreen(RenderContext renderContext)
        {
            RenderContext = renderContext;

            Coarse = new Space<PopulationMember>(500);
            Fine = new Space<PopulationMember>(250);

            int popSize = 400;
            int spawnRange = 1000;

            var rPop = new RandomPopulation(popSize, spawnRange, 500);
            rPop.OnAdd += new Population.OnAddEventHandler((mem) =>
            {
                Coarse.Register(mem, mem.Position);
                Fine.Register(mem, mem.Position);
            });

            rPop.GenerateUntilSize(rPop.MaxPop / 2, rPop.SpawnRange, 10);
            rPop.GenerateUntilSize(rPop.MaxPop, rPop.SpawnRange * 2, 10);

            Pop = rPop;
            DrawRadius = 5;
            UpdateRadius = 3;
        }


        public override void LoadContent()
        {
            base.LoadContent();

            SetupCamera();

        }

        Partition<PopulationMember> CurrentDrawingPartition { get; set; }
        IEnumerable<Partition<PopulationMember>> CurrentDrawingPartitions { get; set; }
        IEnumerable<Partition<PopulationMember>> CurrentUpdatingPartitions { get; set; }

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
                foreach (var member in part.Objects)
                {
                    member.Specimen.Body.Render(RenderContext);
                }

                if (part.Box.Contains(camPos) != ContainmentType.Disjoint || part.Objects.Any())
                {
                    Renderer.Render(context, part.Box, Color.Red);
                }
            }

           
             
        }

       

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (!otherScreenHasFocus && !coveredByOtherScreen)
            {
                UpdateCamera(gameTime);
            }

            if (!otherScreenHasFocus)
            {
                float duration = (float)gameTime.ElapsedGameTime.Milliseconds;

                var camPos = RenderContext.Camera.Position;
                if (CurrentUpdatingPartitions == null)
                {
                    CurrentUpdatingPartitions = Fine.GetSpacePartitions(camPos, Fine.GridSize * UpdateRadius);
                }

                foreach (var part in CurrentUpdatingPartitions)
                {
                    var members = part.Objects.ToList();
                    foreach (var member in members)
                    {
                        member.Specimen.Update(duration);
                        var rigidBody = member.Specimen.RigidBody;
                        if (rigidBody.Velocity.LengthSquared() > 0 && rigidBody.Awake)
                        {
                            Fine.Update(member, member.Position);
                        }
                    }
                }

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
    }
}
