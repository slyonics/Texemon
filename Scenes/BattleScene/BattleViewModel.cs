using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

using Texemon.Models;
using Texemon.SceneObjects.Widgets;

namespace Texemon.Scenes.BattleScene
{
    public class BattleViewModel : ViewModel
    {
        int enemyWidth;
        int enemyHeight;

        public BattleViewModel(BattleScene iScene, int iEnemyWidth, int iEnemyHeight)
            : base(iScene, PriorityLevel.GameLevel)
        {
            enemyWidth = iEnemyWidth;
            enemyHeight = iEnemyHeight;
            EnemyWindow.Value = new Rectangle(-enemyWidth / 2 - 3, -90, enemyWidth + 6, enemyHeight + 6);

            BackgroundRender.Value = iScene.backgroundRender;

            LoadView(GameView.BattleScene_BattleView);

            EnemyPanel = GetWidget<Panel>("EnemyPanel");
        }

        public ModelProperty<Rectangle> EnemyWindow { get; set; } = new ModelProperty<Rectangle>(new Rectangle(-60, -90, 112, 112));
        public ModelProperty<Rectangle> PlayerWindow { get; set; } = new ModelProperty<Rectangle>(new Rectangle(-140, 40, 80, 40));
        public ModelProperty<RenderTarget2D> BackgroundRender { get; set; } = new ModelProperty<RenderTarget2D>(null);

        public Panel EnemyPanel { get; private set; }
    }
}
