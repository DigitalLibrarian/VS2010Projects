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


namespace Aquarium
{
    class SimulationScreen : GameScreen
    {
        RenderContext RenderContext { get; set; }
        Simulation Sim { get; set; }

        public ActionBar ActionBar { get; private set; }
        public SimulationScreen(RenderContext renderContext)
        {
            RenderContext = renderContext;
        }

        public override void LoadContent()
        {
            base.LoadContent();

            Sim = new Simulation();

            var spriteFont = ScreenManager.Font;
            var actionBarSlotHeight = 40;
            ActionBar = new ActionBar(RenderContext, 50, actionBarSlotHeight, spriteFont);
            ActionBar.Slots[0].Action = new ActionBarAction(AddNewSpawnerAgent);
        }

        public override void Draw(GameTime gameTime)
        {
            Sim.Draw(gameTime, RenderContext);

            RenderContext.Set2DRenderStates();
            var batch = ScreenManager.SpriteBatch;
            batch.Begin();
            ActionBar.Draw(gameTime, batch);
            batch.End();
            RenderContext.Set3DRenderStates();
            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            Sim.Update(gameTime);

            ActionBar.Update(gameTime);
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void HandleInput(InputState input)
        {
            base.HandleInput(input);
            //TODO
            ActionBar.HandleInput(input);
        }

        private void AddNewSpawnerAgent()
        {
            var principle = Sim.UpdateSet.Principle;


            //TODO - create a new spawner agent here

        }
    }

}
