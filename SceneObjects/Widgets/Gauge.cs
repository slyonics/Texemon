using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Texemon.Main;
using Texemon.Models;

namespace Texemon.SceneObjects.Widgets
{
    public class Gauge : Widget
    {
        private string frame;
        private string background;
        private NinePatch gaugeFrame;
        private NinePatch gaugeBackground;

        private int height;

        public float Minimum { get; private set; } = 0;
        public float Maximum { get; private set; } = 100;

        private ModelProperty<float> minimumBinding;
        private ModelProperty<float> maximumBinding;

        public Gauge(Widget iParent, float widgetDepth)
            : base(iParent, widgetDepth)
        {

        }

        public override void LoadAttributes(XmlNode xmlNode)
        {
            base.LoadAttributes(xmlNode);

            foreach (XmlAttribute xmlAttribute in xmlNode.Attributes)
            {
                string[] tokens;
                switch (xmlAttribute.Name)
                {
                    case "Frame": frame = "Gauges_" + xmlAttribute.Value; break;
                    case "Background": background = "Gauges_" + xmlAttribute.Value; break;
                    case "Minimum": Minimum = int.Parse(xmlAttribute.Value); break;
                    case "Maximum": Maximum = int.Parse(xmlAttribute.Value); break;
                    case "Height": height = int.Parse(xmlAttribute.Value); break;

                    case "MinimumBinding":
                        minimumBinding = OldLookupBinding<float>(xmlAttribute.Value);
                        minimumBinding.ModelChanged += MinimumBinding_ModelChanged;
                        Minimum = (float)minimumBinding.Value;
                        break;

                    case "MaximumBinding":
                        maximumBinding = OldLookupBinding<float>(xmlAttribute.Value);
                        maximumBinding.ModelChanged += MaximumBinding_ModelChanged;
                        Maximum = (float)maximumBinding.Value;
                        break;
                }
            }
        }

        public override void ApplyAlignment()
        {
            base.ApplyAlignment();

            gaugeBackground = new NinePatch(background, Depth);
            gaugeBackground.Bounds = currentWindow;

            gaugeFrame = new NinePatch(frame, Depth - 0.01f);
            gaugeFrame.Bounds = currentWindow;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            gaugeBackground?.Draw(spriteBatch, Position);
            gaugeFrame?.Draw(spriteBatch, Position);
        }

        public void MinimumBinding_ModelChanged()
        {
            Minimum = (int)minimumBinding.Value;
        }

        public void MaximumBinding_ModelChanged()
        {
            Maximum = (int)maximumBinding.Value;
        }
    }

    public class GaugeBar : Widget
    {
        private Gauge parentGauge;

        private float barValue;
        private string background;
        private NinePatch gaugeBackground;

        private ModelProperty<float> binding;

        public GaugeBar(Gauge iParent, float widgetDepth)
            : base(iParent, widgetDepth)
        {
            parentGauge = iParent;
        }

        public override void LoadAttributes(XmlNode xmlNode)
        {
            base.LoadAttributes(xmlNode);

            foreach (XmlAttribute xmlAttribute in xmlNode.Attributes)
            {
                string[] tokens;
                switch (xmlAttribute.Name)
                {
                    case "Value": barValue = float.Parse(xmlAttribute.Value); break;
                    case "Bar": background = "Gauges_" + xmlAttribute.Value; break;

                    case "Binding":
                        binding = OldLookupBinding<float>(xmlAttribute.Value);
                        binding.ModelChanged += Binding_ModelChanged;
                        break;
                }
            }

            gaugeBackground = new NinePatch(background, Depth);

            if (binding != null)
            {
                barValue = Convert.ToSingle(binding.Value);
                ApplyAlignment();
            }
        }

        public void Binding_ModelChanged()
        {
            barValue = Convert.ToSingle(binding.Value);
            ApplyAlignment();
        }

