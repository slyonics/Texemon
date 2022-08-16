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

        private ModelProperty<string> styleBinding;

        private NinePatch textplateFrame;

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
                string[] tokens;
                switch (xmlAttribute.Name)
                {
                    case "Style": textplateFrame = new NinePatch("Textplate_" + xmlAttribute.Value, Depth); break;
                    case "Text": text = ParseString(xmlAttribute.Value); break;

                    case "StyleBinding":
                        styleBinding = LookupBinding<string>(xmlAttribute.Value);
                        styleBinding.ModelChanged += StyleBinding_ModelChanged;
                        if (styleBinding.Value != null) StyleBinding_ModelChanged();
                        break;

                    case "Binding":
                        binding = LookupBinding<string>(xmlAttribute.Value);
                        binding.ModelChanged += Binding_ModelChanged;
                        if (binding.Value != null) Binding_ModelChanged();
                        break;
                }
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

        private void StyleBinding_ModelChanged()
        {
            textplateFrame = new NinePatch("Textplate_" + styleBinding.Value, Depth);
            textplateFrame.Bounds = currentWindow;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (String.IsNullOrEmpty(text)) return;

            base.Draw(spriteBatch);

            textplateFrame.Draw(spriteBatch, Position);
            Text.DrawCenteredText(spriteBatch, new Vector2(currentWindow.Center.X, currentWindow.Center.Y ) + Position, Font, ParseString(text), Color, 0);
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
    }
}
