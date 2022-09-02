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

        int category = -1;
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

            if (!Input.MOUSE_MODE)
            {
                slot = ActivePlayer.HeroModel.LastSlot.Value;
                (GetWidget<DataGrid>("CommandList").ChildList[slot] as Button).RadioSelect();
                SelectCommand(AvailableCommands.ElementAt(slot));
                ActivePlayer.HeroModel.LastSlot.Value = slot;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.MOUSE_MODE) slot = -1;

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
                    SelectActions();
                    slot = 0;
                    (GetWidget<DataGrid>("CommandList").ChildList[slot] as Button).RadioSelect();
                    SelectCommand(AvailableCommands.ElementAt(slot));
                    ActivePlayer.HeroModel.LastSlot.Value = slot;
                    actionsButton.RadioSelect();
                    break;

                case 1:
                    if (GetWidget<Button>("Equipment").Enabled)
                    {
                        Audio.PlaySound(GameSound.menu_select);
                        equipmentButton.RadioSelect();
                        SelectEquipment();
                        slot = 0;
                        (GetWidget<DataGrid>("CommandList").ChildList[slot] as Button).RadioSelect();
                        SelectCommand(AvailableCommands.ElementAt(slot));
                        ActivePlayer.HeroModel.LastSlot.Value = slot;
                    }
                    else
                    {
                        Audio.PlaySound(GameSound.menu_select);
                        SelectActions();
                        slot = 0;
                        (GetWidget<DataGrid>("CommandList").ChildList[slot] as Button).RadioSelect();
                        SelectCommand(AvailableCommands.ElementAt(slot));
                        ActivePlayer.HeroModel.LastSlot.Value = slot;
                        actionsButton.RadioSelect();
                    }
                    break;

                case 2:
                    if (abilitiesButton.Enabled)
                    {
                        Audio.PlaySound(GameSound.menu_select);
                        abilitiesButton.RadioSelect();
                        SelectAbilities();
                        slot = 0;
                        (GetWidget<DataGrid>("CommandList").ChildList[slot] as Button).RadioSelect();
                        SelectCommand(AvailableCommands.ElementAt(slot));
                        ActivePlayer.HeroModel.LastSlot.Value = slot;
                    }
                    else if (equipmentButton.Enabled)
                    {
                        Audio.PlaySound(GameSound.menu_select);
                        equipmentButton.RadioSelect();
                        SelectEquipment();
                        slot = 0;
                        (GetWidget<DataGrid>("CommandList").ChildList[slot] as Button).RadioSelect();
                        SelectCommand(AvailableCommands.ElementAt(slot));
                        ActivePlayer.HeroModel.LastSlot.Value = slot;
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
                        SelectAbilities();
                        slot = 0;
                        (GetWidget<DataGrid>("CommandList").ChildList[slot] as Button).RadioSelect();
                        SelectCommand(AvailableCommands.ElementAt(slot));
                        ActivePlayer.HeroModel.LastSlot.Value = slot;
                    }
                    else
                    {
                        Audio.PlaySound(GameSound.menu_select);
                        SelectActions();
                        slot = 0;
                        (GetWidget<DataGrid>("CommandList").ChildList[slot] as Button).RadioSelect();
                        SelectCommand(AvailableCommands.ElementAt(slot));
                        ActivePlayer.HeroModel.LastSlot.Value = slot;
                        actionsButton.RadioSelect();
                    }
                    break;

                case 1:
                    Audio.PlaySound(GameSound.menu_select);
                    SelectActions();
                    actionsButton.RadioSelect();
                    slot = 0;
                    (GetWidget<DataGrid>("CommandList").ChildList[slot] as Button).RadioSelect();
                    SelectCommand(AvailableCommands.ElementAt(slot));
                    ActivePlayer.HeroModel.LastSlot.Value = slot;
                    break;

                case 2:
                    if (equipmentButton.Enabled)
                    {
                        Audio.PlaySound(GameSound.menu_select);
                        equipmentButton.RadioSelect();
                        SelectEquipment();
                        slot = 0;
                        (GetWidget<DataGrid>("CommandList").ChildList[slot] as Button).RadioSelect();
                        SelectCommand(AvailableCommands.ElementAt(slot));
                        ActivePlayer.HeroModel.LastSlot.Value = slot;
                    }
                    else if (abilitiesButton.Enabled)
                    {
                        Audio.PlaySound(GameSound.menu_select);
                        abilitiesButton.RadioSelect();
                        SelectAbilities();
                        slot = 0;
                        (GetWidget<DataGrid>("CommandList").ChildList[slot] as Button).RadioSelect();
                        SelectCommand(AvailableCommands.ElementAt(slot));
                        ActivePlayer.HeroModel.LastSlot.Value = slot;
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
            SelectCommand(AvailableCommands.ElementAt(slot));
            ActivePlayer.HeroModel.LastSlot.Value = slot;
        }

        private void CursorDown()
        {
            Audio.PlaySound(GameSound.menu_select);

            if (slot == -1) slot = 0;
            else if (slot == AvailableCommands.Count() - 1) slot = 0;
            else slot++;

            (GetWidget<DataGrid>("CommandList").ChildList[slot] as Button).RadioSelect();
            SelectCommand(AvailableCommands.ElementAt(slot));
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
            if (category == 0) return;

            targetViewModel?.Terminate();
            targetViewModel = null;

            AvailableCommands.ModelList = ActivePlayer.HeroModel.Equipment.ModelList;
            ActivePlayer.HeroModel.LastCategory.Value = category = 0;

            Description1.Value = Description2.Value = Description3.Value = Description4.Value = Description5.Value = null;
        }

        public void SelectAbilities()
        {
            if (category == 1) return;

            targetViewModel?.Terminate();
            targetViewModel = null;

            AvailableCommands.ModelList = ActivePlayer.HeroModel.Abilities.ModelList;
            ActivePlayer.HeroModel.LastCategory.Value = category = 1;

            Description1.Value = Description2.Value = Description3.Value = Description4.Value = Description5.Value = null;
        }

        public void SelectActions()
        {
            if (category == 2) return;

            targetViewModel?.Terminate();
            targetViewModel = null;

            AvailableCommands.ModelList = ActivePlayer.HeroModel.Actions.ModelList;
            ActivePlayer.HeroModel.LastCategory.Value = category = 2;

            Description1.Value = Description2.Value = Description3.Value = Description4.Value = Description5.Value = null;
        }

        public void SelectCommand(object parameter)
        {
            CommandRecord record;
            if (parameter is IModelProperty)
            {
                record = (CommandRecord)((IModelProperty)parameter).GetValue();
            }
            else record = (CommandRecord)parameter;

            targetViewModel?.Terminate();
            targetViewModel = null;

            if (Input.MOUSE_MODE)
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
