using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Texemon.Models;
using Texemon.Scenes.BattleScene;

namespace Texemon.Scenes.StatusScene
{
    public class HeroModel
    {
        public HeroModel(ClassType classType)
        {
            ClassRecord classRecord = StatusScene.CLASSES.First(x => x.ClassType == classType);

            Stats.Value = new BattlerModel(classRecord.BattlerModel);
        }

        public ModelProperty<BattlerModel> Stats { get; set; } = new ModelProperty<BattlerModel>(new BattlerModel());
        public ModelProperty<GameSprite> Sprite { get; set; } = new ModelProperty<GameSprite>(GameSprite.Actors_AirDrone);
    }
}
