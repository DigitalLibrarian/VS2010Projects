using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls.Arcade;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls.Desktop;

namespace Aquarium.UI.Controls
{

    public partial class SliderGroup : Control
    {
        /// <summary>Initializes a new GUI demonstration dialog</summary>
        public SliderGroup()
        {
            InitializeComponent();
        }

    }

    partial class SliderGroup 
    {
        public bool ShowInt { get; set; }
        public static readonly float Width = 400;
        public static readonly float Height = 50;

        float _Scale;
        public float Scale
        {
            get
            {
                return _Scale;
            }
            set
            {
                _Scale = value;
                
                UpdateValueLabel();
            }
        }
        public float Value
        {
            get { return Slider.ThumbPosition * Scale; }
            set {
                Slider.ThumbPosition = value / Scale;
                UpdateValueLabel();
            }
        }

        public string Text
        {
            get { return NameLabel.Text; }
            set { NameLabel.Text = value; }
        }

        public LabelControl NameLabel { get; set; }
        public LabelControl ValueLabel { get; set; }
        HorizontalSliderControl Slider { get; set; }
        public void InitializeComponent()
        {
            _Scale = 1f;
            NameLabel = new LabelControl()
            {
                Bounds = new UniRectangle(0, 0, 100, 20)
            };

            Slider = new HorizontalSliderControl
            {
                Bounds = new UniRectangle(0, 25, 300, 20),
                ThumbSize = 0.1f,
                ThumbPosition = 0.0f,
            };

            float valueLabelWidth = 25;
            float valueLabelHeight = 25;
            ValueLabel = new LabelControl()
            {
                Bounds = new UniRectangle(300 + 5, 20, valueLabelWidth, valueLabelHeight)
            };

            Slider.Moved += new EventHandler(Slider_Moved);

            Children.Add(NameLabel);
            Children.Add(ValueLabel);
            Children.Add(Slider);
        }

        void Slider_Moved(object sender, EventArgs e)
        {
            UpdateValueLabel();
        }

        private void UpdateValueLabel()
        {
            if (ShowInt)
            {
                ValueLabel.Text = ((int)Value).ToString();
            }
            else
            {
                ValueLabel.Text = Value.ToString();
            }
        }
    }
}
