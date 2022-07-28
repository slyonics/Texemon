using Microsoft.Xna.Framework;
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
        private AnimatedSprite sprite;
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
            base.LoadAttributes(xmlNode);

            foreach (XmlAttribute xmlAttribute in xmlNode.Attributes)
            {
                string[] tokens;
                switch (xmlAttribute.Name)
                {
                    case "Icon": icon = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Widgets_Icons_" + xmlAttribute.Value)]; break;
                    case "Sprite": sprite = LoadSprite(xmlAttribute.Value); break;
                    case "Picture": picture = LoadPicture(xmlAttribute.Value); break;
                    case "SpriteScale":
                        SpriteScale = ParseInt(xmlAttribute.Value);
                        break;

                    case "SpriteBinding":
                        spriteBinding = LookupBinding<AnimatedSprite>(xmlAttribute.Value);
                        spriteBinding.ModelChanged += SpriteBinding_ModelChanged;
                        SpriteBinding_ModelChanged();
                        break;

                    case "PictureBinding":
                        pictureBinding = LookupBinding<string>(xmlAttribute.Value);
                        pictureBinding.ModelChanged += PictureBinding_ModelChanged;
                        PictureBinding_ModelChanged();
                        break;

                    case "RenderBinding":
                        renderBinding = LookupBinding<RenderTarget2D>(xmlAttribute.Value);
                        renderBinding.ModelChanged += RenderBinding_ModelChanged;
                        RenderBinding_ModelChanged();
                        break;
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
            sprite = (AnimatedSprite)spriteBinding.Value;
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

            sprite?.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (icon != null)
            {
                spriteBatch.Draw(icon, new Vector2(currentWindow.Center.X - icon.Width / 2, currentWindow.Center.Y - icon.Height / 2) + Position, null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, depth - 0.0001f);
            }
            else if (picture != null)
            {
                if (alignment == Alignment.Bottom)
                    spriteBatch.Draw(picture, new Rectangle(currentWindow.Left + (int)Position.X, (int)Position.Y + currentWindow.Top, currentWindow.Width, currentWindow.Height), null, color, 0.0f, Vector2.Zero, SpriteEffects.None, depth - 0.0001f);
                // spriteBatch.Draw(picture, new Rectangle(currentWindow.Left + (int)Position.X, -currentWindow.Height + (int)Position.Y + parent.InnerBounds.Height / 2 - parent.InnerMargin.Y * CrossPlatformGame.Scale, currentWindow.Width, currentWindow.Height), null, color, 0.0f, Vector2.Zero, SpriteEffects.None, depth - 0.0001f);
                else spriteBatch.Draw(picture, new Rectangle(currentWindow.X + (int)Position.X, currentWindow.Y + (int)Position.Y, currentWindow.Width, currentWindow.Height), null, color, 0.0f, Vector2.Zero, SpriteEffects.None, depth - 0.0001f);
            }
            else if (sprite != null)
            {
                sprite.Scale = new Vector2(SpriteScale);

                switch (alignment)
                {
                    case Alignment.BottomRight:
                        sprite?.Draw(spriteBatch, new Vector2(currentWindow.Center.X - (sprite.SpriteBounds().Width * SpriteScale) / 2, currentWindow.Center.Y) + Position, null, depth - 0.0001f);
                        break;

                    case Alignment.Center:
                        sprite?.Draw(spriteBatch, new Vector2(currentWindow.Center.X, currentWindow.Center.Y + bounds.Y) + Position, null, depth - 0.0001f);
                        break;

                    case Alignment.Bottom:
                        sprite?.Draw(spriteBatch, new Vector2(currentWindow.Center.X, currentWindow.Bottom - parent.InnerMargin.Height) + Position, null, depth - 0.0001f);
                        break;

                    default:
                        sprite?.Draw(spriteBatch, new Vector2(currentWindow.Center.X - (sprite.SpriteBounds().Width * SpriteScale) / 2, currentWindow.Center.Y - (sprite.SpriteBounds().Height * SpriteScale) / 2) + Position, null, depth - 0.0001f);
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

        public AnimatedSprite AnimatedSprite { get => sprite; }
    }
}
