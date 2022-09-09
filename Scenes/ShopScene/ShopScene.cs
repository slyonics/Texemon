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
        public ShopScene()
        {

        }

        public static List<ItemRecord> ITEMS { get => StatusScene.StatusScene.ITEMS; }
        public static List<AbilityRecord> ABILITIES { get => StatusScene.StatusScene.ABILITIES; }


    }
}
