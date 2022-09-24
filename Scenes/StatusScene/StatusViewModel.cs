using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

using Texemon.Models;
using Texemon.SceneObjects.Widgets;

namespace Texemon.Scenes.StatusScene
{
    public class StatusViewModel : ViewModel
    {
        public static readonly Dictionary<string, Animation> HERO_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { "Idle", new Animation(0, 0, 24, 32, 4, 400) }
        };

        StatusScene statusScene;

        public ViewModel ChildViewModel { get; set; }

        public ModelCollection<AnimatedSprite> PlayerSprites { get; private set; } = new ModelCollection<AnimatedSprite>();

        public StatusViewModel(StatusScene iScene)
            : base(iScene, PriorityLevel.GameLevel)
        {
            statusScene = iScene;

            //AvailableMenus.Add(new ItemViewModel(statusScene));
            foreach (ModelProperty<HeroModel> heroModelProperty in GameProfile.PlayerProfile.Party)
            {
                Texture2D sprite = AssetCache.SPRITES[heroModelProperty.Value.Sprite.Value];
                AnimatedSprite animatedSprite = new AnimatedSprite(sprite, HERO_ANIMATIONS);
                PlayerSprites.Add(animatedSprite);
            }

            LoadView(GameView.StatusScene_StatusView);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (ChildViewModel != null)
            {
                if (ChildViewModel.Terminated) ChildViewModel = null;
            }
            else
            {
                if (Input.CurrentInput.CommandPressed(Command.Cancel)) Back();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void Terminate()
        {
            base.Terminate();

            statusScene.EndScene();
        }

        public void SelectItems()
        {
            ChildViewModel = statusScene.AddView(new ItemViewModel(statusScene));
        }

        public void SelectQuit()
        {
            ((MapScene.MapScene)CrossPlatformGame.SceneStack.First(x => x is MapScene.MapScene)).SaveMapPosition();
            GameProfile.SetSaveData<HeroModel>("PartyLeader", GameProfile.PlayerProfile.Party.First().Value);
            GameProfile.SaveState();
            CrossPlatformGame.Transition(typeof(TitleScene.TitleScene));
        }

        public void Back()
        {
            Close();
        }
    }
}
