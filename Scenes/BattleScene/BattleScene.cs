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

        private EncounterRecord encounterRecord;

        private BattleViewModel battleViewModel;

        private List<Battler> initiativeList = new List<Battler>();
        private List<BattleEnemy> initialEnemies = new List<BattleEnemy>();
        private List<BattleEnemy> enemyList = new List<BattleEnemy>();
        private List<BattlePlayer> playerList = new List<BattlePlayer>();

        private bool introFinished;

        public BattleScene(string encounterName)
        {
            RenderTarget2D matchRender = new RenderTarget2D(CrossPlatformGame.GameInstance.GraphicsDevice, 240, 720);

            encounterRecord = ENCOUNTERS.First(x => x.Name == encounterName);

            battleViewModel = AddView(new BattleViewModel(this));
        }

        public override void BeginScene()
        {
            sceneStarted = true;
        }

        public static void Initialize()
        {
            if (ENEMIES == null)
            {
                ENEMIES = AssetCache.LoadRecords<EnemyRecord>("EnemyData");
                ENCOUNTERS = AssetCache.LoadRecords<EncounterRecord>("EncounterData");

                BattleEnemy.Initialize();
            }
        }

        private void IntroFinished()
        {
            introFinished = true;
        }

        private void SpawnEnemies()
        {
            foreach (BattleEnemy battleEnemy in initialEnemies) Add(battleEnemy);
        }

        public void Add(Battler battler)
        {
            EnqueueInitiative(battler);
            entityList.Add(battler);
            if (battler is BattleEnemy) enemyList.Add(battler as BattleEnemy);
            else if (battler is BattlePlayer) playerList.Add(battler as BattlePlayer);
        }

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
