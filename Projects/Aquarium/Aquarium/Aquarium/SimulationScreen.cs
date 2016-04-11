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

using Forever.Extensions;
using Aquarium.Targeting;
using Aquarium.UI.Controls;
using Nuclex.UserInterface.Controls;


namespace Aquarium
{
    class SimulationScreen : FlyAroundGameScreen
    {
        Simulation Sim { get; set; }

        TargetManager TargetManager { get; set; }
        TargetWindowControl TargetWindowControl { get; set; }
        PopulationWindowControl PopulationWindowControl { get; set; }

        List<Control> ControlsToAdd { get; set; }
        List<Control> ControlsToRemove { get; set; }

        Aquarium.Life.Spec.OrganismSpecParser SpecParser = new Life.Spec.OrganismSpecParser();

        public override void LoadContent()
        {
            PropagateInput = true;
            base.LoadContent();

            Sim = new Simulation();

            ControlsToAdd = new List<Control>();
            ControlsToRemove = new List<Control>();

            TargetManager = new Targeting.TargetManager(RenderContext, InputManager,
                new Func<Ray, ITarget>((ray) => GetNextTarget(ray)));
            TargetManager.OnNewTarget += new EventHandler<NewTargetEventArgs>(TargetManager_OnNewTarget);
            TargetManager.RegisterForInput();

            SetupActionBar();

            var asset = AssetNames.UHFSatelliteModel;
            SpawnerModel = ScreenManager.Game.Content.Load<Model>(asset);

            var pos = RenderContext.Camera.Position;
            var principle = Sim.Space.GetOrCreate(pos);

            User.Body.Position = principle.Box.GetCenter();
            User.ControlForces.Analog.ForceShiftMag = 0.01f;
        }

        void TargetManager_OnNewTarget(object sender, NewTargetEventArgs e)
        {
            targetWindow_OnNewTarget(sender, e);
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

            var pos = RenderContext.Camera.Position;
            var principle = Sim.Space.GetOrCreate(pos);

            Renderer.Render(RenderContext, principle.Box, Color.White);
            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (!otherScreenHasFocus && !coveredByOtherScreen)
            {
                Sim.Update(gameTime);
                if (TargetWindowControl != null && TargetWindowControl.IsOpen)
                {
                    TargetWindowControl.UpdateTarget();
                }
            }


            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            foreach (var c in ControlsToRemove)
            {
                GuiManager.Screen.Desktop.Children.Remove(c);
            }
            ControlsToRemove.Clear();

            foreach (var c in ControlsToAdd)
            {
                GuiManager.Screen.Desktop.Children.Add(c);
            }
            ControlsToAdd.Clear();
        }


        private void AddNewSpawnerAgent()
        {
            var pos = RenderContext.Camera.Position;
            var principle = Sim.Space.GetOrCreate(pos);
            //TODO - need better box.  i'm sure i have somethign to extract from model
            var box = BoundingBox.CreateFromSphere(new BoundingSphere(pos, 5f));

            var agent = new SpawnerAgent(pos, principle as IOrganismAgentGroup, SpawnerModel, box);
            Sim.Space.Register(agent, pos);
            agent.Thread.Start();


            spawners.Add(agent);
        }

        private void KillOrganism()
        {
            var target = this.TargetManager.Target;

            if (target != null && target is OrganismAgent)
            {
                var orgAgent = target as OrganismAgent;

                orgAgent.Organism.BeConsumed(orgAgent.Organism.ConsumableEnergy);
            }
        }


        List<SpawnerAgent> spawners = new List<SpawnerAgent>();
        bool TargetWindowAcquiringTargets = true;
        List<IUiElement> CreateUILayout()
        {
            var spriteFont = ScreenManager.Font;

            SpawnerEditor = new SpawnerEditor(ScreenManager.Game, RenderContext);
            var targetWindow = new TargetWindow(
                new Func<Ray, ITarget>((ray) => GetNextTarget(ray)), 
                new Func<TargetWindow, bool>(tw => TargetWindowAcquiringTargets),
                RenderContext, 
                DebugLabelStrip(), 
                ScreenManager.Font);

            targetWindow.OnNewTarget += new EventHandler<NewTargetEventArgs>(targetWindow_OnNewTarget);

            SetupActionBar();

            return new List<IUiElement>
            {
                targetWindow, 
            };
        }

        void SetupActionBar()
        {
            ActionBar.Slots[0].Action = new ActionBarAction(() => AddNewSpawnerAgent());
            ActionBar.Slots[1].Action = new ActionBarAction(() => KillOrganism());
            ActionBar.Slots[1].TotalCoolDown = 200;
        }

        void targetWindow_OnNewTarget(object sender, NewTargetEventArgs e)
        {
            if (ControlsToRemove.Any()) return;
            if (e.Target is SpawnerAgent)
            {
                if(PopulationWindowControl == null)
                {
                    PopulationWindowControl = new UI.Controls.PopulationWindowControl(0, 45);
                    PopulationWindowControl.OnCloseButtonPress += new EventHandler(PopulationWindow_OnCloseButtonPress);
                    PopulationWindowControl.Bindings.ReadFromModel(PopulationWindowControl, e.Target as SpawnerAgent, SpecParser);
                    ControlsToAdd.Add(PopulationWindowControl);
                }
            }

            if (TargetWindowControl != null && TargetWindowControl.Target == e.Target) return;
            if (TargetWindowControl != null)
            {
                TargetWindowControl_OnCloseButtonPress(this, null);
            }else
            {
                TargetWindowControl = new TargetWindowControl(1310, 10);
                TargetWindowControl.OnCloseButtonPress += new EventHandler(TargetWindowControl_OnCloseButtonPress);
                TargetWindowControl.SetNewTarget(e.Target);
                ControlsToAdd.Add(TargetWindowControl);

                TargetWindowAcquiringTargets = false;
            }
        }

        void TargetWindowControl_OnCloseButtonPress(object sender, EventArgs e)
        {
            ControlsToRemove.Add(TargetWindowControl);
            TargetWindowControl = null;
        }

        void PopulationWindow_OnCloseButtonPress(object sender, EventArgs e)
        {
            ControlsToRemove.Add(PopulationWindowControl);
            PopulationWindowControl = null;
        }

        SpawnerEditor SpawnerEditor;
        bool Engaged = true;
        ITarget LastTarget = null;
        private ITarget GetNextTarget(Microsoft.Xna.Framework.Ray ray)
        {
            if (Engaged)
            {
                var pos = RenderContext.Camera.Position;
                var space = Sim.Space.GetOrCreate(pos) as SimSpacePartition;

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
