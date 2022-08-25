using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Texemon.Models;
using Texemon.Scenes.BattleScene;

namespace Texemon.Scenes.StatusScene
{
    public class HeroModel : BattlerModel
    {
        public HeroModel(ClassType classType)
            : base(StatusScene.CLASSES.First(x => x.ClassType == classType).BattlerModel)
        {
            ClassRecord classRecord = StatusScene.CLASSES.First(x => x.ClassType == classType);


        }

        public ModelProperty<GameSprite> Sprite { get; set; } = new ModelProperty<GameSprite>(GameSprite.Actors_AirDrone);
    }
}
