using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTrainer.Scenes.StatusScene
{
    public class StatusScene : Scene
    {
        public static List<HeroRecord> HEROES { get; private set; }
        public static List<ClassRecord> CLASSES { get; private set; }
        public static List<ItemRecord> ITEMS { get; private set; }
        public static List<AbilityRecord> ABILITIES { get; private set; }

        public StatusViewModel StatusViewModel { get; private set; }

        public StatusScene()
            : base()
        {
            StatusViewModel = AddView(new StatusViewModel(this));
        }

        public static void Initialize()
        {
            HEROES = AssetCache.LoadRecords<HeroRecord>("HeroData");
            CLASSES = AssetCache.LoadRecords<ClassRecord>("ClassData");

            ITEMS = AssetCache.LoadRecords<ItemRecord>("ItemData");
            foreach (ItemRecord itemRecord in ITEMS) itemRecord.ChargesLeft = itemRecord.Charges;

            ABILITIES = AssetCache.LoadRecords<AbilityRecord>("AbilityData");
            foreach (AbilityRecord abilityRecord in ABILITIES) abilityRecord.ChargesLeft = abilityRecord.Charges;
        }

        public override void BeginScene()
        {
            sceneStarted = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);


        }
    }
}
