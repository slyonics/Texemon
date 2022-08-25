using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texemon.Scenes.StatusScene
{
    public enum ClassType
    {
        Inventor,
        Android,
        Robot,
        Magician,
        Human,
        Monster,
        Spirit
    }

    public class ClassRecord
    {
        public ClassType ClassType { get; set; }
        public string[] InitialEquipment { get; set; }
        public string[] InitialAbilities { get; set; }
        public BattleScene.BattlerModel BattlerModel { get; set; }
    }
}
