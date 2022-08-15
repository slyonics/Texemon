using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection;
using System.Xml;
using Texemon.Models;

namespace Texemon.SceneObjects.Widgets
{
    public class Label : Widget
    {
        public string Text { get; set; }

        private ModelProperty<string> binding;

        private Alignment? textAlignment;

        public Label(Widget iParent, float widgetDepth)
            : base(iParent, widgetDepth)
        {
            textAlignment = Alignment.Center;
        }

        /*
        public Label(DataCell iParent, XmlNode xmlNode, Rectangle iBounds, float widgetDepth)
            : base()
        {
            parent = iParent;
            depth = widgetDepth;

            LoadAttributes(xmlNode);

            currentWindow = bounds = iBounds;
        }
        */

        public override void LoadAttributes(XmlNode xmlNode)
        {
            base.LoadAttributes(xmlNode);

            foreach (XmlAttribute xmlAttribute in xmlNode.Attributes)
            {
                string[] tokens;
                switch (xmlAttribute.Name)
                {
                    case "Text": Text = ExpandText(xmlAttribute.Value); break;
                    case "TextAlignment": textAlignment = (Alignment)Enum.Parse(typeof(Alignment), xmlAttribute.Value); break;
                    case "SoftBinding": Text = LookupSoftBinding<object>(xmlAttribute.Value).ToString(); break;

                    case "Binding":
                        binding = LookupBinding<string>(xmlAttribute.Value);
                        binding.ModelChanged += Binding_ModelChanged;
                        break;
                }
            }

            Binding_ModelChanged();

            if (bounds.Width == 0 && bounds.Height == 0 && bounds.X == 0 && bounds.Y == 0)
            {
                bounds = new Rectangle(0, 0, Main.Text.GetStringLength(Font, Text), Main.Text.GetStringHeight(Font));
            }
        }

        public void Binding_ModelChanged()
        {
            if (binding == null || binding.Value == null) return;


            Text = binding.Value; // String.IsNullOrEmpty(bindingFormat) ? binding.ToString() : binding.ToString(bindingFormat);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            Color drawColor = (parent.Enabled) ? Color : new Color(120, 120, 120, 255);

            switch (textAlignment.HasValue ? textAlignment.Value : Alignment)
            {
                case Alignment.Left:
                    Main.Text.DrawText(spriteBatch, new Vector2(currentWindow.Left, currentWindow.Center.Y - Main.Text.GetStringHeight(Font) / 2) + base.Position, Font, Text, drawColor, Depth);
                    break;

                case Alignment.Center:
                    Main.Text.DrawCenteredText(spriteBatch, new Vector2(currentWindow.Center.X, currentWindow.Center.Y) + base.Position, Font, Text, drawColor, Depth);
                    break;

                case Alignment.Right:
                    Main.Text.DrawText(spriteBatch, new Vector2(currentWindow.Right - Main.Text.GetStringLength(Font, Text), currentWindow.Center.Y - Main.Text.GetStringHeight(Font) / 2) + base.Position, Font, Text, drawColor, Depth);
                    break;
            }
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
