using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using Forever.Render;
namespace Aquarium.UI
{
    public class Slot
    {
        public int Index { get; private set; }
        public SlotState State { get; private set; }
        public Keys Key { get; private set; }

        public int TotalCoolDown { get; private set; }
        public int CoolDownLeft { get; private set; }
        public double RatioCooldownLeft
        {
            get
            {
                return (
                     (double)CoolDownLeft / (double)TotalCoolDown
                );
            }
        }


        public IActionBarAction Action { get; set; }
        ActionBar ActionBar { get; set; }

        public Slot(ActionBar bar, Keys key, int index, int totalCoolDown)
        {
            ActionBar = bar;
            Index = index;
            State = SlotState.Inactive;

            Key = key;
            TotalCoolDown = totalCoolDown;
            CoolDownLeft = 0;
        }

        public void Draw(
            Microsoft.Xna.Framework.GameTime gameTime,
            SpriteBatch batch,
            SpriteFont font,
            RenderContext renderContext)
        {
            var rect = GetRectangle(renderContext.GraphicsDevice.Viewport);


            batch.DrawRectangle(rect, Color.White, 1);

            var label = Key.ToString();
            if (Action != null && Action.Label != null)
            {
                label = Action.Label;
            }
            var labelPoint = new Vector2(rect.Left, rect.Top);

            var color = Color.White;
            if (State == SlotState.CoolDown)
                color = Color.Gray;
            batch.DrawString(font, label, labelPoint, color);

            var ratio = this.RatioCooldownLeft;
            label = string.Format("{0}%", (int)(ratio * 100));
            labelPoint = new Vector2(rect.Center.X, rect.Center.Y);
            color = Color.White;
            if (State == SlotState.CoolDown)
                color = Color.Yellow;
            batch.DrawString(font, label, labelPoint, color);
        }

        private Rectangle GetRectangle(Viewport viewport)
        {
            var vW = viewport.Width;
            var vH = viewport.Height;
            
            var offX = ActionBar.SlotWidth * Index;
            var y = vH - ActionBar.SlotHeight;

            var totalBarWidth = ActionBar.SlotWidth * ActionBar.Slots.Count();
            var center = viewport.Bounds.Center.X;
            var toBegin = -totalBarWidth / 2;
            var x = center + toBegin + offX;

            return new Rectangle(x, y, ActionBar.SlotWidth, ActionBar.SlotHeight);
        }

        public void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {

            var milli = gameTime.ElapsedGameTime.Milliseconds;
            switch (State)
            {
                case SlotState.Inactive:
                    SS_Inactive(milli);
                    break;
                case SlotState.Fired:
                    SS_Fired(milli);
                    break;
                case SlotState.CoolDown:
                    SS_Cooling(milli);
                    break;
                default:
                    throw new Exception("Unknown SlotState");
            }
            activation = false;
        }

        private void SS_Inactive(int milli)
        {
            if (activation)
            {
                State = SlotState.Fired;
            }
        }

        private void SS_Fired(int milli)
        {
            if (Action != null) Action.OnFire();

            //start cooldown
            CoolDownLeft = TotalCoolDown;
            State = SlotState.CoolDown;
        }

        private void SS_Cooling(int milli)
        {
            CoolDownLeft -= milli;
            if (CoolDownLeft <= 0)
            {
                CoolDownLeft = 0;
                State = SlotState.Inactive;
            }
        }



        bool activation = false;
        public void OnButtonPress()
        {
            activation = true;
        }

    }
}
