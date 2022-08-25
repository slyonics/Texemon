using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texemon.Scenes.StatusScene
{
    public class StatusScene
    {
        public static List<ClassRecord> CLASSES { get; private set; }
        public static List<ItemRecord> ITEMS { get; private set; }
        public static List<AbilityRecord> ABILITIES { get; private set; }

        public static void Initialize()
        {
            CLASSES = AssetCache.LoadRecords<ClassRecord>("ClassData");
            ITEMS = AssetCache.LoadRecords<ItemRecord>("ItemData");
            ABILITIES = AssetCache.LoadRecords<AbilityRecord>("AbilityData");
        }
    }
}
