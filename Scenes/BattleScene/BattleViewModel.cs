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
        BattleScene battleScene;
        int enemyWidth;
        int enemyHeight;

        public BattleViewModel(BattleScene iScene, int iEnemyWidth, int iEnemyHeight)
            : base(iScene, PriorityLevel.GameLevel)
        {
            battleScene = iScene;
            enemyWidth = iEnemyWidth;
            enemyHeight = iEnemyHeight;
            foreach (BattleEnemy enemy in battleScene.EnemyList) Enemies.Add(enemy);
            foreach (BattlePlayer player in battleScene.PlayerList) Players.Add(player);

            EnemyWindow.Value = new Rectangle(-enemyWidth / 2 - 3, -110, enemyWidth + 6, enemyHeight + 6);
            PlayerWindow.Value = new Rectangle(-150, 30, 130, (battleScene.PlayerList.Count) * 20 + 4);

            BackgroundRender.Value = iScene.backgroundRender;

            LoadView(GameView.BattleScene_BattleView);

            EnemyPanel = GetWidget<Panel>("EnemyPanel");
        }

        public ModelCollection<BattleEnemy> Enemies { get; set; } = new ModelCollection<BattleEnemy>();
        public ModelCollection<BattlePlayer> Players { get; set; } = new ModelCollection<BattlePlayer>();

        public ModelProperty<Rectangle> EnemyWindow { get; set; } = new ModelProperty<Rectangle>(new Rectangle());
        public ModelProperty<Rectangle> PlayerWindow { get; set; } = new ModelProperty<Rectangle>(new Rectangle());
        public ModelProperty<RenderTarget2D> BackgroundRender { get; set; } = new ModelProperty<RenderTarget2D>(null);


        public Panel EnemyPanel { get; private set; }
    }
}
