using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Texemon.Models;

namespace Texemon.Scenes.BattleScene
{
    public class BattleViewModel : ViewModel
    {
        int enemyWidth;
        int enemyHeight;

        public BattleViewModel(Scene iScene, int iEnemyWidth, int iEnemyHeight)
            : base(iScene, PriorityLevel.GameLevel)
        {
            enemyWidth = iEnemyWidth;
            enemyHeight = iEnemyHeight;
            EnemyWindow.Value = new Rectangle(-enemyWidth / 2, -80, enemyWidth, enemyHeight);

            LoadView(GameView.BattleScene_BattleView);
        }

        public ModelProperty<Rectangle> EnemyWindow { get; set; } = new ModelProperty<Rectangle>(new Rectangle(-60, -80, 112, 112));
        public ModelProperty<Rectangle> PlayerWindow { get; set; } = new ModelProperty<Rectangle>(new Rectangle(-140, 20, 80, 40));
    }
}
