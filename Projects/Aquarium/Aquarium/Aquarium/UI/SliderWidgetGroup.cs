using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ruminate.GUI.Framework;
using Ruminate.GUI.Content;

namespace Aquarium.Ui
{
    public class SliderWidgetGroup
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
            var slider = new Slider(x + pad + labelWidth + pad, y, sliderWidth, delegate(Widget w)
            {
                float rawValue = ((Slider)w).Value;
                var diff = max - min;
                var dist = diff * rawValue;

                int newValue = min + (int)dist;
                sliderLabel.Value = newValue.ToString();

                Setter(newValue);
            });
            Slider = slider;
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

        public void Reset()
        {
            var newValue = Getter();
            ValueLabel.Value = newValue.ToString();

            float diff = Max -  Min;

            float vDiff = newValue - Min;
            Slider.Value = (vDiff) / diff;

        }


    }
}
