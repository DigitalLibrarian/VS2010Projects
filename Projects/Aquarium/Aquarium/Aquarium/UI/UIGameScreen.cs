using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.Render;
using Microsoft.Xna.Framework;
using Forever.Screens;

namespace Aquarium.UI
{
    public abstract class UIGameScreen : GameScreen
    {
        public List<IUIElement> UIElements { get; private set; }

        public RenderContext RenderContext { get; private set; }
        public UIGameScreen(RenderContext renderContext) : base()
        {
            RenderContext = renderContext;
            UIElements = new List<IUIElement>();
        }

        private void Visit(Action<IUIElement> action)
        {
            foreach (var ele in UIElements)
            {
                action(ele);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            RenderContext.Set2DRenderStates();
            var batch = ScreenManager.SpriteBatch;
            batch.Begin();
            Visit((ele) => ele.Draw(gameTime, batch));
            batch.End();
            RenderContext.Set3DRenderStates();

            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            Visit((ele) => ele.Update(gameTime));

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }


        public override void HandleInput(InputState input)
        {
            base.HandleInput(input);

            Visit((ele) => ele.HandleInput(input));
        }

    }
}
