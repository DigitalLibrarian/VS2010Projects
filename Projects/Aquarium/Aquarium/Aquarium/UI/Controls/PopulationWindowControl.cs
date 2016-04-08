using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls.Desktop;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface;

namespace Aquarium.UI.Controls
{
    public partial class PopulationWindowControl : WindowControl
    {
        /// <summary>Initializes a new GUI demonstration dialog</summary>
        public PopulationWindowControl()
        {
            InitializeComponent();
        }

    }

    partial class PopulationWindowControl : WindowControl
    {
        public float Width { get; set; }
        public float Height { get; set; }
        public bool UseRandom { get; set; }
        public bool UseMeiosis { get; set; }

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

            var sliderSpecs = new[]{
                new { Label = "Max Local Population", Scale = 1000, ShowInt = false },
                new { Label = "Spawn Range", Scale = 1000, ShowInt = false },
                new { Label = "Birth Energy", Scale = 1000, ShowInt = false },
                new { Label = "Basal Cost", Scale = 1000, ShowInt = false },
                new { Label = "Volume Cost", Scale = 1000, ShowInt = false },
                new { Label = "Body Part Unit Cost", Scale = 1000, ShowInt = false },
                new { Label = "Neural Unit Cost", Scale = 1000, ShowInt = false },
                new { Label = "Organ Unit Cost", Scale = 1000, ShowInt = false },
                new { Label = "Ability Unit Cost", Scale = 1000, ShowInt = false },
                new { Label = "Timer Unit Cost", Scale = 1000, ShowInt = false },
                new { Label = "Ability Firing Cost", Scale = 1000, ShowInt = false },
                new { Label = "Thruster Firing Cost", Scale = 1000, ShowInt = false },
                new { Label = "Spinner Firing Cost", Scale = 1000, ShowInt = false },
                new { Label = "Sensor Firing Cost", Scale = 1000, ShowInt = false },
                new { Label = "Biter Firing Cost", Scale = 1000, ShowInt = false },
                new { Label = "Min Body Parts", Scale = 1000, ShowInt = false },
                new { Label = "Max Body Parts", Scale = 1000, ShowInt = false },
                new { Label = "Min Organs Per Part", Scale = 1000, ShowInt = false },
                new { Label = "Max Organs Per Part", Scale = 1000, ShowInt = false },
                new { Label = "Min Neural Networks Per Part", Scale = 1000, ShowInt = false },
                new { Label = "Max Neural Networks Per Part", Scale = 1000, ShowInt = false },
                new { Label = "Mutation Probability Per Birth", Scale = 1000, ShowInt = false },
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
            int startY = 40;

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

            this.Bounds = new UniRectangle(panelPadding, panelPadding, Width, Height);


            var closeWindowButton = new CloseWindowButtonControl()
            {
                Text = "X",
                Bounds = new UniRectangle(this.Width - 25, 2, 21, 21),
            };
            closeWindowButton.Pressed += new EventHandler(closeWindowButton_Pressed);

            return new List<Control>
                {
                    closeWindowButton,
                    optionPanel,
                    sliderPanel,
                };
        }

        void closeWindowButton_Pressed(object sender, EventArgs e)
        {
            this.Close();
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

        private OptionControl RandomOptionControl { get; set; }
        private OptionControl MeiosisOptionControl { get; set; }
        private LabelControl PopulationValueLabelControl { get; set; }
    }

}
