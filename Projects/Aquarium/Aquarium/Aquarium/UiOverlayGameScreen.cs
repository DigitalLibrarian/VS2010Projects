using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.Screens;
using Aquarium.Ui;
using Microsoft.Xna.Framework;
using Forever.Render;
using Forever.Render.Cameras;

namespace Aquarium
{
    abstract class UiOverlayGameScreen : GameScreen
    {
        protected UiOverlay Ui { get; set; }
        protected RenderContext RenderContext { get; set; }

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
        }
        
        public override void HandleInput(InputState input)
        {
            Ui.HandleInput(input);

            base.HandleInput(input);
        }

        public override void Draw(GameTime gameTime)
        {
            Ui.Draw(gameTime);

            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            Ui.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }
    }
}
