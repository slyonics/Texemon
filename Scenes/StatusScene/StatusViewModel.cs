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
        List<ViewModel> SubViews { get; set; } = new List<ViewModel>();

        StatusScene statusScene;

        int slot = -1;

        public ViewModel ChildViewModel { get; set; }

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
            if (currentInput.CommandPressed(Command.Left)) CursorLeft();
            else if (currentInput.CommandPressed(Command.Right)) CursorRight();
            else if (currentInput.CommandPressed(Command.Cancel)) Back();
            
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
            else Audio.PlaySound(GameSound.Cursor);

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
            else Audio.PlaySound(GameSound.Cursor);

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
            if (ChildViewModel != null) ChildViewModel.Visible = false;

            slot = 0;
            ChildViewModel = SubViews.First(x => x is PartyViewModel);

            ChildViewModel.Visible = true;
        }

        public void SelectItems()
        {
            ChildViewModel.Visible = false;

            slot = 1;
            ChildViewModel = SubViews.First(x => x is ItemViewModel);

            ChildViewModel.Visible = true;
        }

        public void SelectEquipment()
        {
            ChildViewModel.Visible = false;

            slot = 2;
            ChildViewModel = SubViews.First(x => x is EquipmentViewModel);

            ChildViewModel.Visible = true;
        }

        public void SelectAbilities()
        {
            ChildViewModel.Visible = false;

            slot = 3;
            ChildViewModel = SubViews.First(x => x is AbilitiesViewModel);

            ChildViewModel.Visible = true;
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
