using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.Screens;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Aquarium
{
    public class DevScreenSwitchLauncher : GameScreen
    {
        DevScreenSwitcher Switcher { get; set; }
        public DevScreenSwitchLauncher()
        {
            var index = new List<GameScreen>
            {
                new InstancingScreen(),
                new VolumeScreen(),
                new NuclexFrameworkDemoScreen(),
                new OctTreeRayMarchingScreen(),
                new VoxelScreen(),
                new SimulationScreen(),
                new FlyAroundGameScreen()
            }.ToDictionary(x => x.GetType().ToString());;

            Switcher = new DevScreenSwitcher(this, "Screen Switcher", index, ShouldClose);
            KillCode = new Keys[]{
                Keys.Escape,
                Keys.OemTilde,
                Keys.Tab
            };
        }

        Keys[] KillCode { get; set;}
        public override void HandleInput(InputState input)
        {
            if (KillCode.All(key => input.IsKeyDown(key, PlayerIndex.One)))
            {
                ScreenManager.Game.Exit();
            } 
            else if (input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                BringUpSwitcherScreen();
            }

            base.HandleInput(input);
        }

        public void BringUpSwitcherScreen()
        {
            if (!ScreenManager.GetScreens().Any(x => x == Switcher))
            {
                ScreenManager.AddScreen(Switcher);
            }
        }

        bool ShouldClose(GameScreen screen)
        {
            return screen != this;
        }
    }

    class DevScreenSwitcher : MenuScreen
    {
        DevScreenSwitchLauncher Launcher { get; set; }
        Dictionary<string, GameScreen> Index { get; set; }
        Func<GameScreen, bool> ShouldClose { get; set; }
        public DevScreenSwitcher(DevScreenSwitchLauncher launcher,  string menuName, Dictionary<string, GameScreen> index, Func<GameScreen, bool> shouldClose) : base(menuName)
        {
            Launcher = launcher;
            Index = index;
            ShouldClose = shouldClose;
        }

        public override void LoadContent()
        {
            base.LoadContent();

            foreach (var name in Index.Keys)
            {
                MenuEntries.Add(new MenuEntry(name));
            }
        }

        public override void UnloadContent()
        {
            MenuEntries.Clear();

            base.UnloadContent();
        }

        protected override void OnSelectEntry(int entryIndex)
        {
            base.OnSelectEntry(entryIndex);

            var name = MenuEntries[entryIndex].Text;
            CloseAll();
            GiveControl(Index[name]);
        }

        IEnumerable<GameScreen> FindSetToClose()
        {
            return ScreenManager.GetScreens().Where(x => ShouldClose(x)).ToList();
        }

        void CloseAll()
        {
            foreach (var screen in FindSetToClose())
            {
                screen.ExitScreen();
            }
        }

        void GiveControl(GameScreen screen)
        {
            LoadingScreen.Load(ScreenManager, true, new [] { Launcher, screen });
        }
    }
}
