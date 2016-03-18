using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Forever.Physics;
using Forever.Render;
using Forever.Screens;

using Aquarium.Agent;

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

        SimulationScreen Screen { get; set;}

        SpawnerEditor SpawnerEditor;

        public TargetWindow(
            Func<Ray, ITarget> targetFinder, RenderContext renderContext, Vector2 offset, SpriteFont font, 
            SimulationScreen screen,
            SpawnerEditor spawnerAgentEditor
            )
        {
            TargetFinder = targetFinder;
            RenderContext = renderContext;
            Offset = offset;
            SpriteFont = font;
            Screen = screen;
            SpawnerEditor = spawnerAgentEditor;

            
        }
        
        public void HandleInput(Forever.Screens.InputState input)
        {
            if (SpawnerEditor.IsOpen || SpawnerEditor.IsExiting) return;

            // if mouse click, produce ray and test for new target
            if (input.IsMouseLeftClick())
            {
                var mousePoint = input.CurrentMousePoint.ToVector2();
                //todo - limit to targeting area
                var ray = GetMouseRay(mousePoint);

                Target = TargetFinder(ray);
                OnNewTarget();
            }
        }

        private void OnNewTarget()
        {
            if (Target != null)
            {
                if (Target is SpawnerAgent)
                {
                    SpawnerEditor.AcquireTarget(Target);
                    LoadNewPopup(SpawnerEditor);
                }
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

        public void Update(GameTime gameTime) { }

        private void LoadNewPopup(GameScreen screen)
        {
            Screen.ScreenManager.AddScreen(screen);
        }
    }
}
