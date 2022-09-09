using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Texemon.Scenes.StatusScene;

namespace Texemon.Scenes.ShopScene
{
    public class ShopScene : Scene
    {
        public static List<ShopRecord> SHOPS { get; private set; }

        public ShopScene()
        {

        }

        public static void Initialize()
        {
            SHOPS = AssetCache.LoadRecords<ShopRecord>("ShopData");
        }

        public static List<ItemRecord> ITEMS { get => StatusScene.StatusScene.ITEMS; }
        public static List<AbilityRecord> ABILITIES { get => StatusScene.StatusScene.ABILITIES; }


    }
}
