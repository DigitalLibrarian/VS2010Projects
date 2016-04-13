using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.Input.Devices;
using Nuclex.Input;
using Microsoft.Xna.Framework;
using Forever.Render;
using Microsoft.Xna.Framework.Graphics;

namespace Aquarium.Targeting
{
    public class TargetManager
    {
        RenderContext RenderContext { get; set; }
        InputManager InputManager { get; set; }

        public ITarget Target { get; private set; }

        public event EventHandler<NewTargetEventArgs> OnNewTarget;
        Func<Ray, ITarget> TargetFinder { get; set; }

        MouseButtonDelegate NewTargetEventHandler { get; set; }

        bool _acceptingInput = false;
        public bool AcceptingInput { get { return _acceptingInput; } }

        public TargetManager(
            RenderContext renderContext,
            InputManager inputManager,
            Func<Ray, ITarget> targetFinder)
        {
            RenderContext = renderContext;
            InputManager = inputManager;
            TargetFinder = targetFinder;
            NewTargetEventHandler = new MouseButtonDelegate(TargetManager_MouseButtonReleased);
        }

        public void RegisterForInput()
        {
            InputManager.GetMouse().MouseButtonReleased += NewTargetEventHandler;
            _acceptingInput = true;
        }
        public void UnRegisterForInput()
        {
            InputManager.GetMouse().MouseButtonReleased -= NewTargetEventHandler;
            _acceptingInput = false;
        }

        
        void TargetManager_MouseButtonReleased(MouseButtons buttons)
        {
            if (buttons == MouseButtons.Left)
            {
                var mouseState = InputManager.GetMouse().GetState();
                var mousePoint = new Vector2(mouseState.X, mouseState.Y);
                //todo - limit to targeting area
                var ray = GetMouseRay(mousePoint);

                Target = TargetFinder(ray);
                Fire_OnNewTarget();
            }
        }

        private void Fire_OnNewTarget()
        {
            if (Target != null)
            {
                if (OnNewTarget != null)
                {
                    OnNewTarget(this, new NewTargetEventArgs { Target = this.Target });
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
    }
}
