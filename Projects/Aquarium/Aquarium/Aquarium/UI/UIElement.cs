using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Forever.Screens;

namespace Aquarium.Ui
{
    public interface IUiElement
    {
        void HandleInput(InputState input);

        void Draw(GameTime gameTime, SpriteBatch batch);
        void Update(GameTime gameTime);

    }


}
