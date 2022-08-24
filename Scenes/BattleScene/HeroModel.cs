using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Texemon.Models;

namespace Texemon.Scenes.BattleScene
{
    public class HeroModel
    {
        public ModelProperty<BattlerModel> Stats { get; set; } = new ModelProperty<BattlerModel>(new BattlerModel());
        public ModelProperty<GameSprite> Sprite { get; set; }= new ModelProperty<GameSprite>(GameSprite.Actors_AirDrone);
    }
}
