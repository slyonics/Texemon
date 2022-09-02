using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

using Texemon.Models;
using Texemon.SceneObjects.Widgets;
using Texemon.Scenes.StatusScene;

namespace Texemon.Scenes.BattleScene
{
    public class CommandViewModel : ViewModel
    {
        BattleScene battleScene;

        TargetViewModel targetViewModel;

        Button equipmentButton;
        Button abilitiesButton;
        Button actionsButton;

        int slot = -1;


        public CommandViewModel(BattleScene iScene, BattlePlayer iBattlePlayer)
            : base(iScene, PriorityLevel.GameLevel)
        {
            battleScene = iScene;

            ActivePlayer = iBattlePlayer;

            LoadView(GameView.BattleScene_CommandView);

            equipmentButton = GetWidget<Button>("Equipment");
            abilitiesButton = GetWidget<Button>("Abilities");
            actionsButton = GetWidget<Button>("Actions");

            if (ActivePlayer.HeroModel.Equipment.Count() == 0) equipmentButton.Enabled = false;
            if (ActivePlayer.HeroModel.Abilities.Count() == 0) abilitiesButton.Enabled = false;

            int lastCategory = ActivePlayer.HeroModel.LastCategory.Value;
            ActivePlayer.HeroModel.LastCategory.Value = -1;
            switch (lastCategory)
            {
                case 0:
                    SelectEquipment();
                    equipmentButton.RadioSelect();
                    break;

                case 1:
                    SelectAbilities();
                    abilitiesButton.RadioSelect();
                    break;

                case 2:
                    SelectActions();
                    actionsButton.RadioSelect();
                    break;
            }

            slot = ActivePlayer.HeroModel.LastSlot.Value;
            if (slot >= 0)
            {
                (GetWidget<DataGrid>("CommandList").ChildList[slot] as Button).RadioSelect();
                SelectCommandBody(AvailableCommands.ElementAt(slot), false);
                ActivePlayer.HeroModel.LastSlot.Value = slot;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var input = Input.CurrentInput;
            if (targetViewModel == null)
            {
                if (input.CommandPressed(Command.Left)) CursorLeft();
                else if (input.CommandPressed(Command.Right)) CursorRight();
                else if (input.CommandPressed(Command.Up)) CursorUp();
                else if (input.CommandPressed(Command.Down)) CursorDown();
                else if (input.CommandPressed(Command.Confirm)) CursorSelect();
            }
            else
            {
                if (input.CommandPressed(Command.Cancel) && slot == -1) slot = 0;
                if (targetViewModel.Terminated) targetViewModel = null;
            }
        }

        private void CursorLeft()
        {
            switch (ActivePlayer.HeroModel.LastCategory.Value)
            {
                case 0:
                    Audio.PlaySound(GameSound.menu_select);
                    SelectActionsBody(false);
                    actionsButton.RadioSelect();
                    break;

                case 1:
                    if (GetWidget<Button>("Equipment").Enabled)
                    {
                        Audio.PlaySound(GameSound.menu_select);
                        equipmentButton.RadioSelect();
                        SelectEquipmentBody(false);
                    }
                    else
                    {
                        Audio.PlaySound(GameSound.menu_select);
                        SelectActionsBody(false);
                        actionsButton.RadioSelect();
                    }
                    break;

                case 2:
                    if (abilitiesButton.Enabled)
                    {
                        Audio.PlaySound(GameSound.menu_select);
                        abilitiesButton.RadioSelect();
                        SelectAbilitiesBody(false);
                    }
                    else if (equipmentButton.Enabled)
                    {
                        Audio.PlaySound(GameSound.menu_select);
                        equipmentButton.RadioSelect();
                        SelectEquipmentBody(false);
                    }
                    else return;
                    break;
            }
        }

        private void CursorRight()
        {
            switch (ActivePlayer.HeroModel.LastCategory.Value)
            {
                case 0:
                    if (abilitiesButton.Enabled)
                    {
                        Audio.PlaySound(GameSound.menu_select);
                        abilitiesButton.RadioSelect();
                        SelectAbilitiesBody(false);
                    }
                    else
                    {
                        Audio.PlaySound(GameSound.menu_select);
                        SelectActionsBody(false);
                        actionsButton.RadioSelect();
                    }
                    break;

                case 1:
                    Audio.PlaySound(GameSound.menu_select);
                    SelectActionsBody(false);
                    actionsButton.RadioSelect();
                    break;

                case 2:
                    if (equipmentButton.Enabled)
                    {
                        Audio.PlaySound(GameSound.menu_select);
                        equipmentButton.RadioSelect();
                        SelectEquipmentBody(false);
                    }
                    else if (abilitiesButton.Enabled)
                    {
                        Audio.PlaySound(GameSound.menu_select);
                        abilitiesButton.RadioSelect();
                        SelectAbilitiesBody(false);
                    }
                    else return;
                    break;
            }
        }

        private void CursorUp()
        {
            Audio.PlaySound(GameSound.menu_select);

            if (slot == -1) slot = 0;
            else if (slot == 0) slot = AvailableCommands.Count() - 1;
            else slot--;

            (GetWidget<DataGrid>("CommandList").ChildList[slot] as Button).RadioSelect();
            SelectCommandBody(AvailableCommands.ElementAt(slot), false);
            ActivePlayer.HeroModel.LastSlot.Value = slot;
        }

        private void CursorDown()
        {
            Audio.PlaySound(GameSound.menu_select);

            if (slot == -1) slot = 0;
            else if (slot == AvailableCommands.Count() - 1) slot = 0;
            else slot++;

            (GetWidget<DataGrid>("CommandList").ChildList[slot] as Button).RadioSelect();
            SelectCommandBody(AvailableCommands.ElementAt(slot), false);
            ActivePlayer.HeroModel.LastSlot.Value = slot;
        }

        private void CursorSelect()
        {
            if (slot == -1) return;

            Audio.PlaySound(GameSound.menu_select);

            CommandRecord record= (GetWidget<DataGrid>("CommandList").Items.ElementAt(slot) as IModelProperty).GetValue() as CommandRecord;
            targetViewModel = new TargetViewModel(battleScene, ActivePlayer, record);
            battleScene.AddView(targetViewModel);
        }

        public void SelectEquipment()
        {
            SelectEquipmentBody(true);
        }

        public void SelectEquipmentBody(bool mouseMode)
        {
            if (ActivePlayer.HeroModel.LastCategory.Value == 0) return;

            targetViewModel?.Terminate();
            targetViewModel = null;

            AvailableCommands.ModelList = ActivePlayer.HeroModel.Equipment.ModelList;
            ActivePlayer.HeroModel.LastCategory.Value = 0;

            Description1.Value = Description2.Value = Description3.Value = Description4.Value = Description5.Value = null;

            if (mouseMode) slot = -1;
            else
            {
                slot = 0;
                (GetWidget<DataGrid>("CommandList").ChildList[slot] as Button).RadioSelect();
                SelectCommandBody(AvailableCommands.ElementAt(slot), false);
                ActivePlayer.HeroModel.LastSlot.Value = slot;
            }
        }

        public void SelectAbilities()
        {
            SelectAbilitiesBody(true);
        }

        public void SelectAbilitiesBody(bool mouseMode)
        {
            if (ActivePlayer.HeroModel.LastCategory.Value == 1) return;

            targetViewModel?.Terminate();
            targetViewModel = null;

            AvailableCommands.ModelList = ActivePlayer.HeroModel.Abilities.ModelList;
            ActivePlayer.HeroModel.LastCategory.Value = 1;

            Description1.Value = Description2.Value = Description3.Value = Description4.Value = Description5.Value = null;

            if (mouseMode) slot = -1;
            else
            {
                slot = 0;
                (GetWidget<DataGrid>("CommandList").ChildList[slot] as Button).RadioSelect();
                SelectCommandBody(AvailableCommands.ElementAt(slot), false);
                ActivePlayer.HeroModel.LastSlot.Value = slot;
            }
        }

        public void SelectActions()
        {
            SelectActionsBody(true);
        }

        public void SelectActionsBody(bool mouseMode)
        {
            if (ActivePlayer.HeroModel.LastCategory.Value == 2) return;

            targetViewModel?.Terminate();
            targetViewModel = null;

            AvailableCommands.ModelList = ActivePlayer.HeroModel.Actions.ModelList;
            ActivePlayer.HeroModel.LastCategory.Value = 2;

            Description1.Value = Description2.Value = Description3.Value = Description4.Value = Description5.Value = null;

            if (mouseMode) slot = -1;
            else
            {
                slot = 0;
                (GetWidget<DataGrid>("CommandList").ChildList[slot] as Button).RadioSelect();
                SelectCommandBody(AvailableCommands.ElementAt(slot), false);
                ActivePlayer.HeroModel.LastSlot.Value = slot;
            }
        }

        public void SelectCommand(object parameter)
        {
            SelectCommandBody(parameter, true);
        }

        public void SelectCommandBody(object parameter, bool mouseMode)
        {
            if (mouseMode) ActivePlayer.HeroModel.LastSlot.Value = -1;

            CommandRecord record;
            if (parameter is IModelProperty)
            {
                record = (CommandRecord)((IModelProperty)parameter).GetValue();
            }
            else record = (CommandRecord)parameter;

            targetViewModel?.Terminate();
            targetViewModel = null;

            if (mouseMode)
            {
                targetViewModel = new TargetViewModel(battleScene, ActivePlayer, record);
                battleScene.AddView(targetViewModel);
            }

            Description1.Value = record.Description.ElementAtOrDefault(0);
            Description2.Value = record.Description.ElementAtOrDefault(1);
            Description3.Value = record.Description.ElementAtOrDefault(2);
            Description4.Value = record.Description.ElementAtOrDefault(3);
            Description5.Value = record.Description.ElementAtOrDefault(4);
        }


        public BattlePlayer ActivePlayer { get; set; }
        public ModelCollection<CommandRecord> AvailableCommands { get; set; } = new ModelCollection<CommandRecord>();

        public ModelProperty<string> Description1 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description2 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description3 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description4 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description5 { get; set; } = new ModelProperty<string>("");
    }
}
