using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Texemon.Models;

namespace Texemon.SceneObjects.Widgets
{
    public class CrawlText : Widget
    {
        private class TextElement
        {
            public int line;
            public int offset;
            public Color color = Color.White;
            public StringBuilder text = new StringBuilder();
        }

        private const int TEXT_QUEUE_COOLDOWN = 10;
        private const int TALK_COOLDOWN = 100;

        private ModelProperty<string> binding;
        private ModelProperty<string> voiceBinding;

        private string textContent = null;
        private List<TextElement> textQueue = new List<TextElement>();
        private List<TextElement> textLines = new List<TextElement>();

        private int textQueueTimer = 0;
        private int maxTextLength = 0;
        private int crawlFactor = 1;

        private int talkTimer = 0;
        private GameSound talkSound = GameSound.dialogue_auto_scroll;

        public CrawlText(Widget iParent, float widgetDepth)
            : base(iParent, widgetDepth)
        {

        }

        ~CrawlText()
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
                    case "Text": textContent = xmlAttribute.Value; break;
                    case "VoiceSound": talkSound = string.IsNullOrEmpty(xmlAttribute.Value) ? GameSound.None : (GameSound)Enum.Parse(typeof(GameSound), xmlAttribute.Value); break;

                    case "Binding":
                        binding = LookupBinding<string>(xmlAttribute.Value);
                        binding.ModelChanged += Binding_ModelChanged;
                        if (binding.Value != null) textContent = binding.Value.ToString();
                        break;

                    case "VoiceBinding":
                        voiceBinding = LookupBinding<string>(xmlAttribute.Value);
                        voiceBinding.ModelChanged += VoiceBinding_ModelChanged;
                        if (voiceBinding.Value != null) VoiceBinding_ModelChanged();
                        break;
                }
            }
        }

        private void Binding_ModelChanged()
        {
            AddLines(binding.Value.ToString());
        }

        private void VoiceBinding_ModelChanged()
        {
            talkSound = string.IsNullOrEmpty(voiceBinding.Value as string) ? GameSound.None : (GameSound)Enum.Parse(typeof(GameSound), (string)voiceBinding.Value);
        }

        public override void ApplyAlignment()
        {
            base.ApplyAlignment();

            maxTextLength = currentWindow.Width;

            if (!String.IsNullOrEmpty(textContent))
            {
                AddLines(textContent);
            }
        }

        public void AddLines(string text)
        {
            if (string.IsNullOrEmpty(text)) talkTimer = 10000;
            else talkTimer = 0;

            textLines.Clear();
            textQueueTimer = 0;

            textQueue = GetTextLines(ExpandText(text), maxTextLength);
            textLines.Add(new TextElement());
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!Transitioning)
            {
                UpdateText(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (!Transitioning)
            {
                foreach (TextElement textElement in textLines)
                {
                    Text.DrawText(spriteBatch, Position + new Vector2(currentWindow.X + textElement.offset, currentWindow.Y), font, textElement.text.ToString(), textElement.color, textElement.line);
                }
            }
        }

        private List<TextElement> ExpandText(string text)
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

            List<TextElement> result = new List<TextElement>();
            string[] tokens = text.Split(' ');
            foreach (string token in tokens)
            {
                TextElement textElement = new TextElement() { color = Color.White };
                textElement.text.Append(token);
                result.Add(textElement);
            }

            return result;
        }

        private void UpdateText(GameTime gameTime)
        {
            if (!ReadyToProceed)
            {
                textQueueTimer -= gameTime.ElapsedGameTime.Milliseconds * crawlFactor;

                while (textQueueTimer <= 0 && !ReadyToProceed)
                {
                    textQueueTimer += TEXT_QUEUE_COOLDOWN;

                    talkTimer -= gameTime.ElapsedGameTime.Milliseconds;
                    if (talkTimer <= 0)
                    {
                        if (talkSound != GameSound.None) Audio.PlaySound(talkSound);
                        talkTimer += TALK_COOLDOWN;
                    }

                    AdvanceCharacter();
                }
            }
        }

        public void FinishText()
        {
            while (!ReadyToProceed)
            {
                AdvanceCharacter();
            }
        }

        private void AdvanceCharacter()
        {
            TextElement queueElement = textQueue.First();
            TextElement textElement = textLines.Last();

            if (queueElement.text.Length == 0)
            {
                textQueue.Remove(queueElement);
                queueElement = textQueue.FirstOrDefault();

                textElement = new TextElement() { line = queueElement.line, color = queueElement.color, offset = queueElement.offset };
                textLines.Add(textElement);
            }

            if (queueElement.text.Length > 0)
            {
                textElement.text.Append(queueElement.text[0]);
                queueElement.text.Remove(0, 1);
            }
        }

        private List<TextElement> GetTextLines(List<TextElement> textElements, int windowWidth)
        {
            List<TextElement> textLines = new List<TextElement>();
            textLines.Add(new TextElement());

            int currentLine = 0;
            int currentLength = 0;
            Color currentColor = Color.White;

            foreach (TextElement textElement in textElements)
            {
                if (textElement.text.Length > 0 && textElement.text[0] == '#')
                {
                    currentColor = Graphics.ParseHexcode(textElement.text.ToString());
                    textLines.Add(new TextElement() { line = currentLine, color = currentColor, offset = currentLength });
                    continue;
                }
                else if (textElement.text.ToString() == "[n]")
                {
                    currentLine++;
                    currentLength = 0;
                    textLines.Add(new TextElement() { line = currentLine, color = currentColor });
                    continue;
                }

                int tokenLength = Text.GetStringLength(font, textElement.text.ToString());
                if (currentLength + tokenLength > windowWidth)
                {
                    currentLine++;
                    currentLength = 0;
                    textLines.Add(new TextElement() { line = currentLine, color = currentColor });
                }

                StringBuilder lastElement = textLines.LastOrDefault(x => x.line == currentLine)?.text;
                lastElement?.Append(textElement.text + " ");

                currentLength += tokenLength + Text.GetStringLength(font, " ");
            }

            return textLines;
        }

        public override void EndLeftClick(Vector2 mouseStart, Vector2 mouseEnd, Widget otherWidget)
        {
            base.EndLeftClick(mouseStart, mouseEnd, otherWidget);

            if (otherWidget == this)
            {
                GetParent<ViewModel>().LeftClickChild(mouseStart, mouseEnd, this, otherWidget);
            }
        }

        public bool ReadyToProceed { get => textQueue.Count == 0 || textQueue.LastOrDefault().text.Length == 0; }
        public int CrawlFactor { set => crawlFactor = value; }
    }
}
