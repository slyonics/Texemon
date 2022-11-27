using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

using MonsterLegends.Models;
using MonsterLegends.SceneObjects.Widgets;
using MonsterLegends.Scenes.StatusScene;

namespace MonsterLegends.Scenes.BattleScene
{
    public class BattleViewModel : ViewModel
    {
        BattleScene battleScene;
        int enemyWidth;
        int enemyHeight;
        CommandViewModel commandViewModel;
        public BattleEnemy LastEnemyTarget { get; set; }
        public BattlePlayer LastPlayerTarget { get; set; }

        public BattleViewModel(BattleScene iScene, EncounterRecord encounterRecord)
            : base(iScene, PriorityLevel.GameLevel)
        {
            battleScene = iScene;

            string[] enemyTokens = encounterRecord.Enemies;
            int totalEnemyWidth = 0;
            int enemyMargin = 0;
            foreach (string enemyName in enemyTokens)
            {
                EnemyRecord enemyRecord = BattleScene.ENEMIES.First(x => x.Name == enemyName);
                Texture2D enemySprite = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Enemies_" + enemyRecord.Sprite)];
                totalEnemyWidth += enemySprite.Width;
                InitialEnemies.Add(enemyRecord);
            }

            if (encounterRecord.Width > 0)
            {
                enemyWidth = encounterRecord.Width;
                enemyMargin = (encounterRecord.Width - totalEnemyWidth) / 2;
            }
            else enemyWidth = totalEnemyWidth;
            enemyHeight = 112;
            EnemyWindow.Value = new Rectangle(-enemyWidth / 2 - 4, -110, enemyWidth + 8, enemyHeight + 6);
            EnemyMargin.Value = new Rectangle(enemyMargin, 0, enemyMargin, 0);

            BackgroundRender.Value = iScene.backgroundRender;

            foreach (var model in GameProfile.PlayerProfile.Party)
            {
                model.Value.NameColor = new ModelProperty<Color>(new Color(252, 252, 252, 255));
                model.Value.HealthColor = new ModelProperty<Color>(new Color(252, 252, 252, 255));
            }

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

        public void StartPlayerTurn(BattlePlayer battlePlayer)
        {
            PlayerTurn.Value = true;
            commandViewModel = new CommandViewModel(battleScene, battlePlayer);
            battleScene.AddView(commandViewModel);
        }

        public void EndPlayerTurn(BattlePlayer battlePlayer)
        {
            PlayerTurn.Value = false;
            commandViewModel.Terminate();
        }

        public override void Close()
        {
            base.Close();

            battleScene.EnemyList.Clear();
        }

        public override void Terminate()
        {
            base.Terminate();

            battleScene.EndScene();
        }

        public List<EnemyRecord> InitialEnemies { get; set; } = new List<EnemyRecord>();

        public ModelProperty<Rectangle> EnemyWindow { get; set; } = new ModelProperty<Rectangle>(new Rectangle());
        public ModelProperty<Rectangle> EnemyMargin { get; set; } = new ModelProperty<Rectangle>(new Rectangle());
        public ModelProperty<Rectangle> PlayerWindow { get; set; } = new ModelProperty<Rectangle>(new Rectangle(-150, 30, 130, (4) * 20 + 3));
        public ModelProperty<RenderTarget2D> BackgroundRender { get; set; } = new ModelProperty<RenderTarget2D>(null);

        public ModelProperty<bool> ReadyToProceed { get; set; } = new ModelProperty<bool>(false);
        public ModelProperty<bool> PlayerTurn { get; set; } = new ModelProperty<bool>(false);
        public Panel EnemyPanel { get; private set; }
    }
}
