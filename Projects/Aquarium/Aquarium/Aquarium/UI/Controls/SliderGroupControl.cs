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
    public partial class SliderGroupControl : Control
    {
        /// <summary>Initializes a new GUI demonstration dialog</summary>
        public SliderGroupControl()
        {
            InitializeComponent();
        }

    }

    partial class SliderGroupControl
    {
        public static readonly float SliderWidth = 270;
        public static readonly float SliderHeight = 25;
        public static readonly float LabelWidth = 100;
        public static readonly float LabelHeight = 10;
        public static readonly float ValueLabelWidth = 90;
        public static readonly float ValueLabelHeight = 20;

        public bool ShowInt { get; set; }
        public static readonly float Width = Pad + SliderWidth + Pad + ValueLabelWidth + Pad;
        public static readonly float Height = Pad + LabelHeight + Pad + SliderHeight + Pad;

        public static readonly float Pad = 5;

        float _Scale;
        public float Scale
        {
            get { return _Scale; }
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

        // Summary:
        //     Triggered when the slider has been moved
        public event EventHandler ValueChanged;

        public LabelControl NameLabel { get; set; }
        public LabelControl ValueLabel { get; set; }
        HorizontalSliderControl Slider { get; set; }
        public void InitializeComponent()
        {
            _Scale = 1f;
            NameLabel = new LabelControl()
            {
                Bounds = new UniRectangle(Pad, Pad, LabelWidth, LabelHeight)
            };

            Slider = new HorizontalSliderControl
            {
                Bounds = new UniRectangle(Pad, Pad + LabelHeight + Pad, SliderWidth, SliderHeight),
                ThumbSize = 0.1f,
                ThumbPosition = 0.0f,
            };

            ValueLabel = new LabelControl()
            {
                Bounds = new UniRectangle(Pad + SliderWidth + Pad, Pad + LabelHeight + Pad, ValueLabelWidth, ValueLabelHeight)
            };

            Slider.Moved += new EventHandler(Slider_Moved);

            Children.Add(NameLabel);
            Children.Add(ValueLabel);
            Children.Add(Slider);
        }

        void Slider_Moved(object sender, EventArgs e)
        {
            UpdateValueLabel();
            if (ValueChanged != null)
            {
                ValueChanged(this, e);
            }
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
