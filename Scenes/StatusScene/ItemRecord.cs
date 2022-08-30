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
    public class ItemRecord : CommandRecord
    {
        public ItemType ItemType { get; set; }
    }
}
