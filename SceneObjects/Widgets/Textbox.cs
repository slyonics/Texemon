using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using Texemon.Main;
using Texemon.Models;

namespace Texemon.SceneObjects.Widgets
{
    public class Textbox : Widget
    {
        private ModelProperty<string> binding;
        private string text;

        private NinePatch textboxFrame;
        private string style;
        private string activeStyle;
        private bool active;

        private bool blinkCarat;
        private int blinkTime;

        public Textbox(Widget iParent, float widgetDepth)
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
                    case "Style": style = "Buttons_" + xmlAttribute.Value; break;
                    case "ActiveStyle": activeStyle = "Buttons_" + xmlAttribute.Value; break;

                    case "Binding":
                        binding = LookupBinding<string>(xmlAttribute.Value);
                        binding.ModelChanged += Binding_ModelChanged;
                        break;
                }
            }

            textboxFrame = new NinePatch(style, depth);

            Binding_ModelChanged();
        }

        public void Binding_ModelChanged()
        {
            if (binding == null || binding.Value == null) return;

            text = binding.Value;
        }

        public override void ApplyAlignment()
        {
            base.ApplyAlignment();

            textboxFrame.Bounds = currentWindow;
        }

        public override void Update(GameTime gameTime)
        {
            blinkTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            blinkTime %= 1000;

            if (active)
            {
                var key = Input.CurrentInput.GetKey();
                if (key != Microsoft.Xna.Framework.Input.Keys.None)
                {
                    bool shift = (Input.CurrentInput.KeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) || Input.CurrentInput.KeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift));
                    char keyChar = (char)(key - Microsoft.Xna.Framework.Input.Keys.A + ((shift) ? 'A' : 'a'));

                    if (Text.GetStringLength(font, text + keyChar + '_') <= InnerBounds.Width)
                    {
                        if (binding == null)
                        {
                            text += keyChar;
                        }
                        else
                        {
                            string boundText = binding.Value as string;

                            binding.Value = boundText + keyChar;
                        }
                    }

                    blinkTime = 0;
                }
                else if (Input.CurrentInput.KeyPressed(Microsoft.Xna.Framework.Input.Keys.Back) && text.Length > 0)
                {
                    if (binding == null) text = text.Substring(0, text.Length - 1);
                    else
                    {
                        string boundText = binding.Value as string;
                        binding.Value = boundText.Substring(0, boundText.Length - 1);
                    }

                    blinkTime = 0;
                }
                else if (Input.CurrentInput.KeyPressed(Microsoft.Xna.Framework.Input.Keys.Enter))
                {
                    active = false;

                    if (active) textboxFrame.SetSprite(activeStyle);
                    else textboxFrame.SetSprite(style);
                }
            }


            blinkCarat = blinkTime < 700;

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            textboxFrame?.Draw(spriteBatch, Position);

            Color drawColor = (parent.Enabled) ? color : new Color(160, 160, 160, 255);

            Text.DrawText(spriteBatch, new Vector2(currentWindow.Left + innerMargin.Left, currentWindow.Center.Y - Text.GetStringHeight(font) / 2) + Position, font, text, drawColor);

            if (active && blinkCarat)
            {
                Text.DrawText(spriteBatch, new Vector2(currentWindow.Left + innerMargin.Left + Text.GetStringLength(font, text), currentWindow.Center.Y - Text.GetStringHeight(font) / 2) + Position, font, "_", drawColor);
            }
        }

        public override void StartLeftClick(Vector2 mousePosition)
        {
            Audio.PlaySound(GameSound.menu_select);

            active = !active;

            if (active) textboxFrame.SetSprite(activeStyle);
            else textboxFrame.SetSprite(style);
        }

        public override void EndLeftClick(Vector2 mouseStart, Vector2 mouseEnd, Widget otherWidget)
        {
            if (otherWidget == this) active = true;
            else active = false;

            if (active) textboxFrame.SetSprite(activeStyle);
            else textboxFrame.SetSprite(style);
        }

        public override void LoseFocus()
        {
            active = false;

            textboxFrame.SetSprite(style);
        }
    }
}
