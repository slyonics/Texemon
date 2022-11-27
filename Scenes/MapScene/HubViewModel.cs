﻿using System;
using System.Linq;
using System.Collections.Generic;

using MonsterTrainer.Main;
using MonsterTrainer.Models;
using MonsterTrainer.SceneObjects;
using MonsterTrainer.SceneObjects.Widgets;
using System.Threading.Tasks;

namespace MonsterTrainer.Scenes.ConversationScene
{
    public class HubViewModel : ViewModel
    {


        public HubViewModel(Scene iScene)
            : base(iScene, PriorityLevel.GameLevel)
        {
            if (GameProfile.GetSaveData<bool>("GotMonster")) ShowHud.Value = true;

            LoadView(GameView.MapScene_HubView);
        }

        

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            

        }

        public ModelProperty<bool> ShowHud { get; set; } = new ModelProperty<bool>(false);

    }
}
