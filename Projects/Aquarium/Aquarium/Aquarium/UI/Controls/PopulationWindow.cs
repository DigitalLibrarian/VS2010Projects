using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls.Desktop;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface;

namespace Aquarium.UI.Controls
{

    public partial class PopulationWindow : WindowControl
    {

        /// <summary>Initializes a new GUI demonstration dialog</summary>
        public PopulationWindow()
        {
            InitializeComponent();
        }

    }

    partial class PopulationWindow : WindowControl
    {
        public bool UseRandom { get; set; }
        public bool UseMeiosis { get; set; }

        SliderGroup MaxPopSlider { get; set; }
        SliderGroup RangeSlider { get; set; }
        SliderGroup BirthEnergySlider { get; set; }


        private void InitializeComponent()
        {
            this.Title = string.Format("Population Window");
            this.Bounds = new UniRectangle(10, 50, 600, 600);

            foreach (var control in CreateUi())
            {
                this.Children.Add(control);
            }
        }

        IEnumerable<Control> CreateUi()
        {
            MaxPopSlider = CreateSliderGroup(0, "Max Local Population", 1000, showInt: true);
            RangeSlider = CreateSliderGroup(1, "Spawn Range", 2000, showInt: true);
            BirthEnergySlider = CreateSliderGroup(2, "Birth Energy", 1000, showInt: true);

            return new List<Control>{
                MaxPopSlider,
                RangeSlider,
                BirthEnergySlider
            };
        }

        SliderGroup CreateSliderGroup(int index, string text, float scale, bool showInt)
        {
            int sliderX = 10;
            int sliderStartY = 50;
            float sliderWidth = SliderGroup.Width;
            float sliderHeight = SliderGroup.Height;

            return new SliderGroup()
            {
                Text = text,
                Bounds = new UniRectangle(sliderX, sliderStartY + (index * sliderHeight), sliderWidth, sliderHeight),
                Scale = 2000,
                ShowInt = true
            };
        }

    }

}
