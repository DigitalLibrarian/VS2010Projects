using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.Screens;
using Microsoft.Xna.Framework;
using Ruminate.GUI.Framework;

namespace Aquarium.Ui
{
    public abstract class RuminateGuiGameScreen : GameScreen
    {
        public Gui Gui { get; private set; }

        protected abstract Gui CreateGui();

        public RuminateGuiGameScreen()
        {
            this.PropagateInput = true;
            this.PropagateDraw = true;
            this.TransitionOffTime = new TimeSpan(0, 0, 0, 0, 50);
        }


        public override void LoadContent()
        {
            base.LoadContent();
            if (Gui == null)
            {
                try
                {
                    Gui = CreateGui();
                }
                catch (System.Reflection.ReflectionTypeLoadException re)
                {
                    throw re;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Gui.Draw();
            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            Gui.Update();
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }
    }
}
