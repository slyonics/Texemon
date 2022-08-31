using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Texemon.Main;
using Texemon.Models;

namespace Texemon.SceneObjects.Widgets
{
    public class Textplate : Widget
    {
        private string text = "";
        private string Text { get => text; set { text = value; ResizeTextplate(); } }

        private NinePatch textplateFrame;
        private string style;
        private string Style { get => style; set { style = value; UpdateFrame(); } }

        public Textplate(Widget iParent, float widgetDepth)
            : base(iParent, widgetDepth)
        {

        }

        public override void LoadAttributes(XmlNode xmlNode)
        {
            foreach (XmlAttribute xmlAttribute in xmlNode.Attributes)
            {
                switch (xmlAttribute.Name)
                {
                    default: ParseAttribute(xmlAttribute.Name, xmlAttribute.Value); break;
                }
            }

            UpdateFrame();
        }

        public void UpdateFrame()
        {
            if (style != null)
            {
                if (textplateFrame == null) textplateFrame = new NinePatch(style, Depth);
                textplateFrame.SetSprite(style);
            }
        }

        public override void ApplyAlignment()
        {
            base.ApplyAlignment();

            int width = Main.Text.GetStringLength(Font, Text) + InnerMargin.X + InnerMargin.Width; // Math.Max(Text.GetStringLength(font, text) + TOOLTIP_MARGIN_WIDTH * 2, textplateFrame.FrameWidth * 3) + 20;
            int height = Main.Text.GetStringHeight(Font) + InnerMargin.Y + InnerMargin.Height; //Math.Max(Text.GetStringHeight(font) + TOOLTIP_MARGIN_HEIGHT * 2, textplateFrame.FrameHeight * 3);

            if (Alignment == Alignment.Center)
            {
                currentWindow.Width = width;
                currentWindow.Height = height;

                base.ApplyAlignment();

                currentWindow.X -= width / 2;
                currentWindow.Y -= height / 2;
                currentWindow.Width = width;
                currentWindow.Height = height;
            }
            else
            {
                currentWindow.Width = width;
                currentWindow.Height = height;
            }

            textplateFrame.Bounds = currentWindow;
        }

        private void ResizeTextplate()
        {
            int width = Main.Text.GetStringLength(Font, Text) + InnerMargin.X + InnerMargin.Width; // Math.Max(Text.GetStringLength(font, text) + TOOLTIP_MARGIN_WIDTH * 2, textplateFrame.FrameWidth * 3) + 20;
            int height = Main.Text.GetStringHeight(Font) + InnerMargin.Y + InnerMargin.Height; // Math.Max(Text.GetStringHeight(font) + TOOLTIP_MARGIN_HEIGHT * 2, textplateFrame.FrameHeight * 3);

            if (Alignment == Alignment.Center)
            {
                currentWindow.Width = width;
                currentWindow.Height = height;

                base.ApplyAlignment();

                currentWindow.X -= width / 2;
                currentWindow.Y -= height / 2;
                currentWindow.Width = width;
                currentWindow.Height = height;
            }
            else
            {
                currentWindow.Width = width;
                currentWindow.Height = height;
            }

            textplateFrame.Bounds = currentWindow;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (String.IsNullOrEmpty(Text)) return;

            base.Draw(spriteBatch);

            textplateFrame.Draw(spriteBatch, Position);
            Main.Text.DrawCenteredText(spriteBatch, new Vector2(InnerBounds.Center.X, InnerBounds.Center.Y + Main.Text.FONT_DATA[Font].heightOffset) + Position, Font, base.ParseString(Text), Color, 0);
        }
    }
}
