using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.Agent;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


using Ruminate.GUI.Framework;
using Ruminate.GUI.Content;

using Forever.Render;
using System.IO;
using Microsoft.Xna.Framework;
using Aquarium.Ui.Targets;
using Forever.Screens;
using Aquarium.Life;
using Aquarium.Targeting;

namespace Aquarium.Ui
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
            
            var panel = new Panel(5, 20, 600, 850);
            panel.AddWidget(closeButton);
            panel.AddWidget(QueueSizeLabel);

            var pickerHeight = 30;
            int pickers = 0;
            int start = 35;
            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 0, 2000, 
                "Max Pop", 
                () => { return Agent.MaxPopSize; }, 
                (v) => { Agent.MaxPopSize = (int) v; }));

            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 0, 1000, 
                "Range", 
                () => { return Agent.SpawnRange; },
                (v) => { Agent.SpawnRange = (int) v; }));

            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 0, 50, 
                "MaxQueue", 
                () => { return Agent.MaxBirthQueueSize; },
                (v) => { Agent.MaxBirthQueueSize = (int)v; }));

            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 0, 10000,
                "Max Energy",
                () => { return LifeForce.Data.MaxEnergy; },
                (v) => { LifeForce.Data.MaxEnergy = (int)v; }));

            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 0, 2,
                "Basal Cost",
                () => { return LifeForce.Data.BasalEnergyCost; },
                (v) => { LifeForce.Data.BasalEnergyCost = v; }));

            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 0, 1,
                "Body Part Cost",
                () => { return LifeForce.Data.BodyPartUnitCost; },
                (v) => { LifeForce.Data.BodyPartUnitCost = v; }));

            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 0, 1,
                "Volume Cost",
                () => { return LifeForce.Data.BodyPartVolumeCost; },
                (v) => { LifeForce.Data.BodyPartVolumeCost = v; }));

            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 0, 1,
                "Neural Cost",
                () => { return LifeForce.Data.NeuralOrganCost; },
                (v) => { LifeForce.Data.NeuralOrganCost = v; }));

            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 0, 1,
                "OrganUnitCost",
                () => { return LifeForce.Data.OrganUnitCost; },
                (v) => { LifeForce.Data.OrganUnitCost = v; }));

            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 0, 1,
                "AbilityOrganCost",
                () => { return LifeForce.Data.AbilityOrganCost; },
                (v) => { LifeForce.Data.AbilityOrganCost = v; }));

            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 0, 1,
                "TimerOrganCost",
                () => { return LifeForce.Data.TimerOrganCost; },
                (v) => { LifeForce.Data.TimerOrganCost = v; }));

            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 0, 10,
                "AbilityFiringBaseCost",
                () => { return LifeForce.Data.AbilityFiringBaseCost; },
                (v) => { LifeForce.Data.AbilityFiringBaseCost = v; }));

            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 0, 10,
                "ThrusterFiringCost",
                () => { return LifeForce.Data.ThrusterFiringCost; },
                (v) => { LifeForce.Data.ThrusterFiringCost = v; }));

            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 0, 10,
                "SpinnerFiringCost",
                () => { return LifeForce.Data.SpinnerFiringCost; },
                (v) => { LifeForce.Data.SpinnerFiringCost = v; }));

            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 0, 10,
                "SensorFiringCost",
                () => { return LifeForce.Data.SensorFiringCost; },
                (v) => { LifeForce.Data.SensorFiringCost = v; }));

            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 0, 10,
                "BiterFiringCost",
                () => { return LifeForce.Data.BiterFiringCost; },
                (v) => { LifeForce.Data.BiterFiringCost = v; }));


            var sp = Agent.SpecParser;
            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 1, 30,
                "MinBodyParts",
                () => { return (int)sp.MinBodyParts; },
                (v) => { sp.MinBodyParts = (int)v; }));

            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 1, 50,
                "MaxBodyParts",
                () => { return (int)sp.MaxBodyParts; },
                (v) => { sp.MaxBodyParts = (int)v; }));

            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 1, 30,
                "MinOrgans",
                () => { return (int)sp.MinOrgans; },
                (v) => { sp.MinOrgans = (int)v; }));

            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 1, 50,
                "MaxOrgans",
                () => { return (int)sp.MaxOrgans; },
                (v) => { sp.MaxOrgans = (int)v; }));

            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 1, 30,
                "MinNeuralNetworks",
                () => { return (int)sp.MinNeuralNetworks; },
                (v) => { sp.MinNeuralNetworks = (int)v; }));

            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 1, 50,
                "MaxNeuralNetworks",
                () => { return (int)sp.MaxNeuralNetworks; },
                (v) => { sp.MaxNeuralNetworks = (int)v; }));

            Sliders.Add(new SliderWidgetGroup(0, start + (pickerHeight * pickers++), 0, 1,
                "Mutation %",
                () => { return Agent.MutationChance; },
                (v) => { Agent.MutationChance = v ; }));
           


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
