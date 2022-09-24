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
        }

        public ItemType ItemType { get; set; }
    }
}
