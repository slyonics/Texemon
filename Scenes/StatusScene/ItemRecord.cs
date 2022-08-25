using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texemon.Scenes.StatusScene
{
    public enum ItemType
    {
        Weapon,
        Armor,
        Consumable
    }

    public enum TargetType
    {
        SingleEnemy,
        AllEnemy,
        SingleAlly,
        AllAlly,
        All,
        Self,
        Auto
    }

    [Serializable]
    public class ItemRecord
    {
        public string Name;
        public ItemType ItemType;
        public string Animation;
        public string[] Description;
        public int Icon;
        public int Charges;
        public int MaxCharges;
        public TargetType Targetting;
        public string[] Script;
    }
}
