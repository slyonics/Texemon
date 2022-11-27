using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTrainer.Scenes.StatusScene
{
    public enum ItemType
    {
        Weapon,
        Armor,
        Consumable,
        Crafting
    }

    [Serializable]
    public class ItemRecord : CommandRecord
    {
        public ItemRecord()
        {
            
        }

        public ItemRecord(ItemRecord clone)
            : base(clone)
        {
            ItemType = clone.ItemType;

            BonusHealth = clone.BonusHealth;
            BonusStrength = clone.BonusStrength;
            BonusDefense = clone.BonusDefense;
            BonusAgility = clone.BonusAgility;
            BonusMana = clone.BonusMana;

            RobotHealth = clone.RobotHealth;
            RobotStrength = clone.RobotStrength;
            RobotDefense = clone.RobotDefense;
            RobotAgility = clone.RobotAgility;
            RobotMana = clone.RobotMana;
        }

        public ItemType ItemType { get; set; }

        public int BonusHealth { get; set; }
        public int BonusStrength { get; set; }
        public int BonusDefense { get; set; }
        public int BonusAgility { get; set; }
        public int BonusMana { get; set; }


        public int RobotHealth { get; set; }
        public int RobotStrength { get; set; }
        public int RobotDefense { get; set; }
        public int RobotAgility { get; set; }
        public int RobotMana { get; set; }
    }
}
