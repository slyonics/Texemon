using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texemon.Models
{
    [Serializable]
    public class PlayerProfile
    {
        public PlayerProfile()
        {
            Items.Add(new ItemModel(ItemModel.Models.First(x => x.Name == "Medicine")));
            Items.Add(new ItemModel(ItemModel.Models.First(x => x.Name == "SoulSnuff")));
            Items.Add(new ItemModel(ItemModel.Models.First(x => x.Name == "Fire Stone")));
            Items.Add(new ItemModel(ItemModel.Models.First(x => x.Name == "Water Stone")));
            Items.Add(new ItemModel(ItemModel.Models.First(x => x.Name == "Life Stone")));
            Items.Add(new ItemModel(ItemModel.Models.First(x => x.Name == "Star Stone")));
            Items.Add(new ItemModel(ItemModel.Models.First(x => x.Name == "Mind Stone")));
        }

        public ModelCollection<ItemModel> Items { get; set; } = new ModelCollection<ItemModel>();

        public ModelProperty<bool> ShowAttackTutorial { get; set; } = new ModelProperty<bool>(true);
    }
}
