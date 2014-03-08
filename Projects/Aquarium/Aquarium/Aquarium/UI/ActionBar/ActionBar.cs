using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Forever.Screens;
using Microsoft.Xna.Framework.Input;
using Forever.Render;

namespace Aquarium.UI
{
    public class ActionBar
    {
        public int SlotWidth { get; private set; }
        public int SlotHeight { get; private set; }

        public List<Slot> Slots { get; private set; }
        public RenderContext RenderContext { get; private set; }

        SpriteFont SpriteFont { get; set; }

        public ActionBar(RenderContext renderContext, int cellWidth, int cellHeight, SpriteFont spriteFont
            )
        {
            RenderContext = renderContext;
            SlotWidth = cellWidth;
            SlotHeight = cellHeight;

            SpriteFont = spriteFont;

            Slots = new List<Slot>();
            InitializeSlots();
        }
        void InitializeSlots()
        {
            var keys = new Keys[] 
            {
                Keys.D1,
                Keys.D2,
                Keys.D3,
                Keys.D4,
                Keys.D5,
                Keys.D6,
                Keys.D7,
                Keys.D8,
                Keys.D9,
                Keys.D0
            };

            int coolDowns = 3000;
            var index = 0;
            foreach (var key in keys)
            {
                var slot = new Slot(this, key, index, coolDowns);
                Slots.Add(slot);
                index++;
            }
        }

        public void Draw(Microsoft.Xna.Framework.GameTime gameTime, SpriteBatch batch)
        {
            var font = this.SpriteFont;

            foreach (var slot in Slots)
            {
                slot.Draw(gameTime, batch, font, RenderContext);
            }

        }

        public void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            foreach (var slot in Slots)
            {
                slot.Update(gameTime);
            }

        }

        public void HandleInput(InputState input)
        {
            foreach (var slot in Slots)
            {
                if (input.IsNewKeyPress(slot.Key))
                {
                    slot.OnButtonPress();
                }
                else if (input.IsMouseLeftClick())
                {
                    var point = new Point(input.CurrentMouseState.X, input.CurrentMouseState.Y);
                    if (slot.LastRectangle.Contains(point))
                    {
                        slot.OnMouseClick();
                    }
                }
            }
        }

        public void RegisterAction(IActionBarAction action, int index)
        {
            var slot = Slots[index];
            slot.Action = action;
            // TODO - keep registry and remove old events
        }
    }

   
    

    
}
