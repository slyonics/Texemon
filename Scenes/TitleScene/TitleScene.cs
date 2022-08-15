using Microsoft.Xna.Framework.Graphics;
using System;
using Texemon.Main;

namespace Texemon.Scenes.TitleScene
{
    public class TitleScene : Scene
    {
        private Texture2D backgroundColorSprite = AssetCache.SPRITES[GameSprite.Background_Splash];
        private Texture2D ponsonaSprite = AssetCache.SPRITES[GameSprite.Background_Title];

        private TitleViewModel titleMenuViewModel;

        public TitleScene()
            : base()
        {
            titleMenuViewModel = AddView<TitleViewModel>(new TitleViewModel(this, GameView.TitleScene_TitleView));
        }

        public override void BeginScene()
        {
            base.BeginScene();

            Audio.PlayMusic(GameMusic.SMP_TTL);
        }

        public override void DrawBackground(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(backgroundColorSprite, new Rectangle(0, 0, CrossPlatformGame.ScreenWidth, CrossPlatformGame.ScreenHeight), new Rectangle(0, 0, 1, 1), Color.Black, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);

            spriteBatch.Draw(ponsonaSprite, new Vector2(CrossPlatformGame.ScreenWidth / 2, CrossPlatformGame.ScreenHeight / 2) - new Vector2(0, 145), null, Color.White, 0.0f, new Vector2(ponsonaSprite.Width / 2, ponsonaSprite.Height / 2), 1, SpriteEffects.None, 0.5f);
        }

        public void ResetSettings()
        {
            titleMenuViewModel.Terminate();
            titleMenuViewModel = new TitleViewModel(this, GameView.TitleScene_TitleView);
            AddOverlay(titleMenuViewModel);
            titleMenuViewModel.SettingsMenu();
        }
    }
}
