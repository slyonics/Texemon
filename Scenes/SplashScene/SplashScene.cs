﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texemon.Main;
using Texemon.SceneObjects.Controllers;

namespace Texemon.Scenes.SplashScene
{
    public class SplashScene : Scene, ISkippableWait
    {
        private Texture2D splashSprite = AssetCache.SPRITES[GameSprite.Background_Splash];

        public SplashScene()
            : base()
        {
            AddController(new SkippableWaitController(PriorityLevel.MenuLevel, this));
        }

        public override void Update(GameTime gameTime, PriorityLevel priorityLevel = PriorityLevel.GameLevel)
        {
            base.Update(gameTime);
        }

        public override void DrawBackground(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(splashSprite, new Rectangle(0, 0, CrossPlatformGame.ScreenWidth, CrossPlatformGame.ScreenHeight), new Rectangle(0, 0, 1, 1), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            spriteBatch.Draw(splashSprite, new Rectangle((CrossPlatformGame.ScreenWidth - splashSprite.Width) / 2, (CrossPlatformGame.ScreenHeight - splashSprite.Height) / 2, splashSprite.Width, splashSprite.Height), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
        }

        public void Notify(SkippableWaitController sender)
        {
            CrossPlatformGame.Transition(typeof(TitleScene.TitleScene));
        }

        public bool Terminated { get => false; }
    }
}
