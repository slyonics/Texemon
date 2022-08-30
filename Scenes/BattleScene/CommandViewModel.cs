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
        }


        public ModelProperty<HeroModel> ActivePlayer { get; set; } = new ModelProperty<HeroModel>(null);
    }
}
