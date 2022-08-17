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
        private const int TOOLTIP_MARGIN_WIDTH = 8;
        private const int TOOLTIP_MARGIN_HEIGHT = 4;

        private string text = "";
        private ModelProperty<string> binding;

        private NinePatch textplateFrame;
        private string style;

        public Textplate(Widget iParent, float widgetDepth)
            : base(iParent, widgetDepth)
        {

        }

        ~Textplate()
        {
            if (binding != null) binding.ModelChanged -= Binding_ModelChanged;
        }

        public override void LoadAttributes(XmlNode xmlNode)
        {
            base.LoadAttributes(xmlNode);

            foreach (XmlAttribute xmlAttribute in xmlNode.Attributes)
            {
                switch (xmlAttribute.Name)
                {
                    case "Text": text = ParseString(xmlAttribute.Value); break;

                    case "Binding":
                        binding = LookupBinding<string>(xmlAttribute.Value);
                        binding.ModelChanged += Binding_ModelChanged;
                        if (binding.Value != null) Binding_ModelChanged();
                        break;

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

            int width = Text.GetStringLength(Font, text) + TOOLTIP_MARGIN_WIDTH * 2; // Math.Max(Text.GetStringLength(font, text) + TOOLTIP_MARGIN_WIDTH * 2, textplateFrame.FrameWidth * 3) + 20;
            int height = Text.GetStringHeight(Font) + TOOLTIP_MARGIN_HEIGHT * 2; //Math.Max(Text.GetStringHeight(font) + TOOLTIP_MARGIN_HEIGHT * 2, textplateFrame.FrameHeight * 3);
            currentWindow.Width = width;
            currentWindow.Height = height;
            //currentWindow.Y -= height;

            textplateFrame.Bounds = currentWindow;
        }

        private void Binding_ModelChanged()
        {
            text = ParseString(binding.ToString());

            int width = Text.GetStringLength(Font, text) + TOOLTIP_MARGIN_WIDTH * 2; // Math.Max(Text.GetStringLength(font, text) + TOOLTIP_MARGIN_WIDTH * 2, textplateFrame.FrameWidth * 3) + 20;
            int height = Text.GetStringHeight(Font) + TOOLTIP_MARGIN_HEIGHT * 2; // Math.Max(Text.GetStringHeight(font) + TOOLTIP_MARGIN_HEIGHT * 2, textplateFrame.FrameHeight * 3);

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
            if (String.IsNullOrEmpty(text)) return;

            base.Draw(spriteBatch);

            textplateFrame.Draw(spriteBatch, Position);
            Text.DrawCenteredText(spriteBatch, new Vector2(currentWindow.Center.X, currentWindow.Center.Y + Text.FONT_DATA[Font].heightOffset) + Position, Font, ParseString(text), Color, 0);
        }

        private string ExpandText(string text)
        {
            int startIndex = text.IndexOf('{');
            int endIndex = text.IndexOf('}');

            while (startIndex != -1 && endIndex > startIndex)
            {
                string originalToken = text.Substring(startIndex, endIndex - startIndex + 1);
                PropertyInfo propertyInfo = GameProfile.PlayerProfile.GetType().GetProperty(originalToken.Substring(1, originalToken.Length - 2));
                string newToken = (propertyInfo.GetValue(GameProfile.PlayerProfile) as ModelProperty<string>).Value;

                text = text.Replace(originalToken, newToken.ToString());

                startIndex = text.IndexOf('{');
                endIndex = text.IndexOf('}');
            }

            return text;
        }

        private string Style { get => style; set { style = value; UpdateFrame(); } }
    }
}
