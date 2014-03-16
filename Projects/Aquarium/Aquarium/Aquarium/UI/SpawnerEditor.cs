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

    public abstract class RuminateGuiGameScreen : GameScreen
    {
        public Gui Gui { get; private set; }

        protected abstract Gui CreateGui();

        public RuminateGuiGameScreen()
        {
            this.PropagateInput = true;
            this.PropagateDraw = true;
            this.TransitionOffTime = new TimeSpan(0, 0, 0, 0, 50);
        }


        public override void LoadContent()
        {
            base.LoadContent();
            if (Gui == null)
            {
                Gui = CreateGui();
            }
        }
        
        public override void Draw(GameTime gameTime)
        {
            Gui.Draw();
            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            Gui.Update();
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }
    }


    public class SpawnerEditor : RuminateGuiGameScreen
    {
        public SpawnerAgent Agent { get; private set; }
        public RenderContext RenderContext { get; private set; }

        Game Game { get; set; }


        public SpawnerEditor(Game game, RenderContext renderContext)
        {
            Game = game;
            RenderContext = renderContext;
        }

        const string ImageMapAsset = @"SimpleGui\Skins\TestSkin\ImageMap";
        const string MapAsset = @"Content\SimpleGui\Skins\TestSkin\Map.txt";
        const string FontAsset = @"SimpleGui\Skins\TestSkin\Font";
        


        public bool IsOpen { get; private set; }
        public void AcquireTarget(ITarget target)
        {
            Edit((SpawnerAgent)target);
        }
                
        private void Edit(SpawnerAgent agent)
        {
            IsOpen = true;
            Agent = agent;

            foreach (var group in NumPickers)
            {
                group.UpdateLabel();

                UseRandom.SetToggle(Agent.UseRandom);
                UseMeiosis.SetToggle(Agent.UseMeiosis);
            }
        }
        CheckBox UseRandom;
        CheckBox UseMeiosis;

        public void Close()
        {
            Agent = null;
            IsOpen = false;
            this.ExitScreen();
        }


        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        


        protected override Gui CreateGui()
        {
            var content = this.ScreenManager.Game.Content;
            var imageMap = content.Load<Texture2D>(SpawnerEditor.ImageMapAsset);
            var map = File.OpenText(SpawnerEditor.MapAsset).ReadToEnd();
            var font = content.Load<SpriteFont>(SpawnerEditor.FontAsset);
            var skin = new Skin(imageMap, map);
            var text = new Text(font, Color.Cyan);


            var testSkins = new[] { new Tuple<string, Skin>("testSkin", skin) };
            var testTexts = new[] { new Tuple<string, Text>("testText", text) };


            var closeButton = new Button(5, 5, "x") { Skin = "testSkin", Text = "testText" };
            var wEvent = new WidgetEvent((width) => this.Close());
            closeButton.ClickEvent += wEvent;
            
            var panel = new Panel(5, 20, 500, 300);
            panel.AddWidget(closeButton);

            var pickerHeight = 30;
            int pickers = 0;
            int start = 35;
            NumPickers.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 0, 1000, "Max Pop", () => { return Agent.MaxPopSize; }, (v) => { Agent.MaxPopSize = v; }));
            NumPickers.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 0, 200, "Range", () => { return Agent.SpawnRange; }, (v) => { Agent.SpawnRange = v; }));
            NumPickers.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 0, 1000, "MaxQueue", () => { return Agent.MaxBirthQueueSize; }, (v) => { Agent.MaxBirthQueueSize = v; }));
            NumPickers.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 1, 50, "# Parts", () => { return Agent.DefaultParts; }, (v) => { Agent.DefaultParts = v; }));
            NumPickers.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 1, 50, "# Organs", () => { return Agent.DefaultOrgans; }, (v) => { Agent.DefaultOrgans = v; }));
            NumPickers.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 1, 50, "# Networks", () => { return Agent.DefaultNN; }, (v) => { Agent.DefaultNN = v; }));
            NumPickers.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 1, 10000, "Max Genome", () => { return Agent.GeneCap; }, (v) => { Agent.GeneCap = v; }));


            foreach (var pickerGroup in NumPickers)
            {
                foreach (var widget in pickerGroup.Widgets)
                {
                    panel.AddWidget(widget);
                }
            }

            var pickersEnd = start + (pickerHeight * pickers);

            UseRandom = new CheckBox(0, pickersEnd, "Random");
            UseRandom.OnToggle += new WidgetEvent((Widget target) => { Agent.UseRandom = true; });
            UseRandom.OffToggle += new WidgetEvent((Widget target) => { Agent.UseRandom = false; });

            UseMeiosis = new CheckBox(100, pickersEnd, "Meiosis");
            UseMeiosis.OnToggle += new WidgetEvent((Widget target) => { Agent.UseMeiosis = true; });
            UseMeiosis.OffToggle += new WidgetEvent((Widget target) => { Agent.UseMeiosis = false; });

            panel.AddWidget(UseRandom);
            panel.AddWidget(UseMeiosis);

            return new Gui(Game, skin, text, testSkins, testTexts)
            {
                Widgets = new Widget[] {
                    panel
                }
            };
        }

        List<SliderWidgetGroup> NumPickers = new List<SliderWidgetGroup>();

    }

    class SliderWidgetGroup
    {
        public List<Widget> Widgets
        {
            get;
            private set;
        }
        int Max;
        int Min;
        public Slider Slider { get; private set; }
        public Label ValueLabel { get; private set; }

        Func<int> Getter { get; set; }
        Action<int> Setter { get; set; }

        public SliderWidgetGroup(int x, int y, int min, int max, string labelText, Func<int> getter, Action<int> setter)
        {
            Max = max;
            Min = min;
            Getter = getter;
            Setter = setter;

            var labelWidth = 150;
            var pad = 5;

            var sliderWidth = 200;
            var label = new Label(x + pad, y, labelText);
            var sliderLabel = new Label(x + pad + labelWidth + pad + sliderWidth + pad, y, Getter().ToString());
            var slider = new Slider(x + pad + labelWidth + pad, y, sliderWidth, delegate(Widget w) {
                        float rawValue = ((Slider)w).Value;
                        var diff = max - min;
                        var dist = diff * rawValue;

                        int newValue = min + (int) dist;
                        sliderLabel.Value = newValue.ToString();

                        Setter(newValue);
                    });
            Slider = slider;
            var v = Getter();
            var ratio = (v - min) / (max - min);
            slider.Value = ratio;
            slider.ValueChanged(slider);
            ValueLabel = sliderLabel;
           
            Widgets = new List<Widget>
            {
               
                label,
                slider,
                sliderLabel
            };
        }

        private void Increment()
        {
            var value = Getter();
            value++;
            Setter(value);
        }

        private void Decrement()
        {
            var value = Getter();
            value--;
            Setter(value);
        }

        public void UpdateLabel()
        {
           ValueLabel.Value = Getter().ToString();
            
        }


    }


}
