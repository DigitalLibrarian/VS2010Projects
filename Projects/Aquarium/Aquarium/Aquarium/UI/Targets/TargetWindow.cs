using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Aquarium.Sim.Agents;
using Microsoft.Xna.Framework.Graphics;
using Forever.Render;

namespace Aquarium.UI.Targets
{
    interface ITarget
    {
        string Label { get; }
        IAgent Agent { get; }
        BoundingBox TargetBB { get; }
    }


    class TargetWindow : IUIElement
    {
        RenderContext RenderContext { get; set; }
        Vector2 Offset { get; set; }

        public ITarget Target { get; set; }
        public bool HasTarget { get { return Target != null; } }

        public SpriteFont SpriteFont { get; private set; }
        Func<Ray, ITarget> Source { get; set; }
        public TargetWindow(Func<Ray, ITarget> source, RenderContext renderContext, Vector2 offset, SpriteFont font)
        {
            Source = source;
            RenderContext = renderContext;
            Offset = offset;
            SpriteFont = font;
        }
        
        public void HandleInput(Forever.Screens.InputState input)
        {
            // if mouse click, produce ray and test for new target

            if (input.IsMouseLeftClick())
            {
                var mousePoint = input.CurrentMousePoint.ToVector2();
                //todo - limit to targeting area
                var ray = GetMouseRay(mousePoint);

                Target = Source(ray);
            }
        }



        public Ray GetMouseRay(Vector2 mousePosition)
        {
            var projection = RenderContext.Camera.Projection;
            var view = RenderContext.Camera.View;
            Viewport viewport = RenderContext.GraphicsDevice.Viewport;

            Vector3 near = new Vector3(mousePosition, 0);
            Vector3 far = new Vector3(mousePosition, 1);

            near = viewport.Unproject(near, projection, view, Matrix.Identity);
            far = viewport.Unproject(far, projection, view, Matrix.Identity);

            return new Ray(near, Vector3.Normalize(far - near));
        }


        public void Draw(GameTime gameTime, SpriteBatch batch)
        {
            //for now we are going to draw a string

            string label = HasTarget ? Target.Label : "<no target>";

            var bounds = RenderContext.GraphicsDevice.Viewport.Bounds;
            var position = new Vector2(bounds.Left, bounds.Top);
            position += Offset;

            batch.DrawString(SpriteFont, label, position, Color.Yellow);
            
            if(HasTarget)
            {
                RenderContext.Set3DRenderStates();

                Renderer.Render(RenderContext, Target.TargetBB, Color.Cyan);

                RenderContext.Set2DRenderStates();
            }
        }

        public void Update(GameTime gameTime)
        {
          

        }
    }
}
