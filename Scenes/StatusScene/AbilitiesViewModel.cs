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
    public class AbilitiesViewModel : ViewModel
    {
        public static readonly Dictionary<string, Animation> HERO_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { "Idle", new Animation(0, 0, 24, 32, 4, 400) }
        };

        StatusScene statusScene;

        int slot = -1;

        public ViewModel ChildViewModel { get; set; }

        public ModelCollection<PartyMemberModel> PartyMembers { get; private set; } = new ModelCollection<PartyMemberModel>();
        public ModelCollection<CommandRecord> AbilitiesList { get; private set; } = new ModelCollection<CommandRecord>();

        public AbilitiesViewModel(StatusScene iScene)
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

            LoadView(GameView.StatusScene_AbilitiesView);

            Visible = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.CurrentInput.CommandPressed(Command.Up)) CursorUp();
            else if (Input.CurrentInput.CommandPressed(Command.Down)) CursorDown();
        }

        private void CursorUp()
        {
            slot--;
            if (slot < 0)
            {
                slot = 0;
                return;
            }

            Audio.PlaySound(GameSound.Cursor);

            SelectParty(PartyMembers[slot].HeroModel);
            (GetWidget<DataGrid>("PartyList").ChildList[slot] as Button).RadioSelect();
        }

        private void CursorDown()
        {
            slot++;
            if (slot >= GameProfile.PlayerProfile.Party.Count())
            {
                slot = GameProfile.PlayerProfile.Party.Count() - 1;
                return;
            }

            Audio.PlaySound(GameSound.Cursor);

            SelectParty(PartyMembers[slot].HeroModel);
            (GetWidget<DataGrid>("PartyList").ChildList[slot] as Button).RadioSelect();
        }

        public void SelectParty(object parameter)
        {
            HeroModel record;
            if (parameter is IModelProperty)
            {
                record = (HeroModel)((IModelProperty)parameter).GetValue();
            }
            else record = (HeroModel)parameter;

            slot = PartyMembers.ToList().FindIndex(x => x.Value.HeroModel.Value == record);
            AbilitiesList.ModelList = record.Abilities.ModelList;
        }

        public void SelectItem(object parameter)
        {
            CommandRecord record;
            if (parameter is IModelProperty)
            {
                record = (CommandRecord)((IModelProperty)parameter).GetValue();
            }
            else record = (CommandRecord)parameter;

            Description1.Value = record.Description.ElementAtOrDefault(0);
            Description2.Value = record.Description.ElementAtOrDefault(1);
            Description3.Value = record.Description.ElementAtOrDefault(2);
            Description4.Value = record.Description.ElementAtOrDefault(3);
            Description5.Value = record.Description.ElementAtOrDefault(4);
        }

        public ModelProperty<string> Description1 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description2 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description3 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description4 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description5 { get; set; } = new ModelProperty<string>("");
    }
}
