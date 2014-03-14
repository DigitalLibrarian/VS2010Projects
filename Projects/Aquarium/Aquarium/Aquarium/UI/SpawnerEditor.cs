using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.Sim.Agents;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Forever.Render;

namespace Aquarium.UI
{
    class SpawnerEditor : IUIElement
    {
        public SpawnerAgent Agent { get; private set; }
        public RenderContext RenderContext { get; private set; }

        public List<Button> Buttons { get; private set; }

        public SpawnerEditor(RenderContext renderContext)
        {
            Buttons = new List<Button>();
            RenderContext = renderContext;
        }

        const string VerticalPanelAsset = "Panels/VerticalGradientGray";

        public void LoadContent(ContentManager content)
        {
            var verticalPanelTexture = content.Load<Texture2D>(VerticalPanelAsset);
            var button1 = new Button(new Microsoft.Xna.Framework.Rectangle(100, 100, 5, 20), verticalPanelTexture);
            var button2 = new Button(new Microsoft.Xna.Framework.Rectangle(120, 100, 5, 20), verticalPanelTexture);

            Buttons.Add(button1);
            Buttons.Add(button2);

        }
                


        public void Edit(SpawnerAgent agent)
        {
            Agent = agent;
        }

        public void Close()
        {
            Agent = null;
        }


        public void HandleInput(Forever.Screens.InputState input)
        {
            if (Agent == null) return;
            Buttons.ForEach(button => button.HandleInput(input));
        }

        public void Draw(Microsoft.Xna.Framework.GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch batch)
        {
            if (Agent == null) return;

            Buttons.ForEach(button => button.Draw(gameTime, batch, RenderContext));
        }

        public void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (Agent == null) return;
        }
    }
}
