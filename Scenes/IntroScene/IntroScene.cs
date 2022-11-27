using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;
using Texemon.Models;

namespace Texemon.Scenes.IntroScene
{
    public class IntroScene : Scene
    {

        private Texture2D backgroundColorSprite = AssetCache.SPRITES[GameSprite.Background_Blank];

        private SelectionViewModel selectionViewModel;

        Color backgroundColor = new Color(63, 61, 63);

        public IntroScene()
            : base()
        {
            GameProfile.NewState();

            selectionViewModel = AddView(new SelectionViewModel(this, GameView.IntroScene_SelectionView));

            selectionViewModel.Autostart();
        }

        public override void BeginScene()
        {
            base.BeginScene();

            //Audio.PlayMusic(GameMusic.Selection);
        }

        public override void DrawBackground(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(backgroundColorSprite, new Rectangle(0, 0, CrossPlatformGame.ScreenWidth, CrossPlatformGame.ScreenHeight), new Rectangle(0, 0, 1, 1), backgroundColor, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
        }
    }
}
