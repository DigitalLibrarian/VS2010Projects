using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Forever.Physics;
using Forever.Render;
using Forever.Screens;
using Forever.Extensions;

using Aquarium.Agent;
using Aquarium.Targeting;

namespace Aquarium.Ui.Targets
{
    class TargetWindow : IUiElement
    {
        RenderContext RenderContext { get; set; }
        Vector2 Offset { get; set; }

        public ITarget Target { get; set; }
        public bool HasTarget { get { return Target != null; } }

        public SpriteFont SpriteFont { get; private set; }
        Func<Ray, ITarget> TargetFinder { get; set; }

        Func<TargetWindow, bool> ShouldAcceptInput { get; set; }
        public event EventHandler<NewTargetEventArgs> OnNewTarget;

        public TargetWindow(
            Func<Ray, ITarget> targetFinder, 
            Func<TargetWindow, bool> shouldAcceptInput,
            RenderContext renderContext, 
            Vector2 offset, 
            SpriteFont font
            )
        {
            TargetFinder = targetFinder;
            ShouldAcceptInput = shouldAcceptInput;
            RenderContext = renderContext;
            Offset = offset;
            SpriteFont = font;
        }
        
        public void HandleInput(Forever.Screens.InputState input)
        {
            if (!ShouldAcceptInput(this)) return;

            // if mouse click, produce ray and test for new target
            if (input.IsMouseLeftClick())
            {
                var mousePoint = input.CurrentMousePoint.ToVector2();
                //todo - limit to targeting area
                var ray = RenderContext.GetScreenRay(mousePoint);

                Target = TargetFinder(ray);
                Fire_OnNewTarget();
            }
        }

        private void Fire_OnNewTarget()
        {
            if (Target != null)
            {
                if(OnNewTarget != null)
                {
                    OnNewTarget(this, new NewTargetEventArgs { Target = this.Target });
                }
            }
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

        public void Update(GameTime gameTime) { }
    }
}
