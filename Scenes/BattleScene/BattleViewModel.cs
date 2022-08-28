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

        public BattleViewModel(BattleScene iScene, EncounterRecord encounterRecord)
            : base(iScene, PriorityLevel.GameLevel)
        {
            battleScene = iScene;

            string[] enemyTokens = encounterRecord.Enemies;
            int totalEnemyWidth = 0;
            foreach (string enemyName in enemyTokens)
            {
                EnemyRecord enemyRecord = BattleScene.ENEMIES.First(x => x.Name == enemyName);
                Texture2D enemySprite = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Enemies_" + enemyRecord.Sprite)];
                totalEnemyWidth += enemySprite.Width;
                InitialEnemies.Add(enemyRecord);
            }
            enemyWidth = totalEnemyWidth;
            enemyHeight = 112;
            EnemyWindow.Value = new Rectangle(-enemyWidth / 2 - 3, -110, enemyWidth + 6, enemyHeight + 6);

            BackgroundRender.Value = iScene.backgroundRender;

            LoadView(GameView.BattleScene_BattleView);

            EnemyPanel = GetWidget<Panel>("EnemyPanel");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);


        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public List<EnemyRecord> InitialEnemies { get; set; } = new List<EnemyRecord>();

        public ModelProperty<Rectangle> EnemyWindow { get; set; } = new ModelProperty<Rectangle>(new Rectangle());
        public ModelProperty<Rectangle> PlayerWindow { get; set; } = new ModelProperty<Rectangle>(new Rectangle(-150, 30, 130, (GameProfile.PlayerProfile.Party.Count()) * 20 + 4));
        public ModelProperty<RenderTarget2D> BackgroundRender { get; set; } = new ModelProperty<RenderTarget2D>(null);

        public ModelProperty<bool> ReadyToProceed { get; set; } = new ModelProperty<bool>(false);
        public Panel EnemyPanel { get; private set; }

        public bool Transitioning { get => EnemyPanel.Transitioning; }
    }
}
