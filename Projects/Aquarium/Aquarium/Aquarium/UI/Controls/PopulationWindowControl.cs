using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls.Desktop;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface;
using Aquarium.Life;

namespace Aquarium.UI.Controls
{
    public partial class PopulationWindowControl : WindowControl
    {
        public PopulationWindowControl(int x, int y)
        {
            StartX = x;
            StartY = y;
            InitializeComponent();
        }

        public PopulationWindowControl()
        {
            InitializeComponent();
        }
    }

    partial class PopulationWindowControl : WindowControl
    {
        int StartX { get; set; }
        int StartY { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        Dictionary<string, SliderGroupControl> Sliders { get; set; }

        private void InitializeComponent()
        {
            this.Title = string.Format("Population Window");

            foreach (var control in CreateUi())
            {
                this.Children.Add(control);
            }
        }

        IEnumerable<Control> CreateUi()
        {
            int popLabelWidth = 100;
            int popLabelHeight = 20;

            var popLabel = new LabelControl
            {
                Text = "Population: ",
                Bounds = new UniRectangle(0, 0, popLabelWidth, popLabelHeight)
            };
            int popValueLabelWidth = 160;
            int popValueLabelHeight = 20;
            PopulationValueLabelControl = new LabelControl
            {
                Text = "0",
                Bounds = new UniRectangle(popLabelWidth, 0, popValueLabelWidth, popValueLabelHeight)
            };

            int choiceWidth = 100;
            int choiceHeight = 20;

            var genLabelWidth = 100;
            var genLabelHeight = choiceHeight;
            var birthStratLabel = new LabelControl
            {
                Text = "Generation: ",
                Bounds = new UniRectangle(popLabelWidth + popValueLabelWidth, 0, genLabelWidth, genLabelHeight)
            };

            RandomOptionControl = new OptionControl
            {
                Selected = false,
                Text = "Random",
                Bounds = new UniRectangle(popLabelWidth + popValueLabelWidth + genLabelWidth, 0, choiceWidth, choiceHeight)
            };

            MeiosisOptionControl = new OptionControl
            {
                Selected = false,
                Text = "Meiosis",
                Bounds = new UniRectangle(popLabelWidth + popValueLabelWidth + genLabelWidth + choiceWidth, 0, choiceWidth, choiceHeight)
            };

            var costRange = 10f;
            var partsScale = 50f;
            var sliderSpecs = new[]{
                new { Label = "Max Local Population", Scale = 1000f, ShowInt = true },
                new { Label = "Spawn Range", Scale = 1000f, ShowInt = true },
                new { Label = "Birth Energy", Scale = 10000f, ShowInt = false },
                new { Label = "Basal Cost", Scale = 2f, ShowInt = false },
                new { Label = "Volume Cost", Scale = 0.5f, ShowInt = false },
                new { Label = "Body Part Unit Cost", Scale = 10f, ShowInt = false },
                new { Label = "Neural Unit Cost", Scale = costRange, ShowInt = false },
                new { Label = "Organ Unit Cost", Scale = costRange, ShowInt = false },
                new { Label = "Ability Unit Cost", Scale = costRange, ShowInt = false },
                new { Label = "Timer Unit Cost", Scale = costRange, ShowInt = false },
                new { Label = "Ability Firing Cost", Scale = costRange, ShowInt = false },
                new { Label = "Thruster Firing Cost", Scale = costRange, ShowInt = false },
                new { Label = "Spinner Firing Cost", Scale = costRange, ShowInt = false },
                new { Label = "Sensor Firing Cost", Scale = costRange, ShowInt = false },
                new { Label = "Biter Firing Cost", Scale = costRange, ShowInt = false },
                new { Label = "Min Body Parts", Scale = partsScale, ShowInt = true },
                new { Label = "Max Body Parts", Scale = partsScale, ShowInt = true },
                new { Label = "Min Organs Per Part", Scale = partsScale, ShowInt = true },
                new { Label = "Max Organs Per Part", Scale = partsScale, ShowInt = true },
                new { Label = "Min Neural Networks Per Part", Scale = partsScale, ShowInt = true },
                new { Label = "Max Neural Networks Per Part", Scale = partsScale, ShowInt = true },
                new { Label = "Mutation Probability Per Birth", Scale = 2f, ShowInt = false },
            };

            Sliders = new Dictionary<string, SliderGroupControl>();
            int i = 0;
            int numPerColumn = 11;
            float panelPadding = 10;
            int sliderColumns = 2;
            int sliderRows = 11;

            float sliderPad = 2;
            float sliderWidth = SliderGroupControl.Width;
            float sliderHeight = SliderGroupControl.Height;

            float totalSliderPaddingX = (sliderColumns + 2) * sliderPad;
            float totalSliderPaddingY = (sliderRows + 2) * sliderPad;
            float sliderPanelWidth = (SliderGroupControl.Width * sliderColumns) + totalSliderPaddingX;
            float sliderPanelHeight = (SliderGroupControl.Height * sliderRows) + totalSliderPaddingY;
            
            int startX = 10;
            int startY = 50;

            var optionPanelHeight = choiceHeight + sliderPad  + sliderPad;
            var optionPanelWidth = choiceWidth * 2 + genLabelWidth + popValueLabelWidth + popLabelWidth;
            var optionPanel = new Control
            {
                Bounds = new UniRectangle(startX, startY, optionPanelWidth, optionPanelHeight)
            };

            optionPanel.Children.Add(popLabel);
            optionPanel.Children.Add(PopulationValueLabelControl);
            optionPanel.Children.Add(birthStratLabel);
            optionPanel.Children.Add(RandomOptionControl);
            optionPanel.Children.Add(MeiosisOptionControl);

            var sliderPanel = new Control()
            {
                Bounds = new UniRectangle(startX, startY + optionPanelHeight + panelPadding, sliderPanelWidth, sliderPanelHeight)
            };

            foreach (var sliderSpec in sliderSpecs)
            {
                float indexX = i / numPerColumn;
                float indexY = i % numPerColumn;
                var slider = CreateSliderGroup(
                    (indexX * sliderWidth) + (indexX + 1) * sliderPad, 
                    (indexY * sliderHeight) + (indexY + 1) * sliderPad, 
                    sliderWidth, 
                    sliderHeight,
                    sliderSpec.Label, sliderSpec.Scale, sliderSpec.ShowInt);
                i++;
                Sliders[sliderSpec.Label] = slider;
                sliderPanel.Children.Add(slider);
            }
            Width = startX + panelPadding + sliderPanelWidth + panelPadding;
            Height = startY + panelPadding + optionPanelHeight + panelPadding + sliderPanelHeight + panelPadding + 5;

            this.Bounds = new UniRectangle(StartX + panelPadding, StartY + panelPadding, Width, Height);

            CloseWindowButton = new CloseWindowButtonControl(this);

            return new List<Control>
                {
                    CloseWindowButton,
                    optionPanel,
                    sliderPanel,
                };
        }


        SliderGroupControl CreateSliderGroup(float x, float y, float w, float h, string text, float scale, bool showInt)
        {
            return new SliderGroupControl()
            {
                Text = text,
                Bounds = new UniRectangle(x, y, w, h),
                Scale = scale,
                ShowInt = showInt
            };
        }

        private ButtonControl CloseWindowButton { get; set; }
        public event EventHandler OnCloseButtonPress { add { CloseWindowButton.Pressed += value; } remove { CloseWindowButton.Pressed -= value; } }
        public OptionControl RandomOptionControl { get; set; }
        public OptionControl MeiosisOptionControl { get; set; }
        private LabelControl PopulationValueLabelControl { get; set; }

        public class Bindings
        {
            PopulationWindowControl PopulationWindowControl { get; set; }
            Aquarium.Agent.SpawnerAgent SpawnerAgent { get; set; }
            Aquarium.Life.Spec.OrganismSpecParser SpecParser { get; set; }

            public Bindings(PopulationWindowControl popWindow, Aquarium.Agent.SpawnerAgent spawnerAgent, Aquarium.Life.Spec.OrganismSpecParser specParser)
            {
                PopulationWindowControl = popWindow;
                SpawnerAgent = spawnerAgent;
                SpecParser = specParser;
            }

            public void SetupCallbacks()
            {
                PopulationWindowControl.RandomOptionControl.Changed += new EventHandler(RandomOptionControl_Changed);
                PopulationWindowControl.MeiosisOptionControl.Changed += new EventHandler(MeiosisOptionControl_Changed);
                PopulationWindowControl.Sliders["Max Local Population"].ValueChanged += new EventHandler(Bindings_ValueChanged_MaxLocalPop);
                PopulationWindowControl.Sliders["Spawn Range"].ValueChanged += new EventHandler(Bindings_ValueChanged_SpawnRange);
                PopulationWindowControl.Sliders["Birth Energy"].ValueChanged += new EventHandler(Bindings_ValueChanged_BirthEnergy);
                PopulationWindowControl.Sliders["Basal Cost"].ValueChanged += new EventHandler(Bindings_ValueChanged_BasalCost);
                PopulationWindowControl.Sliders["Volume Cost"].ValueChanged += new EventHandler(Bindings_ValueChanged_VolumeCost);
                PopulationWindowControl.Sliders["Neural Unit Cost"].ValueChanged += new EventHandler(Bindings_ValueChanged_NeuralUnitCost);
                PopulationWindowControl.Sliders["Organ Unit Cost"].ValueChanged += new EventHandler(Bindings_ValueChanged_OrganUnitCost);
                PopulationWindowControl.Sliders["Ability Unit Cost"].ValueChanged +=new EventHandler(Bindings_ValueChanged_AbilityUnitCost);
                PopulationWindowControl.Sliders["Timer Unit Cost"].ValueChanged +=new EventHandler(Bindings_ValueChanged_TimerUnitCost);
                PopulationWindowControl.Sliders["Ability Firing Cost"].ValueChanged +=new EventHandler(Bindings_ValueChanged_AbilityFiringCost);
                PopulationWindowControl.Sliders["Thruster Firing Cost"].ValueChanged +=new EventHandler(Bindings_ValueChanged_ThrusterFiringCost);
                PopulationWindowControl.Sliders["Spinner Firing Cost"].ValueChanged +=new EventHandler(Bindings_ValueChanged_SpinnerFiringCost);
                PopulationWindowControl.Sliders["Sensor Firing Cost"].ValueChanged += new EventHandler(Bindings_ValueChanged_SensorFiringCost);
                PopulationWindowControl.Sliders["Biter Firing Cost"].ValueChanged +=new EventHandler(Bindings_ValueChanged_BiterFiringCost);
                
                PopulationWindowControl.Sliders["Min Body Parts"].ValueChanged += new EventHandler(Bindings_ValueChanged_MinBodyParts);
                PopulationWindowControl.Sliders["Max Body Parts"].ValueChanged += new EventHandler(Bindings_ValueChanged_MaxBodyParts);

                PopulationWindowControl.Sliders["Min Organs Per Part"].ValueChanged += new EventHandler(Bindings_ValueChanged_MinOrgans);
                PopulationWindowControl.Sliders["Max Organs Per Part"].ValueChanged += new EventHandler(Bindings_ValueChanged_MaxOrgans);

                PopulationWindowControl.Sliders["Min Neural Networks Per Part"].ValueChanged +=new EventHandler(Bindings_ValueChanged_MinNeuralNetworks);
                PopulationWindowControl.Sliders["Max Neural Networks Per Part"].ValueChanged += new EventHandler(Bindings_ValueChanged_MaxNeuralNetworks);
                PopulationWindowControl.Sliders["Mutation Probability Per Birth"].ValueChanged += new EventHandler(Bindings_ValueChanged_MutationChance);
            }

            void Bindings_ValueChanged_MutationChance(object sender, EventArgs e)
            {
                var sliderGroup = sender as SliderGroupControl;
                SpawnerAgent.MutationChance = PopulationWindowControl.Sliders[sliderGroup.NameLabel.Text].Value;
            }

            void Bindings_ValueChanged_MaxNeuralNetworks(object sender, EventArgs e)
            {
                var sliderGroup = sender as SliderGroupControl;
                SpecParser.MaxNeuralNetworks = (int)PopulationWindowControl.Sliders[sliderGroup.NameLabel.Text].Value;
            }
            void Bindings_ValueChanged_MinNeuralNetworks(object sender, EventArgs e)
            {
                var sliderGroup = sender as SliderGroupControl;
                SpecParser.MinNeuralNetworks = (int)PopulationWindowControl.Sliders[sliderGroup.NameLabel.Text].Value;
            }
            void Bindings_ValueChanged_MaxOrgans(object sender, EventArgs e)
            {
                var sliderGroup = sender as SliderGroupControl;
                SpecParser.MaxOrgans = (int)PopulationWindowControl.Sliders[sliderGroup.NameLabel.Text].Value;
            }
            void Bindings_ValueChanged_MinOrgans(object sender, EventArgs e)
            {
                var sliderGroup = sender as SliderGroupControl;
                SpecParser.MinOrgans = (int)PopulationWindowControl.Sliders[sliderGroup.NameLabel.Text].Value;
            }
            void Bindings_ValueChanged_MaxBodyParts(object sender, EventArgs e)
            {
                var sliderGroup = sender as SliderGroupControl;
                SpecParser.MaxBodyParts = (int)PopulationWindowControl.Sliders[sliderGroup.NameLabel.Text].Value;
            }
            void Bindings_ValueChanged_MinBodyParts(object sender, EventArgs e)
            {
                var sliderGroup = sender as SliderGroupControl;
                SpecParser.MinBodyParts = (int)PopulationWindowControl.Sliders[sliderGroup.NameLabel.Text].Value;
            }
            void  Bindings_ValueChanged_BiterFiringCost(object sender, EventArgs e)
            {
                var sliderGroup = sender as SliderGroupControl;
                LifeForce.Data.BiterFiringCost = PopulationWindowControl.Sliders[sliderGroup.NameLabel.Text].Value;
            }
            void  Bindings_ValueChanged_SensorFiringCost(object sender, EventArgs e)
            {
                var sliderGroup = sender as SliderGroupControl;
                LifeForce.Data.SensorFiringCost = PopulationWindowControl.Sliders[sliderGroup.NameLabel.Text].Value;
            }
            void  Bindings_ValueChanged_SpinnerFiringCost(object sender, EventArgs e)
            {
                var sliderGroup = sender as SliderGroupControl;
                LifeForce.Data.SpinnerFiringCost = PopulationWindowControl.Sliders[sliderGroup.NameLabel.Text].Value;
            }
            void  Bindings_ValueChanged_ThrusterFiringCost(object sender, EventArgs e)
            {
                var sliderGroup = sender as SliderGroupControl;
                LifeForce.Data.ThrusterFiringCost = PopulationWindowControl.Sliders[sliderGroup.NameLabel.Text].Value;
            }
            void  Bindings_ValueChanged_AbilityFiringCost(object sender, EventArgs e)
            {
                var sliderGroup = sender as SliderGroupControl;
                LifeForce.Data.AbilityFiringBaseCost = PopulationWindowControl.Sliders[sliderGroup.NameLabel.Text].Value;
            }
            void  Bindings_ValueChanged_TimerUnitCost(object sender, EventArgs e)
            {
                var sliderGroup = sender as SliderGroupControl;
                LifeForce.Data.TimerOrganCost = PopulationWindowControl.Sliders[sliderGroup.NameLabel.Text].Value;
            }
            void Bindings_ValueChanged_AbilityUnitCost(object sender, EventArgs e)
            {
                var sliderGroup = sender as SliderGroupControl;
                LifeForce.Data.AbilityOrganCost = PopulationWindowControl.Sliders[sliderGroup.NameLabel.Text].Value;
            }

            void  Bindings_ValueChanged_OrganUnitCost(object sender, EventArgs e)
            {
                var sliderGroup = sender as SliderGroupControl;
                LifeForce.Data.OrganUnitCost = PopulationWindowControl.Sliders[sliderGroup.NameLabel.Text].Value;
            }

            void Bindings_ValueChanged_NeuralUnitCost(object sender, EventArgs e)
            {
                var sliderGroup = sender as SliderGroupControl;
                LifeForce.Data.NeuralOrganCost = PopulationWindowControl.Sliders[sliderGroup.NameLabel.Text].Value;
            }

            void Bindings_ValueChanged_VolumeCost(object sender, EventArgs e)
            {
                var sliderGroup = sender as SliderGroupControl;
                LifeForce.Data.BodyPartVolumeCost = PopulationWindowControl.Sliders[sliderGroup.NameLabel.Text].Value;
            }
            void Bindings_ValueChanged_BasalCost(object sender, EventArgs e)
            {
                var sliderGroup = sender as SliderGroupControl;
                LifeForce.Data.BasalEnergyCost = PopulationWindowControl.Sliders[sliderGroup.NameLabel.Text].Value;
            }

            void MeiosisOptionControl_Changed(object sender, EventArgs e)
            {
                SpawnerAgent.UseMeiosis = PopulationWindowControl.MeiosisOptionControl.Selected;
            }

            void RandomOptionControl_Changed(object sender, EventArgs e)
            {
                SpawnerAgent.UseRandom = PopulationWindowControl.RandomOptionControl.Selected;
            }

            void Bindings_ValueChanged_BirthEnergy(object sender, EventArgs e)
            {
                var sliderGroup = sender as SliderGroupControl;
                LifeForce.Data.MaxEnergy = (int)PopulationWindowControl.Sliders[sliderGroup.NameLabel.Text].Value;
            }

            void Bindings_ValueChanged_SpawnRange(object sender, EventArgs e)
            {
                var sliderGroup = sender as SliderGroupControl;
                SpawnerAgent.SpawnRange = (int)PopulationWindowControl.Sliders[sliderGroup.NameLabel.Text].Value;
            }

            void Bindings_ValueChanged_MaxLocalPop(object sender, EventArgs e)
            {
                var sliderGroup = sender as SliderGroupControl;
                SpawnerAgent.MaxPopSize = (int)PopulationWindowControl.Sliders[sliderGroup.NameLabel.Text].Value;
            }
           
            public static void ReadFromModel(PopulationWindowControl popWindow, Aquarium.Agent.SpawnerAgent spawnerAgent, Aquarium.Life.Spec.OrganismSpecParser specParser)
            {
                popWindow.RandomOptionControl.Selected = spawnerAgent.UseRandom;
                popWindow.MeiosisOptionControl.Selected = spawnerAgent.UseMeiosis;

                popWindow.Sliders["Max Local Population"].Value = spawnerAgent.MaxPopSize;
                popWindow.Sliders["Spawn Range"].Value = spawnerAgent.SpawnRange;
                popWindow.Sliders["Birth Energy"].Value = LifeForce.Data.MaxEnergy;
                popWindow.Sliders["Basal Cost"].Value = LifeForce.Data.BasalEnergyCost;
                popWindow.Sliders["Volume Cost"].Value = LifeForce.Data.BodyPartVolumeCost;
                popWindow.Sliders["Neural Unit Cost"].Value = LifeForce.Data.NeuralOrganCost;
                popWindow.Sliders["Organ Unit Cost"].Value = LifeForce.Data.OrganUnitCost;
                popWindow.Sliders["Ability Unit Cost"].Value = LifeForce.Data.AbilityOrganCost;
                popWindow.Sliders["Timer Unit Cost"].Value = LifeForce.Data.TimerOrganCost;
                popWindow.Sliders["Ability Firing Cost"].Value = LifeForce.Data.AbilityFiringBaseCost;
                popWindow.Sliders["Thruster Firing Cost"].Value = LifeForce.Data.ThrusterFiringCost;
                popWindow.Sliders["Spinner Firing Cost"].Value = LifeForce.Data.SpinnerFiringCost;
                popWindow.Sliders["Sensor Firing Cost"].Value = LifeForce.Data.SensorFiringCost;
                popWindow.Sliders["Biter Firing Cost"].Value = LifeForce.Data.BiterFiringCost;

                popWindow.Sliders["Min Body Parts"].Value = specParser.MinBodyParts;
                popWindow.Sliders["Max Body Parts"].Value = specParser.MaxBodyParts;

                popWindow.Sliders["Min Organs Per Part"].Value = specParser.MinOrgans;
                popWindow.Sliders["Max Organs Per Part"].Value = specParser.MaxOrgans;

                popWindow.Sliders["Min Neural Networks Per Part"].Value = specParser.MinNeuralNetworks;
                popWindow.Sliders["Max Neural Networks Per Part"].Value = specParser.MaxNeuralNetworks;
                popWindow.Sliders["Mutation Probability Per Birth"].Value = spawnerAgent.MutationChance;

                // TODO - remove this, for testing
                new Bindings(popWindow, spawnerAgent, specParser).SetupCallbacks();
            }

        }
    }


}
