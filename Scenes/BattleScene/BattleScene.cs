using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

using Texemon.Models;

namespace Texemon.Scenes.BattleScene
{
    public class BattleScene : Scene
    {
        public static List<EnemyRecord> ENEMIES { get; private set; }
        public static List<EncounterRecord> ENCOUNTERS { get; private set; }

        private static readonly Rectangle[] BACKGROUND_SOURCE = new Rectangle[]
        {
            new Rectangle(0, 0, 16, 112),
            new Rectangle(16, 0, 16, 112),
            new Rectangle(32, 0, 16, 112),
        };

        private EncounterRecord encounterRecord;

        public RenderTarget2D backgroundRender;

        private BattleViewModel battleViewModel;

        private List<Battler> initiativeList = new List<Battler>();
        private List<BattleEnemy> initialEnemies = new List<BattleEnemy>();
        private List<BattleEnemy> enemyList = new List<BattleEnemy>();
        private List<BattlePlayer> playerList = new List<BattlePlayer>();

        private bool introFinished;

        public BattleScene(string encounterName)
        {
            encounterRecord = ENCOUNTERS.First(x => x.Name == encounterName);

            string[] enemyTokens = encounterRecord.Enemies;
            List<Texture2D> enemySpriteList = new List<Texture2D>();
            int totalEnemyWidth = 0;
            foreach (string enemyName in enemyTokens)
            {
                EnemyRecord enemyData = ENEMIES.First(x => x.Name == enemyName);
                Texture2D enemySprite = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Enemies_" + enemyData.Sprite)];
                enemySpriteList.Add(enemySprite);
                totalEnemyWidth += enemySprite.Width;
            }
            initialEnemies = new List<BattleEnemy>(enemyList);

            BuildBackground(AssetCache.SPRITES[GameSprite.Background_Trees], totalEnemyWidth);

            battleViewModel = AddView(new BattleViewModel(this, totalEnemyWidth, 112));
        }

        ~BattleScene()
        {
            backgroundRender.Dispose();
        }

        public override void BeginScene()
        {
            sceneStarted = true;
        }

        public static void Initialize()
        {
            ENEMIES = AssetCache.LoadRecords<EnemyRecord>("EnemyData");
            ENCOUNTERS = AssetCache.LoadRecords<EncounterRecord>("EncounterData");

            BattleEnemy.Initialize();
            //BattlePlayer.Initialize();
        }

        private void BuildBackground(Texture2D backgroundImage, int width)
        {
            backgroundRender = new RenderTarget2D(CrossPlatformGame.GameInstance.GraphicsDevice, width, 112);
            CrossPlatformGame.GameInstance.GraphicsDevice.SetRenderTarget(backgroundRender);
            CrossPlatformGame.GameInstance.GraphicsDevice.Clear(Color.Transparent);
            SpriteBatch spriteBatch = new SpriteBatch(CrossPlatformGame.GameInstance.GraphicsDevice);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, null);
            spriteBatch.Draw(backgroundImage, Vector2.Zero, BACKGROUND_SOURCE[0], Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.99f);
            for (int i = 1; i < width / 16; i++) spriteBatch.Draw(backgroundImage, new Vector2(i * 16, 0), BACKGROUND_SOURCE[1], Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.99f);
            spriteBatch.Draw(backgroundImage, new Vector2(width, 0) - new Vector2(16, 0), BACKGROUND_SOURCE[2], Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.99f);
            spriteBatch.End();
        }

        private void IntroFinished()
        {
            introFinished = true;
        }

        public override void Update(GameTime gameTime, PriorityLevel priorityLevel = PriorityLevel.GameLevel)
        {
            base.Update(gameTime, priorityLevel);

            enemyList.RemoveAll(x => x.Terminated);
            if (!controllerList.Any(y => y.Exists(x => x is BattleController)) && !playerList.Exists(x => x.Busy) && !enemyList.Exists(x => x.Busy))
            {
                if (enemyList.Count == 0)
                {
                    // VICTORY
                }
                else ActivateNextBattler();
            }
        }

        /*
        public void Add(Battler battler)
        {
            EnqueueInitiative(battler);
            entityList.Add(battler);
            if (battler is BattleEnemy) enemyList.Add(battler as BattleEnemy);
            else if (battler is BattlePlayer) playerList.Add(battler as BattlePlayer);
        }
        */

        public void ActivateNextBattler()
        {
            Battler readyBattler = initiativeList.First();
            int timeAdvance = readyBattler.ActionTime;
            foreach (Battler battler in initiativeList) battler.ActionTime -= timeAdvance;
            readyBattler.StartTurn();
        }

        public void EnqueueInitiative(Battler battler)
        {
            if (initiativeList.Count == 0)
            {
                initiativeList.Add(battler);
                return;
            }

            initiativeList.Remove(battler);
            int insertIndex = initiativeList.FindLastIndex(x => x.ActionTime <= battler.ActionTime);
            initiativeList.Insert(insertIndex, battler);
        }

        public List<Battler> InitiativeList { get => initiativeList; }
        public List<BattlePlayer> PlayerList { get => playerList; }
        public List<BattleEnemy> EnemyList { get => enemyList; }
    }
}
