using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.Screens;
using Microsoft.Xna.Framework;
using Aquarium.Ui;
using Aquarium.UI;

namespace Aquarium
{
    class FlyAroundGameScreen : UiOverlayGameScreen
    {
        protected ControlledCraft User { get; set; }

        LabelUiElement FPSLabel { get; set; }
        LabelUiElement PositionLabel { get; set; }


        public override void LoadContent()
        {
            base.LoadContent();
            User = CreateControlledCraft();
            User.Body.AngularDamping = 0.67f;
            User.Body.LinearDamping = 0.5f;
            User.ControlForces.Mouse.ThrustIncrement = 0.0000001f;

            Ui.Elements.AddRange(CreateUILayout());
        }



        #region UI
        List<IUiElement> CreateUILayout()
        {
            var spriteFont = ScreenManager.Font;
            var actionBarSlotHeight = 40;
            var hud = new ControlledCraftHUD(User, RenderContext);
            hud.LoadContent(ScreenManager.Game.Content, ScreenManager.Game.GraphicsDevice);

            var odometer = new OdometerDashboard(User, ScreenManager.Game.GraphicsDevice, new Vector2(0, -actionBarSlotHeight + -15f), 300, 17);

            FPSLabel = new LabelUiElement(RenderContext, spriteFont, DebugLabelStrip());
            PositionLabel = new LabelUiElement(RenderContext, spriteFont, DebugLabelStrip());

            return new List<IUiElement>{
                hud, odometer,
                FPSLabel,
                PositionLabel
            };
        }

        Vector2 DebugLabelStripOffset = Vector2.Zero;
        Vector2 DebugLabelStripDelta = Vector2.UnitY * 30;
        protected Vector2 DebugLabelStrip()
        {
            var v = DebugLabelStripOffset;
            DebugLabelStripOffset += DebugLabelStripDelta;
            return v;
        }
        #endregion

        public override void HandleInput(InputState input)
        {
            base.HandleInput(input);
            User.HandleInput(input);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (!otherScreenHasFocus && !coveredByOtherScreen)
            {
                User.Update(gameTime);

                RenderContext.Camera.Position = User.Body.Position;
                RenderContext.Camera.Rotation = User.Body.Orientation;

            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            float fps = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;
            FPSLabel.Label = string.Format("FPS: {0}", (int)fps);
            PositionLabel.Label = string.Format("Pos: {0}", RenderContext.Camera.Position.ToString());

            base.Draw(gameTime);
        }

    }
}
