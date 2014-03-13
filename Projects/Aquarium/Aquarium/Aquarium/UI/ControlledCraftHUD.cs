using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Forever.Render;

namespace Aquarium.UI
{
    
    public class ControlledCraftHUD : IUIElement
    {

        public Reticule EmptyCircleReticule { get; private set; }
        public Reticule FilledCircleReticule { get; private set; }

        public Reticule CurrentReticule { get; private set; }
        public CursorReticule Cursor { get; private set; }

        public ControlledCraft Craft { get; private set; }

        public RenderContext RenderContext { get; private set; }

        public ControlledCraftHUD(ControlledCraft craft, RenderContext renderContext)
        {
            Craft = craft;
            RenderContext = renderContext;
        }



        public void LoadContent(ContentManager content, GraphicsDevice graphics)
        {
            var emptyCircle = content.Load<Texture2D>("Reticules/SimpleCircleReticule");
            EmptyCircleReticule = new Reticule(emptyCircle, 0, 200, 200, 35, 35);

            var filledCircle = content.Load<Texture2D>("Reticules/SimpleConcentricReticule");
            FilledCircleReticule = new Reticule(filledCircle, 0, 200, 200, 35, 35);

            Cursor = new CursorReticule(emptyCircle, 0, 200, 200, 30, 30);
            CurrentReticule = FilledCircleReticule;
        }


        public void HandleInput(Forever.Screens.InputState input)
        {
            Cursor.HandleInput(input);
            var mouseSteering = Craft.ControlForces.MouseSteeringEngaged;
            if (mouseSteering)
            {
                CurrentReticule = EmptyCircleReticule;
            }
            else
            {
                CurrentReticule = FilledCircleReticule;
            }
        }

        public void Draw(Microsoft.Xna.Framework.GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch batch)
        {

            var center = RenderContext.GraphicsDevice.Viewport.Bounds.Center;
            CurrentReticule.Draw(batch, center);

            if (Craft.ControlForces.MouseSteeringEngaged)
            {
                Cursor.DrawOnCursor(batch);
            }
        }

        public void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
           // throw new NotImplementedException();
        }
    }


}
