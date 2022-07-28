using System;
using System.Collections.Generic;
using System.Text;

namespace Texemon.Models
{
    [Serializable]
    public class ItemModel
    {
        public ItemModel() { }

        public ItemModel(ItemModel clone)
        {
            Name = clone.Name;
            Icon = clone.Icon;
            Description = clone.Description;
            Quantity = clone.Quantity;
            Cost = clone.Cost;

            Script = new string[clone.Script.Length];
            clone.Script.CopyTo(Script, 0);
        }

        public string Name { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; } = 1;
        public int Cost { get; set; }
        public string[] Script { get; set; }

        public static List<ItemModel> Models { get; set; }
    }
}
