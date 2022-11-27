using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTrainer.Scenes.StatusScene
{
    public enum HeroType
    {
        Inventor,
        TechHero,
        RepairDrone,
        DeflectorDrone,
        BlowtorchDrone,
        ArcWelderDrone,
        TechnoMage,
        NoviceWarrior,
        NoviceMage
    }

    public enum ElementType
    {
        Physical,
        Fire,
        Electric,
        Nuclear,
        Poison
    }

    public class HeroRecord
    {
        public HeroType Name { get; set; }
        public ClassType Class { get; set; }
        public string Sprite { get; set; }
        public double HealthGrowth { get; set; }
        public double StrengthGrowth { get; set; }
        public double DefenseGrowth { get; set; }
        public double AgilityGrowth { get; set; }
        public double ManaGrowth { get; set; }
        public int FlightHeight { get; set; }
        public int EquipmentSlots { get; set; } = 6;
        public string[] InitialEquipment { get; set; }
        public string[] InitialAbilities { get; set; }
        public string[] Actions { get; set; }
        public BattleScene.BattlerModel BattlerModel { get; set; }
    }
}
