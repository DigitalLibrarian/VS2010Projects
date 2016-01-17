using System;
using System.Linq;
using System.Text;

using System.Threading;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Forever.Physics;
using Forever.Render;
using Forever.Screens;

using Aquarium.Agent;
using Aquarium.Sim;
using Aquarium.Ui;
using Aquarium.Ui.Steering;
using Aquarium.Ui.Targets;
using Forever.Render.Cameras;


namespace Aquarium
{
    class SimulationScreen : UiOverlayGameScreen
    {
        Simulation Sim { get; set; }

        ControlledCraft User { get; set; }

        public override void HandleInput(InputState input)
        {
            User.HandleInput(input);
            
            base.HandleInput(input);
        }

        public override void LoadContent()
        {
            base.LoadContent();

            var gd = ScreenManager.GraphicsDevice;
            RenderContext = new RenderContext(
                    new EyeCamera(gd),
                    gd
                );
            Ui = new UiOverlay(
                ScreenManager,
                RenderContext
               );

            Sim = new Simulation();

            User = CreateControlledCraft();

            Ui.Elements.AddRange(CreateUILayout());

            var asset = AssetNames.UHFSatelliteModel;
            SpawnerModel = ScreenManager.Game.Content.Load<Model>(asset);
        }

        Model SpawnerModel { get; set; }

        public override void UnloadContent()
        {

            foreach (var agent in spawners)
            {
                agent.Thread.Abort();
            }

            System.Threading.SpinWait.SpinUntil(() =>
            {
                bool allAlive = true;
                System.Threading.Thread.Sleep(100);
                foreach (var agent in spawners)
                {
                    allAlive &= agent.Thread.IsAlive;
                }
                return !allAlive;
            }
                );

            base.UnloadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            Sim.Draw(gameTime, RenderContext);

            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            User.Update(gameTime);

            RenderContext.Camera.Position = User.Body.Position;
            RenderContext.Camera.Rotation = User.Body.Orientation;

            Sim.Update(gameTime);

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }


        private void AddNewSpawnerAgent()
        {
            var principle = Sim.UpdateSet.Principle;

            var pos = RenderContext.Camera.Position;

            //TODO - need better box.  i'm sure i have somethign to extract from model
            var box = BoundingBox.CreateFromSphere(new BoundingSphere(pos, 5f));

            var agent = new SpawnerAgent(pos, principle as IOrganismAgentGroup, SpawnerModel, box);
            Sim.Space.Register(agent, pos);
            agent.Thread.Start();


            spawners.Add(agent);
        }

        List<SpawnerAgent> spawners = new List<SpawnerAgent>();

        ControlledCraft CreateControlledCraft()
        {
            var cam = this.RenderContext.Camera;
            var PlayerRigidBody = new RigidBody(cam.Position);
            PlayerRigidBody.Awake = true;
            PlayerRigidBody.CanSleep = false;
            PlayerRigidBody.LinearDamping = 0.9f;
            PlayerRigidBody.AngularDamping = 0.7f;
            PlayerRigidBody.Mass = 0.1f;
            PlayerRigidBody.InertiaTensor = InertiaTensorFactory.Sphere(PlayerRigidBody.Mass, 1f);
            var mouseSteering = new MouseSteering(RenderContext.GraphicsDevice, PlayerRigidBody, 0.000000001f);
            var analogSteering = new AnalogSteering(PlayerIndex.One, 0.000015f, 0.0025f, 0.0003f, 0.001f, PlayerRigidBody);

            var controlForces = new SteeringControls(mouseSteering, analogSteering);
            controlForces.MaxAngular = 0.025f;
            controlForces.MaxLinear = 0.1f;


            return new ControlledCraft(PlayerRigidBody, controlForces);
        }

        List<IUiElement> CreateUILayout()
        {
            var spriteFont = ScreenManager.Font;
            var actionBarSlotHeight = 40;
            var actionBar = new ActionBar(RenderContext, 50, actionBarSlotHeight, spriteFont);
            actionBar.Slots[0].Action = new ActionBarAction(AddNewSpawnerAgent);

            var hud = new ControlledCraftHUD(User, RenderContext);
            hud.LoadContent(ScreenManager.Game.Content, ScreenManager.Game.GraphicsDevice);

            var odometer = new OdometerDashboard(User, ScreenManager.Game.GraphicsDevice, new Vector2(0, -actionBarSlotHeight + -15f), 300, 17);


            SpawnerEditor = new SpawnerEditor(ScreenManager.Game, RenderContext);
            var targetWindow = new TargetWindow(
                new Func<Ray, ITarget>((ray) => GetNextTarget(ray)), 
                RenderContext, 
                new Vector2(0, 0), 
                ScreenManager.Font,
                this,
                SpawnerEditor);

            actionBar.Slots[1].Action = new ActionBarAction(() =>
            {
                var target = targetWindow.Target;

                if (target != null && target is OrganismAgent)
                {
                    var orgAgent = target as OrganismAgent;

                    orgAgent.Organism.BeConsumed(orgAgent.Organism.ConsumableEnergy);
                }
            });
            actionBar.Slots[1].TotalCoolDown = 200;
            
            return new List<IUiElement>
            {
                actionBar,
                hud, 
                odometer, 
                targetWindow, 
            };
        }
        //TODO - make spawner editor manage this
        SpawnerEditor SpawnerEditor;
        bool Engaged = true;
        ITarget LastTarget = null;
        private ITarget GetNextTarget(Microsoft.Xna.Framework.Ray ray)
        {
            if (Engaged)
            {
                var space = Sim.UpdateSet.Principle as SimSpacePartition;
                var list = space.FindAll(ray);

                if (list.Any())
                {
                    LastTarget = list.Find(agent => agent is ITarget) as ITarget;
                }
                else
                {
                    LastTarget = null;
                }
            }
            return LastTarget;
        }
    }

}
