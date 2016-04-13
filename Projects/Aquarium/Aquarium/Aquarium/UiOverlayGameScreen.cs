using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.Screens;
using Aquarium.Ui;
using Microsoft.Xna.Framework;
using Forever.Render;
using Forever.Render.Cameras;
using Aquarium.Ui.Steering;

namespace Aquarium
{
    abstract class UiOverlayGameScreen : DevScreen
    {
        protected UiOverlay Ui { get; set; }

        public override void LoadContent()
        {
            base.LoadContent();

            Ui = new UiOverlay(
                ScreenManager,
                RenderContext
               );
        }
        
        public override void HandleInput(InputState input)
        {
            base.HandleInput(input);

            Ui.HandleInput(input);
        }

        public override void Draw(GameTime gameTime)
        {
            Ui.Draw(gameTime);

            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (!otherScreenHasFocus && !coveredByOtherScreen)
            {
                Ui.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }
    }
}
