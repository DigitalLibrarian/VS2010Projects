using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Forever.Render;

namespace Aquarium.UI
{
    public interface IOdometerSource
    {
        float Ratio { get; }
    }

    public class OdometerDashboard : IUIElement
    {
        Color MeterColor { get { return Color.Maroon; } }
        Color DashColor { get { return Color.DarkSlateGray; } }
        Color FontColor { get { return Color.LightGreen; } }

        Vector2 Offset { get; set; }

        Vector2 Pin
        {
            get
            {
                var bottomPin = new Vector2(
                    _graphics.Viewport.Bounds.Center.X,
                    _graphics.Viewport.Bounds.Bottom);

                return bottomPin + Offset;
            }
        }
        

        Vector2 LabelOffset { get { return new Vector2(-Width/2, -16); } }
  

        IOdometerSource Source { get; set; }

        GraphicsDevice _graphics;

        int Width;
        int Height;
        public OdometerDashboard(IOdometerSource source, GraphicsDevice g, Vector2 offset, int width, int height)
        {
            Source = source;
            _graphics = g;
            Offset = offset;
            Width = width;
            Height = height;
        }


        public void Draw(GameTime gameTime, SpriteBatch batch)
        {
            var dashboard = GetDashboardRectangle();

            batch.FillRectangle(dashboard, DashColor);
            batch.FillRectangle(GetMeterRectangle(), MeterColor);

        }

        private Microsoft.Xna.Framework.Rectangle GetMeterRectangle()
        {
            var dash = GetDashboardRectangle();

            var c = dash.Center;

            int meterWidth = (int) (Ratio * Width);

            float meterRatio = 0.75f;
            int meterThickness = (int)(Height * meterRatio);

            int theRest = Height - meterThickness;

            int yOffset = meterThickness > 0 ? theRest / 2 : 0;

            return new Rectangle(
                dash.X, dash.Y + yOffset,
                meterWidth, meterThickness);
        }

        private float Ratio
        {
            get {
                return Source.Ratio;
            } 
        }


        private Rectangle GetDashboardRectangle()
        {
            var bottomCenter = Pin;

            return new Rectangle((int)(bottomCenter.X - Width / 2), (int) (bottomCenter.Y - Height), Width, Height);
        }



        public void HandleInput(Forever.Screens.InputState input)
        {
        }


        public void Update(GameTime gameTime)
        {
        }
    }
}