        public override void ApplyAlignment()
        {
            parentGauge = parent as Gauge;
            int barWidth = (int)(barValue / parentGauge.Maximum * parentGauge.InnerBounds.Width);
            currentWindow = bounds = new Rectangle(parentGauge.InnerBounds.Left, parentGauge.InnerBounds.Top, parentGauge.InnerBounds.Width, parentGauge.InnerBounds.Height);

            gaugeBackground.Bounds = new Rectangle(currentWindow.Left, currentWindow.Top, barWidth, currentWindow.Height);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            gaugeBackground?.Draw(spriteBatch, Position);
        }

        public float Value
        {
            get => barValue;
            set
            {
                if (binding == null) barValue = value;
                else
                {
                    binding.Value = value;
                    Binding_ModelChanged();
                }
            }
        }
    }

    public class GaugeSlider : Widget
    {
        private Gauge parentGauge;
        private GaugeBar parentGaugeBar;

        private string slider;
        private NinePatch sliderBackground;

        private bool dragging;
        private float leftX;
        private float rightX;

        public GaugeSlider(GaugeBar iParentGaugeBar, float widgetDepth)
            : base(iParentGaugeBar.GetParent<Gauge>(), widgetDepth)
        {
            parentGauge = iParentGaugeBar.GetParent<Gauge>();
            parentGaugeBar = iParentGaugeBar;

            ApplyAlignment();
        }

        public override void LoadAttributes(XmlNode xmlNode)
        {
            base.LoadAttributes(xmlNode);

            foreach (XmlAttribute xmlAttribute in xmlNode.Attributes)
            {
                string[] tokens;
                switch (xmlAttribute.Name)
                {
                    case "Slider": slider = "Gauges_" + xmlAttribute.Value; break;
                }
            }

            if (slider != null) sliderBackground = new NinePatch(slider, Depth - 0.001f, true);
        }

        public override void ApplyAlignment()
        {
            if (parentGaugeBar == null) return;

            if (slider != null)
            {
                int sliderWidth = sliderBackground.Sprite.Width;
                int barWidth = (int)(parentGaugeBar.Value / parentGauge.Maximum * (parentGauge.InnerBounds.Width));

                Rectangle roughBounds = new Rectangle(parentGauge.InnerBounds.Left + barWidth - sliderWidth / 2, parentGauge.InnerBounds.Top, sliderWidth, parentGauge.InnerBounds.Height);
                //roughBounds.X = parentGauge.InnerBounds.Left + barWidth - 48 + sliderWidth / 2 + 12;
                if (roughBounds.X > parent.InnerBounds.Right - sliderWidth) roughBounds.X = parent.InnerBounds.Right - sliderWidth;
                if (roughBounds.X < parent.InnerBounds.Left) roughBounds.X = parent.InnerBounds.Left;
                currentWindow = bounds = sliderBackground.Bounds = roughBounds;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (dragging)
            {
                if (Input.MousePosition.X <= leftX) parentGaugeBar.Value = parentGauge.Minimum;
                else if (Input.MousePosition.X >= rightX) parentGaugeBar.Value = parentGauge.Maximum;
                else parentGaugeBar.Value = MathHelper.Lerp(parentGauge.Minimum, parentGauge.Maximum, (Input.MousePosition.X - leftX) / (rightX - leftX));

                ApplyAlignment();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            sliderBackground?.Draw(spriteBatch, Position);
        }

        public override void StartLeftClick(Vector2 mousePosition)
        {
            if (slider == null) return;

            dragging = true;

            float interval = (parentGaugeBar.Value - parentGauge.Minimum) / (parentGauge.Maximum - parentGauge.Minimum);
            int barWidth = parentGauge.InnerBounds.Width;
            leftX = mousePosition.X - (barWidth * interval);
            rightX = mousePosition.X + (barWidth * (1.0f - interval));
        }

        public override void EndLeftClick(Vector2 mouseStart, Vector2 mouseEnd, Widget otherWidget)
        {
            if (slider == null) return;

            dragging = false;
        }
    }
}
