using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texemon.Scenes.BattleScene
{
    public class BattleViewModel : ViewModel
    {
        public BattleViewModel(Scene iScene)
            : base(iScene, PriorityLevel.GameLevel, GameView.BattleScene_BattleView)
        {

        }
    }
}
