﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Texemon.Main;
using Texemon.Models;

namespace Texemon.SceneObjects.Widgets
{
    public class Image : Widget
    {
        private Texture2D icon;
        private AnimatedSprite Sprite { get; set; }
        private ModelProperty<AnimatedSprite> spriteBinding;
        private ModelProperty<string> pictureBinding;
        private ModelProperty<RenderTarget2D> renderBinding;
        private Texture2D picture;

        public float SpriteScale { get; set; } = 1.0f;

        public Image(Widget iParent, float widgetDepth)
            : base(iParent, widgetDepth)
        {

        }

        public override void LoadAttributes(XmlNode xmlNode)
        {
            foreach (XmlAttribute xmlAttribute in xmlNode.Attributes)
            {
                string[] tokens;
                switch (xmlAttribute.Name)
                {
                    case "Icon": icon = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Widgets_Icons_" + xmlAttribute.Value)]; break;
                    case "Picture": picture = LoadPicture(xmlAttribute.Value); break;
                    case "SpriteScale":
                        SpriteScale = ParseInt(xmlAttribute.Value);
                        break;

                    case "SpriteBinding":
                        spriteBinding = OldLookupBinding<AnimatedSprite>(xmlAttribute.Value);
                        spriteBinding.ModelChanged += SpriteBinding_ModelChanged;
                        SpriteBinding_ModelChanged();
                        break;

                    case "PictureBinding":
                        pictureBinding = OldLookupBinding<string>(xmlAttribute.Value);
                        pictureBinding.ModelChanged += PictureBinding_ModelChanged;
                        PictureBinding_ModelChanged();
                        break;

                    case "RenderBinding":
                        renderBinding = OldLookupBinding<RenderTarget2D>(xmlAttribute.Value);
                        renderBinding.ModelChanged += RenderBinding_ModelChanged;
                        RenderBinding_ModelChanged();
                        break;

                    default: ParseAttribute(xmlAttribute.Name, xmlAttribute.Value); break;
                }
            }
        }

        public override void ApplyAlignment()
        {
            base.ApplyAlignment();
        }

        private Texture2D LoadPicture(string path)
        {
            return AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), path)];
        }



        private AnimatedSprite LoadSprite(string parameters)
        {
            string[] tokens = parameters.Split(',');
            Texture2D sprite = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), tokens[0].Replace("\\", "_"))];
            Dictionary<string, Animation> animationList = new Dictionary<string, Animation>()
            {
                { "loop", new Animation(int.Parse(tokens[1]), int.Parse(tokens[2]), int.Parse(tokens[3]), int.Parse(tokens[4]), int.Parse(tokens[5]), int.Parse(tokens[6]), int.Parse(tokens[7])) }
            };

            var result = new AnimatedSprite(sprite, animationList);
            result.PlayAnimation("loop");

            return result;
        }

        private void SpriteBinding_ModelChanged()
        {
            Sprite = (AnimatedSprite)spriteBinding.Value;
        }

        private void PictureBinding_ModelChanged()
        {
            picture = LoadPicture(pictureBinding.Value);
        }

        private void RenderBinding_ModelChanged()
        {
            picture = renderBinding.Value;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Sprite?.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (icon != null)
            {
                spriteBatch.Draw(icon, new Vector2(currentWindow.Center.X - icon.Width / 2, currentWindow.Center.Y - icon.Height / 2) + Position, null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, Depth - 0.0001f);
            }
            else if (picture != null)
            {
                if (Alignment == Alignment.Bottom)
                    spriteBatch.Draw(picture, new Rectangle(currentWindow.Left + (int)Position.X, (int)Position.Y + currentWindow.Top, currentWindow.Width, currentWindow.Height), null, Color, 0.0f, Vector2.Zero, SpriteEffects.None, Depth - 0.0001f);
                // spriteBatch.Draw(picture, new Rectangle(currentWindow.Left + (int)Position.X, -currentWindow.Height + (int)Position.Y + parent.InnerBounds.Height / 2 - parent.InnerMargin.Y * CrossPlatformGame.Scale, currentWindow.Width, currentWindow.Height), null, color, 0.0f, Vector2.Zero, SpriteEffects.None, depth - 0.0001f);
                else spriteBatch.Draw(picture, new Rectangle(currentWindow.X + (int)Position.X, currentWindow.Y + (int)Position.Y, currentWindow.Width, currentWindow.Height), null, Color, 0.0f, Vector2.Zero, SpriteEffects.None, Depth - 0.0001f);
            }
            else if (Sprite != null)
            {
                Sprite.Scale = new Vector2(SpriteScale);

                switch (Alignment)
                {
                    case Alignment.BottomRight:
                        Sprite?.Draw(spriteBatch, new Vector2(currentWindow.Center.X - (Sprite.SpriteBounds().Width * SpriteScale) / 2, currentWindow.Center.Y) + Position, null, Depth - 0.0001f);
                        break;

                    case Alignment.Center:
                        Sprite?.Draw(spriteBatch, new Vector2(currentWindow.Center.X, currentWindow.Center.Y + bounds.Y) + Position, null, Depth - 0.0001f);
                        break;

                    case Alignment.Bottom:
                        Sprite?.Draw(spriteBatch, new Vector2(currentWindow.Center.X, currentWindow.Bottom - parent.InnerMargin.Height) + Position, null, Depth - 0.0001f);
                        break;

                    default:
                        Sprite?.Draw(spriteBatch, new Vector2(currentWindow.Center.X - (Sprite.SpriteBounds().Width * SpriteScale) / 2, currentWindow.Center.Y - (Sprite.SpriteBounds().Height * SpriteScale) / 2) + Position, null, Depth - 0.0001f);
                        break;
                }
            }
        }

        public override void EndLeftClick(Vector2 mouseStart, Vector2 mouseEnd, Widget otherWidget)
        {
            base.EndLeftClick(mouseStart, mouseEnd, otherWidget);

            if (otherWidget == this)
            {
                GetParent<ViewModel>().LeftClickChild(mouseStart, mouseEnd, this, otherWidget);
            }
        }

        public AnimatedSprite AnimatedSprite { get => Sprite; }
    }
}
