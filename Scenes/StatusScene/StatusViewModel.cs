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
    public interface IStatusSubView
    {
        void Update(GameTime gameTime);
        void Draw(SpriteBatch spriteBatch);
        void MoveAway();
        void ResetSlot();
        public bool Visible { get; set; }
        public bool SuppressCancel { get; }
        public bool SuppressLeftRight { get; }
    }

    public class StatusViewModel : ViewModel
    {
        public static readonly Dictionary<string, Animation> HERO_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { "Idle", new Animation(0, 0, 24, 32, 4, 400) }
        };

        List<ViewModel> SubViews { get; set; } = new List<ViewModel>();

        StatusScene statusScene;

        int slot = -1;

        public IStatusSubView ChildViewModel { get; set; }

        public StatusViewModel(StatusScene iScene)
            : base(iScene, PriorityLevel.GameLevel)
        {
            statusScene = iScene;

            ModelCollection<PartyMemberModel> partyMembers = new ModelCollection<PartyMemberModel>();
            foreach (ModelProperty<HeroModel> heroModelProperty in GameProfile.PlayerProfile.Party)
            {
                Texture2D sprite = AssetCache.SPRITES[heroModelProperty.Value.Sprite.Value];
                AnimatedSprite animatedSprite = new AnimatedSprite(sprite, HERO_ANIMATIONS);
                PartyMemberModel partyMember = new PartyMemberModel()
                {
                    PlayerSprite = new ModelProperty<AnimatedSprite>(animatedSprite),
                    HeroModel = new ModelProperty<HeroModel>(heroModelProperty.Value)
                };
                partyMembers.Add(partyMember);
            }

            SubViews.Add(new PartyViewModel(statusScene, partyMembers));
            SubViews.Add(new ItemViewModel(statusScene));
            SubViews.Add(new EquipmentViewModel(statusScene, partyMembers));
            SubViews.Add(new AbilitiesViewModel(statusScene, partyMembers));
            SubViews.Add(new SystemViewModel(statusScene));

            LoadView(GameView.StatusScene_StatusView);

            GetWidget<Button>("PartyButton").RadioSelect();
            SelectParty();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            bool suppressCancel = ChildViewModel.SuppressCancel;
            ChildViewModel?.Update(gameTime);
            suppressCancel |= ChildViewModel.SuppressCancel;

            InputFrame currentInput = Input.CurrentInput;
            if (ChildViewModel == null || !ChildViewModel.SuppressLeftRight)
            {
                if (currentInput.CommandPressed(Command.Left)) CursorLeft();
                else if (currentInput.CommandPressed(Command.Right)) CursorRight();
                else if (currentInput.CommandPressed(Command.Cancel) && (ChildViewModel == null || !suppressCancel))
                {
                    Audio.PlaySound(GameSound.Back);
                    Back();
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            ChildViewModel?.Draw(spriteBatch);
        }

        public override void Terminate()
        {
            base.Terminate();

            statusScene.EndScene();
        }

        private void CursorLeft()
        {
            slot--;
            if (slot < 0)
            {
                slot = 0;
                return;
            }
            else Audio.PlaySound(GameSound.menu_select);

            PresentSubmenu(slot);
        }

        private void CursorRight()
        {
            slot++;
            if (slot > 4)
            {
                slot = 4;
                return;
            }
            else Audio.PlaySound(GameSound.menu_select);

            PresentSubmenu(slot);
        }

        private void PresentSubmenu(int slot)
        {
            switch (slot)
            {
                case 0: GetWidget<Button>("PartyButton").RadioSelect(); SelectParty(); break;
                case 1: GetWidget<Button>("ItemsButton").RadioSelect(); SelectItems(); break;
                case 2: GetWidget<Button>("EquipmentButton").RadioSelect(); SelectEquipment(); break;
                case 3: GetWidget<Button>("AbilitiesButton").RadioSelect(); SelectAbilities(); break;
                case 4: GetWidget<Button>("SystemButton").RadioSelect(); SelectSystem(); break;
            }
        }

        public void SelectParty()
        {
            if (ChildViewModel != null)
            {
                ChildViewModel.Visible = false;
                ChildViewModel.MoveAway();
            }

            slot = 0;
            ChildViewModel = SubViews.First(x => x is PartyViewModel) as IStatusSubView;

            ChildViewModel.Visible = true;

            ChildViewModel.ResetSlot();
        }

        public void SelectItems()
        {
            ChildViewModel.Visible = false;
            ChildViewModel.MoveAway();

            slot = 1;
            ChildViewModel = SubViews.First(x => x is ItemViewModel) as IStatusSubView;

            ChildViewModel.Visible = true;

            ChildViewModel.ResetSlot();
        }

        public void SelectEquipment()
        {
            ChildViewModel.Visible = false;
            ChildViewModel.MoveAway();

            slot = 2;
            ChildViewModel = SubViews.First(x => x is EquipmentViewModel) as IStatusSubView;

            ChildViewModel.Visible = true;

            ChildViewModel.ResetSlot();
        }

        public void SelectAbilities()
        {
            ChildViewModel.Visible = false;
            ChildViewModel.MoveAway();

            slot = 3;
            ChildViewModel = SubViews.First(x => x is AbilitiesViewModel) as IStatusSubView;

            ChildViewModel.Visible = true;

            ChildViewModel.ResetSlot();
        }

        public void SelectSystem()
        {
            ChildViewModel.Visible = false;
            ChildViewModel.MoveAway();

            slot = 4;
            ChildViewModel = SubViews.First(x => x is SystemViewModel) as IStatusSubView;

            ChildViewModel.Visible = true;

            ChildViewModel.ResetSlot();
        }

        public void Back()
        {
            Terminate();
        }
    }
}
