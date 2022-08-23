using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Texemon.Models;

namespace Texemon.Scenes.BattleScene
{
    public class BattleScene : Scene
    {
        public static List<EnemyRecord> ENEMIES { get; private set; }
        public static List<EncounterRecord> ENCOUNTERS { get; private set; }

        private EncounterRecord encounterRecord;

        public BattleScene(string encounterName)
        {

            if (ENEMIES == null) ENEMIES = AssetCache.LoadRecords<EnemyRecord>("EnemyData");
            if (ENCOUNTERS == null) ENCOUNTERS = AssetCache.LoadRecords<EncounterRecord>("EncounterData");

            encounterRecord = ENCOUNTERS.First(x => x.Name == encounterName);

        }
    }
}
