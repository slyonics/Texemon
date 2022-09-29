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
        public bool SuppressCancel { get; set; }
        public bool SuppressLeftRight { get; }
    }

    public class StatusViewModel : ViewModel
    {
        List<ViewModel> SubViews { get; set; } = new List<ViewModel>();

        StatusScene statusScene;

        int slot = -1;

        public IStatusSubView ChildViewModel { get; set; }

        public StatusViewModel(StatusScene iScene)
            : base(iScene, PriorityLevel.GameLevel)
        {
            statusScene = iScene;

            SubViews.Add(new PartyViewModel(statusScene));
            SubViews.Add(new ItemViewModel(statusScene));
            SubViews.Add(new EquipmentViewModel(statusScene));
            SubViews.Add(new AbilitiesViewModel(statusScene));

            LoadView(GameView.StatusScene_StatusView);

            GetWidget<Button>("PartyButton").RadioSelect();
            SelectParty();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            ChildViewModel?.Update(gameTime);

            InputFrame currentInput = Input.CurrentInput;

            if (ChildViewModel == null || !ChildViewModel.SuppressLeftRight)
            {
                if (currentInput.CommandPressed(Command.Left)) CursorLeft();
                else if (currentInput.CommandPressed(Command.Right)) CursorRight();
                else if (currentInput.CommandPressed(Command.Cancel) && (ChildViewModel == null || !ChildViewModel.SuppressCancel))
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
            if (slot < 0) slot = 0;
            else Audio.PlaySound(GameSound.menu_select);

            switch (slot)
            {
                case 0: GetWidget<Button>("PartyButton").RadioSelect(); SelectParty(); break;
                case 1: GetWidget<Button>("ItemsButton").RadioSelect(); SelectItems(); break;
                case 2: GetWidget<Button>("EquipmentButton").RadioSelect(); SelectEquipment(); break;
                case 3: GetWidget<Button>("AbilitiesButton").RadioSelect(); SelectAbilities(); break;
            }
        }

        private void CursorRight()
        {
            slot++;
            if (slot > 3) slot = 3;
            else Audio.PlaySound(GameSound.menu_select);

            switch (slot)
            {
                case 0: GetWidget<Button>("PartyButton").RadioSelect(); SelectParty(); break;
                case 1: GetWidget<Button>("ItemsButton").RadioSelect(); SelectItems(); break;
                case 2: GetWidget<Button>("EquipmentButton").RadioSelect(); SelectEquipment(); break;
                case 3: GetWidget<Button>("AbilitiesButton").RadioSelect(); SelectAbilities(); break;
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

        public void SelectQuit()
        {
            ((MapScene.MapScene)CrossPlatformGame.SceneStack.First(x => x is MapScene.MapScene)).SaveMapPosition();
            GameProfile.SetSaveData<HeroModel>("PartyLeader", GameProfile.PlayerProfile.Party.First().Value);
            GameProfile.SaveState();
            CrossPlatformGame.Transition(typeof(TitleScene.TitleScene));
        }

        public void Back()
        {
            Terminate();
        }
    }
}
