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
    public class PartyViewModel : ViewModel
    {
        public static readonly Dictionary<string, Animation> HERO_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { "Idle", new Animation(0, 0, 24, 32, 4, 400) }
        };

        StatusScene statusScene;

        public ViewModel ChildViewModel { get; set; }

        public ModelCollection<AnimatedSprite> PlayerSprites { get; private set; } = new ModelCollection<AnimatedSprite>();

        public PartyViewModel(StatusScene iScene)
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

            LoadView(GameView.StatusScene_PartyView);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // if (Input.CurrentInput.CommandPressed(Command.Cancel)) Ter
        }
    }
}
