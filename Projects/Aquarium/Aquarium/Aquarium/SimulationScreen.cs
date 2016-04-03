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

namespace Aquarium
{
    class SimulationScreen : FlyAroundGameScreen
    {
        Simulation Sim { get; set; }

        public override void LoadContent()
        {
            base.LoadContent();

            Sim = new Simulation();

            Ui.Elements.AddRange(CreateUILayout());

            var asset = AssetNames.UHFSatelliteModel;
            SpawnerModel = ScreenManager.Game.Content.Load<Model>(asset);

            var pos = RenderContext.Camera.Position;
            var principle = Sim.Space.GetOrCreate(pos);

            User.Body.Position = principle.Box.GetCenter();
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
            Sim.Update(gameTime);


            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
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

        private void KillOrganism(TargetWindow targetWindow)
        {
            var target = targetWindow.Target;

            if (target != null && target is OrganismAgent)
            {
                var orgAgent = target as OrganismAgent;

                orgAgent.Organism.BeConsumed(orgAgent.Organism.ConsumableEnergy);
            }
        }

        private void LifeForceEditor(TargetWindow targetWindow)
        {
            var target = targetWindow.Target;
        }

        List<SpawnerAgent> spawners = new List<SpawnerAgent>();

        List<IUiElement> CreateUILayout()
        {
            var spriteFont = ScreenManager.Font;
            var actionBarSlotHeight = 40;
            var horizontalActionBar = new ActionBar(RenderContext, 30, actionBarSlotHeight, spriteFont);

            SpawnerEditor = new SpawnerEditor(ScreenManager.Game, RenderContext);
            var targetWindow = new TargetWindow(
                new Func<Ray, ITarget>((ray) => GetNextTarget(ray)), 
                RenderContext, 
                DebugLabelStrip(), 
                ScreenManager.Font,
                this,
                SpawnerEditor);

            horizontalActionBar.Slots[0].Action = new ActionBarAction(() => AddNewSpawnerAgent());
            horizontalActionBar.Slots[1].Action = new ActionBarAction(() => KillOrganism(targetWindow));
            horizontalActionBar.Slots[1].TotalCoolDown = 200;
            
            return new List<IUiElement>
            {
                horizontalActionBar,
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
                //var space = Sim.UpdateSet.Principle as SimSpacePartition;

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
