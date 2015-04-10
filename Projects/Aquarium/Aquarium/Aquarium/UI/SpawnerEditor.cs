using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.Sim.Agents;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


using Ruminate.GUI.Framework;
using Ruminate.GUI.Content;

using Forever.Render;
using System.IO;
using Microsoft.Xna.Framework;
using Aquarium.UI.Targets;
using Forever.Screens;

namespace Aquarium.UI
{
    public class SpawnerEditor : RuminateGuiGameScreen
    {
        public SpawnerAgent Agent { get; private set; }
        public RenderContext RenderContext { get; private set; }

        Game Game { get; set; }

        CheckBox UseRandom;
        CheckBox UseMeiosis;
        Label QueueSizeLabel;
        List<SliderWidgetGroup> Sliders = new List<SliderWidgetGroup>();

        public SpawnerEditor(Game game, RenderContext renderContext)
        {
            Game = game;
            RenderContext = renderContext;
        }


        public bool IsOpen { get; private set; }
        public void AcquireTarget(ITarget target)
        {
            Edit((SpawnerAgent)target);
        }
                
        private void Edit(SpawnerAgent agent)
        {
            IsOpen = true;
            Agent = agent;

            foreach (var group in Sliders)
            {
                group.Reset();

                UseRandom.SetToggle(Agent.UseRandom);
                UseMeiosis.SetToggle(Agent.UseMeiosis);
            }
        }

        public void Close()
        {
            Agent = null;
            IsOpen = false;
            this.ExitScreen();
        }


        
        protected override Gui CreateGui()
        {
            var content = this.ScreenManager.Game.Content;
            var imageMap = content.Load<Texture2D>(AssetNames.SimpleGuiTestSkinImageMap);
            var map = File.OpenText(AssetNames.SimpleGuiTestSkinMap).ReadToEnd();
            var font = content.Load<SpriteFont>(AssetNames.SimpleGuiSpriteFont);
            var skin = new Skin(imageMap, map);
            var text = new Text(font, Color.Cyan);


            var testSkins = new[] { new Tuple<string, Skin>("testSkin", skin) };
            var testTexts = new[] { new Tuple<string, Text>("testText", text) };


            var closeButton = new Button(5, 5, "x") { Skin = "testSkin", Text = "testText" };
            var wEvent = new WidgetEvent((width) => this.Close());
            closeButton.ClickEvent += wEvent;

            QueueSizeLabel = new Label(35, 5, "");
            
            var panel = new Panel(5, 20, 500, 300);
            panel.AddWidget(closeButton);
            panel.AddWidget(QueueSizeLabel);

            var pickerHeight = 30;
            int pickers = 0;
            int start = 35;
            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 0, 200, "Max Pop", () => { return Agent.MaxPopSize; }, (v) => { Agent.MaxPopSize = v; }));
            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 0, 200, "Range", () => { return Agent.SpawnRange; }, (v) => { Agent.SpawnRange = v; }));
            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 0, 50, "MaxQueue", () => { return Agent.MaxBirthQueueSize; }, (v) => { Agent.MaxBirthQueueSize = v; }));
            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 1, 50, "# Parts", () => { return Agent.DefaultParts; }, (v) => { Agent.DefaultParts = v; }));
            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 1, 50, "# Organs", () => { return Agent.DefaultOrgans; }, (v) => { Agent.DefaultOrgans = v; }));
            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 1, 50, "# Networks", () => { return Agent.DefaultNN; }, (v) => { Agent.DefaultNN = v; }));
            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 1, 100000, "Max Genome", () => { return Agent.GeneCap; }, (v) => { Agent.GeneCap = v; }));


            foreach (var pickerGroup in Sliders)
            {
                foreach (var widget in pickerGroup.Widgets)
                {
                    panel.AddWidget(widget);
                }
            }

            var pickersEnd = start + (pickerHeight * pickers);

            UseRandom = new CheckBox(0, pickersEnd, "Random");
            UseRandom.OnToggle += new WidgetEvent((Widget target) => { if(Agent != null) Agent.UseRandom = true; });
            UseRandom.OffToggle += new WidgetEvent((Widget target) => { if (Agent != null) Agent.UseRandom = false; });

            UseMeiosis = new CheckBox(100, pickersEnd, "Meiosis");
            UseMeiosis.OnToggle += new WidgetEvent((Widget target) => { if (Agent != null) Agent.UseMeiosis = true; });
            UseMeiosis.OffToggle += new WidgetEvent((Widget target) => { if (Agent != null) Agent.UseMeiosis = false; });

            panel.AddWidget(UseRandom);
            panel.AddWidget(UseMeiosis);

            return new Gui(Game, skin, text, testSkins, testTexts)
            {
                Widgets = new Widget[] {
                    panel
                }
            };
        }


        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (IsOpen)
            {
                QueueSizeLabel.Value = string.Format("Queued: {0}", Agent.Births.Count());
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

    }

    


}
