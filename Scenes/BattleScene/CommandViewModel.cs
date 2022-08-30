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


        public CommandViewModel(BattleScene iScene, HeroModel iHeroModel)
            : base(iScene, PriorityLevel.GameLevel)
        {
            battleScene = iScene;

            ActivePlayer.Value = iHeroModel;

            LoadView(GameView.BattleScene_CommandView);

            switch (ActivePlayer.Value.LastCategory.Value)
            {
                case 0:
                    SelectEquipment();
                    GetWidget<Button>("Equipment").RadioSelect(); break;

                case 1:
                    SelectAbilities(); GetWidget<Button>("Abilities").RadioSelect(); break;

                case 2:
                    SelectActions(); GetWidget<Button>("Actions").RadioSelect(); break;
            }
        }

        public void SelectEquipment()
        {
            AvailableCommands.ModelList = ActivePlayer.Value.Equipment.ModelList;
            ActivePlayer.Value.LastCategory.Value = 0;
        }

        public void SelectAbilities()
        {
            AvailableCommands.ModelList = ActivePlayer.Value.Abilities.ModelList;
            ActivePlayer.Value.LastCategory.Value = 1;
        }

        public void SelectActions()
        {
            AvailableCommands.ModelList = ActivePlayer.Value.Actions.ModelList;
            ActivePlayer.Value.LastCategory.Value = 2;
        }


        public ModelProperty<HeroModel> ActivePlayer { get; set; } = new ModelProperty<HeroModel>(null);
        public ModelCollection<CommandRecord> AvailableCommands { get; set; } = new ModelCollection<CommandRecord>();
    }
}
