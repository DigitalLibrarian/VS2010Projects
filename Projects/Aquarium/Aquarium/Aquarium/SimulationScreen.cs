using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.Render;
using Forever.Screens;
using Microsoft.Xna.Framework;
using Aquarium.Sim;
using Aquarium.UI;
using Aquarium.Sim.Agents;
using System.Threading;
using Forever.Physics;
using Aquarium.UI.Steering;


namespace Aquarium
{
    class SimulationScreen : UIGameScreen
    {
        Simulation Sim { get; set; }

        ControlledCraft User { get; set; }

        public SimulationScreen(RenderContext renderContext) : base(renderContext)
        {
        }

        public override void HandleInput(InputState input)
        {
            base.HandleInput(input);

            User.HandleInput(input);
        }

        public override void LoadContent()
        {
            base.LoadContent();

            Sim = new Simulation();

            User = CreateControlledCraft();

            UIElements.AddRange(CreateUILayout());

            SpawnerAgentThread = new Thread(new ThreadStart(UpdateSpawnerAgentsThreadFunc));
            SpawnerAgentThread.IsBackground = true;
            SpawnerAgentThread.Start();
        }

        Thread SpawnerAgentThread;

        public override void UnloadContent()
        {
            SpawnerAgentThread.Abort();
            System.Threading.SpinWait.SpinUntil(() =>
            {
                System.Threading.Thread.Sleep(100);
                return !SpawnerAgentThread.IsAlive;
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
            var agent = new SpawnerAgent(pos, principle as IOrganismAgentPool);
            Sim.Space.Register(agent, pos);

            spawners.Add(agent);
        }

        List<SpawnerAgent> spawners = new List<SpawnerAgent>();

        private void UpdateSpawnerAgentsThreadFunc()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(500);
                foreach (var spawner in spawners.ToList())
                {
                    spawner.BackgroundThreadFunc();
                }

            }
        }

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

        List<IUIElement> CreateUILayout()
        {
            var spriteFont = ScreenManager.Font;
            var actionBarSlotHeight = 40;
            var actionBar = new ActionBar(RenderContext, 50, actionBarSlotHeight, spriteFont);
            actionBar.Slots[0].Action = new ActionBarAction(AddNewSpawnerAgent);

            var hud = new ControlledCraftHUD(User, RenderContext);
            hud.LoadContent(ScreenManager.Game.Content, ScreenManager.Game.GraphicsDevice);


            var odometer = new OdometerDashboard(User, ScreenManager.Game.GraphicsDevice, new Vector2(0, -actionBarSlotHeight + -15f), 300, 17);


            return new List<IUIElement>
            {
                actionBar,
                hud, 
                odometer
            };
        }

    }

}
