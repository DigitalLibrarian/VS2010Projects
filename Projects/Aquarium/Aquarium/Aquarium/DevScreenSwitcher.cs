using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.Screens;

namespace Aquarium
{
    class DevScreenSwitcher : MenuScreen
    {
        Dictionary<string, GameScreen> Index { get; set; }
        public DevScreenSwitcher(string menuName, Dictionary<string, GameScreen> index) : base(menuName)
        {
            Index = index;
        }

        public override void LoadContent()
        {
            base.LoadContent();

            foreach (var name in Index.Keys)
            {
                MenuEntries.Add(new MenuEntry(name));
            }

            MenuEntries.Add(new MenuEntry("Close All"));
        }

        protected override void OnSelectEntry(int entryIndex)
        {
            base.OnSelectEntry(entryIndex);

            var name = MenuEntries[entryIndex].Text;

            if (name == "Close All")
            {
                foreach (var screen in Others())
                {
                    screen.ExitScreen();
                }
            }
            else
            {
                GiveControl(Index[name]);
            }

        }

        IEnumerable<GameScreen> Others()
        {
            return ScreenManager.GetScreens().Where(x => x != this).ToList();
        }

        void GiveControl(GameScreen screen)
        {
            ScreenManager.AddScreen(screen);
            //ExitScreen();
        }

    }
}
