using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texemon.Scenes.StatusScene
{
    public enum HeroType
    {
        Inventor,
        TechHero,
        SupportDrone,
        FlameDrone,
        GunDrone
    }

    public class HeroRecord
    {
        public HeroType Name { get; set; }
        public ClassType Class { get; set; }
        public string Sprite { get; set; }
        public int FlightHeight { get; set; }
        public string[] InitialEquipment { get; set; }
        public string[] InitialAbilities { get; set; }
        public string[] Actions { get; set; }
        public BattleScene.BattlerModel BattlerModel { get; set; }
    }
}
