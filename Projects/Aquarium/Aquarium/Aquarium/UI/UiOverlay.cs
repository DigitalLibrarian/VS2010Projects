using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.Render;
using Microsoft.Xna.Framework;
using Forever.Screens;

namespace Aquarium.Ui
{
    public class UiOverlay
    {
        public List<IUiElement> Elements { get; private set; }

        public RenderContext RenderContext { get; private set; }
        public ScreenManager ScreenManager { get; private set; }
        public UiOverlay(ScreenManager screenManager, RenderContext renderContext)
        {
            ScreenManager = screenManager;
            RenderContext = renderContext;
            Elements = new List<IUiElement>();
        }

        private void Visit(Action<IUiElement> action)
        {
            foreach (var ele in Elements)
            {
                action(ele);
            }
        }

        public void Draw(GameTime gameTime)
        {
            RenderContext.Set2DRenderStates();
            var batch = ScreenManager.SpriteBatch;
            batch.Begin();
            Visit((ele) => ele.Draw(gameTime, batch));
            batch.End();
            RenderContext.Set3DRenderStates();

        }

        public void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (!otherScreenHasFocus)
            {
                Visit((ele) => ele.Update(gameTime));
            }
        }


        public void HandleInput(InputState input)
        {
            Visit((ele) => ele.HandleInput(input));
        }

    }
}
