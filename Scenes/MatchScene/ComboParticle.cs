using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texemon.SceneObjects.Particles;

namespace Texemon.Scenes.MatchScene
{
    public class ComboParticle : Overlay
    {
        private NinePatch window;
        private string message;
        private int yLimit;
        private int decayTime = 500;

        int width;
        int height;
        private Vector2 position;
        private Vector2 velocity;

        public ComboParticle(Scene iScene, Vector2 iPosition, string iMessage, GameSprite gameSprite)
        {
            Texture2D sprite = AssetCache.SPRITES[gameSprite];
            position = iPosition;
            message = iMessage;

            GameFont font = (CrossPlatformGame.Scale == 2) ? GameFont.BigCombo : GameFont.Combo;

            if (message.Length == 0)
            {
                width = sprite.Width;
                height = 34;
            }
            else
            {
                width = Text.GetStringLength(font, message) + 8 * CrossPlatformGame.Scale;
                height = Text.GetStringHeight(font) + 1 * CrossPlatformGame.Scale;
            }

            window = new NinePatch(sprite, 0.03f);
            window.Bounds = new Rectangle((int)position.X, (int)position.Y, width, height);

            yLimit = window.Bounds.Top - MatchTile.TILE_SIZE;

            velocity = new Vector2(0, -50.0f * CrossPlatformGame.Scale);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            position += velocity * gameTime.ElapsedGameTime.Milliseconds / 1000.0f;

            window.Bounds = new Rectangle((int)position.X, (int)position.Y, width, height);
            if (position.Y < yLimit)
            {
                velocity = Vector2.Zero;
                decayTime -= gameTime.ElapsedGameTime.Milliseconds;
                if (decayTime <= 0) Terminate();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            window.Draw(spriteBatch, Vector2.Zero);

            GameFont font = (CrossPlatformGame.Scale == 2) ? GameFont.BigCombo : GameFont.Combo;
            Text.DrawCenteredText(spriteBatch, new Vector2((window.Bounds.Left + window.Bounds.Right) / 2, (window.Bounds.Bottom + window.Bounds.Top) / 2 + 3 * CrossPlatformGame.Scale), font, message, 0.01f);
        }
    }
}
