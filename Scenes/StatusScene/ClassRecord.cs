using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTrainer.Scenes.StatusScene
{
    public enum ClassType
    {
        Android,
        Drone,
        Human,
        Monster,
        Spirit
    }

    public class ClassRecord
    {
        public ClassType ClassType { get; set; }
        public string[] InitialEquipment { get; set; }
        public string[] InitialAbilities { get; set; }
        public string[] Actions { get; set; }
    }
}
