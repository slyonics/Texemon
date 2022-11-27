﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MonsterLegends.Scenes.StatusScene;

namespace MonsterLegends.Scenes.ShopScene
{
    public class ShopScene : Scene
    {
        public static List<ShopRecord> SHOPS { get; private set; }

        private ConstructViewModel shopViewModel;

        public ShopScene(string shopName)
        {
            ShopRecord shopRecord = SHOPS.First(x => x.Name == shopName);

            shopViewModel = AddView(new ConstructViewModel(this, shopRecord));
        }

        public static void Initialize()
        {
            SHOPS = AssetCache.LoadRecords<ShopRecord>("ShopData");
        }

        public override void BeginScene()
        {
            sceneStarted = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (shopViewModel.Terminated) EndScene();
        }

        public static List<ItemRecord> ITEMS { get => StatusScene.StatusScene.ITEMS; }
        public static List<AbilityRecord> ABILITIES { get => StatusScene.StatusScene.ABILITIES; }


    }
}
