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


        public CommandViewModel(BattleScene iScene, BattlePlayer iBattlePlayer)
            : base(iScene, PriorityLevel.GameLevel)
        {
            battleScene = iScene;

            ActivePlayer = iBattlePlayer;

            LoadView(GameView.BattleScene_CommandView);

            int lastCategory = ActivePlayer.HeroModel.LastCategory.Value;
            ActivePlayer.HeroModel.LastCategory.Value = -1;
            switch (lastCategory)
            {
                case 0:
                    SelectEquipment();
                    GetWidget<Button>("Equipment").RadioSelect(); break;

                case 1:
                    SelectAbilities();
                    GetWidget<Button>("Abilities").RadioSelect(); break;

                case 2:
                    SelectActions();
                    GetWidget<Button>("Actions").RadioSelect(); break;
            }
        }

        public void SelectEquipment()
        {
            if (ActivePlayer.HeroModel.LastCategory.Value == 0) return;

            targetViewModel?.Terminate();

            AvailableCommands.ModelList = ActivePlayer.HeroModel.Equipment.ModelList;
            ActivePlayer.HeroModel.LastCategory.Value = 0;
        }

        public void SelectAbilities()
        {
            if (ActivePlayer.HeroModel.LastCategory.Value == 1) return;

            targetViewModel?.Terminate();

            AvailableCommands.ModelList = ActivePlayer.HeroModel.Abilities.ModelList;
            ActivePlayer.HeroModel.LastCategory.Value = 1;
        }

        public void SelectActions()
        {
            if (ActivePlayer.HeroModel.LastCategory.Value == 2) return;

            targetViewModel?.Terminate();

            AvailableCommands.ModelList = ActivePlayer.HeroModel.Actions.ModelList;
            ActivePlayer.HeroModel.LastCategory.Value = 2;
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

            targetViewModel = new TargetViewModel(battleScene, ActivePlayer, record);
            battleScene.AddView(targetViewModel);
        }


        public BattlePlayer ActivePlayer { get; set; }
        public ModelCollection<CommandRecord> AvailableCommands { get; set; } = new ModelCollection<CommandRecord>();
    }
}
