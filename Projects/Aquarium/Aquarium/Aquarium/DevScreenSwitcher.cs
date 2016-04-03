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

        }

        protected override void OnSelectEntry(int entryIndex)
        {
            base.OnSelectEntry(entryIndex);

            var name = MenuEntries[entryIndex].Text;

            if (name == "Close All")
            {
                CloseAll();
            }
            else
            {
                CloseAll();
                GiveControl(Index[name]);
            }

        }

        IEnumerable<GameScreen> Others()
        {
            return ScreenManager.GetScreens().Where(x => x != this).ToList();
        }

        void CloseAll()
        {
            foreach (var screen in ScreenManager.GetScreens())
            {
                screen.ExitScreen();
            }
        }

        void GiveControl(GameScreen screen)
        {
            ScreenManager.AddScreen(screen);
            //ExitScreen();
        }

    }
}
