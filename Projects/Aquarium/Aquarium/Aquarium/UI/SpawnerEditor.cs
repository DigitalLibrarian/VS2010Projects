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
            this.PropagateInput = false;
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
            }
        }


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
            
            var panel = new Panel(5, 20, 280, 300);
            panel.AddWidget(closeButton);

            var pickerHeight = 30;
            NumPickers.Add(new NumPickerWidgetGroup(5, 35 + (pickerHeight * 0), "Max Pop", () => { return Agent.MaxPopSize; }, (v) => { Agent.MaxPopSize = v; }));
            NumPickers.Add(new NumPickerWidgetGroup(5, 35 + (pickerHeight * 1), "Range", () => { return Agent.SpawnRange; }, (v) => { Agent.SpawnRange = v; }));
            NumPickers.Add(new NumPickerWidgetGroup(5, 35 + (pickerHeight * 2), "MaxQueue", () => { return Agent.MaxBirthQueueSize; }, (v) => { Agent.MaxBirthQueueSize = v; }));
            NumPickers.Add(new NumPickerWidgetGroup(5, 35 + (pickerHeight * 3), "# Parts", () => { return Agent.DefaultParts; }, (v) => { Agent.DefaultParts = v; }));
            NumPickers.Add(new NumPickerWidgetGroup(5, 35 + (pickerHeight * 4), "# Organs", () => { return Agent.DefaultOrgans; }, (v) => { Agent.DefaultOrgans = v; }));
            NumPickers.Add(new NumPickerWidgetGroup(5, 35 + (pickerHeight * 5), "# Networks", () => { return Agent.DefaultNN; }, (v) => { Agent.DefaultNN = v; }));
            NumPickers.Add(new NumPickerWidgetGroup(5, 35 + (pickerHeight * 6), "Max Genome", () => { return Agent.GeneCap; }, (v) => { Agent.GeneCap = v; }));


            foreach (var pickerGroup in NumPickers)
            {
                foreach (var widget in pickerGroup.Widgets)
                {
                    panel.AddWidget(widget);
                }
            }

            return new Gui(Game, skin, text, testSkins, testTexts)
            {
                Widgets = new Widget[] {
                    panel
                }
            };
        }

        List<NumPickerWidgetGroup> NumPickers = new List<NumPickerWidgetGroup>();

    }

    class NumPickerWidgetGroup
    {
        public List<Widget> Widgets
        {
            get;
            private set;
        }

        public Label DisplayLabel { get; private set; }
        public Label ValueLabel { get; private set; }
        public Button DownButton { get; private set; }
        public Button UpButton { get; private set; }

        Func<int> Getter { get; set; }
        Action<int> Setter { get; set; }

        public NumPickerWidgetGroup(int x, int y, string labelText, Func<int> getter, Action<int> setter)
        {
            Getter = getter;
            Setter = setter;

            var buttonWidth = 15;
            var labelWidth = 150;
            var valueLabelWidth = 50;
            var pad = 5;

            var label = new Label(x + pad, y, labelText);
            var downButton = new Button(x + pad + labelWidth + pad, y, buttonWidth, "-");
            var valueLabel = new Label(x + pad + labelWidth + pad + buttonWidth + pad, y, Getter().ToString());
            var upButton = new Button(x + pad + labelWidth + pad + buttonWidth + pad + valueLabelWidth + pad, y, buttonWidth + pad, "+");

            DisplayLabel = label;
            DownButton = downButton;
            UpButton = upButton;
            ValueLabel = valueLabel;

            DownButton.ClickEvent += new WidgetEvent((w) =>
            {
                Decrement();
                UpdateLabel();
            });

            UpButton.ClickEvent += new WidgetEvent((w) => {
                Increment();
                UpdateLabel();
            });

            Widgets = new List<Widget>
            {
                label,
                downButton,
                valueLabel,
                upButton
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

        }

        public void UpdateLabel()
        {
           ValueLabel.Value = Getter().ToString();
        }


    }


}
