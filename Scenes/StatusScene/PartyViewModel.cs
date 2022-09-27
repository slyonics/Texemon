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
    public class PartyMemberModel
    {
        public ModelProperty<AnimatedSprite> PlayerSprite { get; set; }
        public ModelProperty<HeroModel> HeroModel { get; set; }
    }

    public class PartyViewModel : ViewModel, IStatusSubView
    {
        public static readonly Dictionary<string, Animation> HERO_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { "Idle", new Animation(0, 0, 24, 32, 4, 400) }
        };

        StatusScene statusScene;

        public ViewModel ChildViewModel { get; set; }

        public bool SuppressCancel { get; set; }

        public ModelCollection<PartyMemberModel> PartyMembers { get; private set; } = new ModelCollection<PartyMemberModel>();

        public PartyViewModel(StatusScene iScene)
            : base(iScene, PriorityLevel.GameLevel)
        {
            statusScene = iScene;

            foreach (ModelProperty<HeroModel> heroModelProperty in GameProfile.PlayerProfile.Party)
            {
                Texture2D sprite = AssetCache.SPRITES[heroModelProperty.Value.Sprite.Value];
                AnimatedSprite animatedSprite = new AnimatedSprite(sprite, HERO_ANIMATIONS);
                PartyMemberModel partyMember = new PartyMemberModel()
                {
                    PlayerSprite = new ModelProperty<AnimatedSprite>(animatedSprite),
                    HeroModel = new ModelProperty<HeroModel>(heroModelProperty.Value)
                };
                PartyMembers.Add(partyMember);
            }

            LoadView(GameView.StatusScene_PartyView);

            Visible = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // if (Input.CurrentInput.CommandPressed(Command.Cancel)) Ter
        }

        public void ResetSlot()
        {

        }

        public bool SuppressLeftRight { get => false; }
    }
}
