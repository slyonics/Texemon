using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

using MonsterTrainer.Scenes.StatusScene;

namespace MonsterTrainer.Models
{
    [Serializable]
    public class PlayerProfile
    {
        public PlayerProfile()
        {

        }

        public ModelProperty<string> WindowStyle { get; set; } = new ModelProperty<string>("Blank");
        public ModelProperty<string> FrameStyle { get; set; } = new ModelProperty<string>("Blank");
        public ModelProperty<string> SelectedStyle { get; set; } = new ModelProperty<string>("Blank");
        public ModelProperty<string> FrameSelectedStyle { get; set; } = new ModelProperty<string>("Blank");
        public ModelProperty<string> LabelStyle { get; set; } = new ModelProperty<string>("Blank");
        public ModelProperty<GameFont> Font { get; set; } = new ModelProperty<GameFont>(GameFont.Tooltip);

        public ModelCollection<HeroModel> Party { get; set; } = new ModelCollection<HeroModel>();
        public ModelProperty<int> Money { get; set; } = new ModelProperty<int>(50);

        public ModelProperty<string> MonsterName { get; set; } = new ModelProperty<string>("Blank");
        public ModelProperty<int> MonsterLevel { get; set; } = new ModelProperty<int>(1);
        public ModelProperty<int> MonsterHealth { get; set; } = new ModelProperty<int>(10);
        public ModelProperty<int> MonsterHealthMax { get; set; } = new ModelProperty<int>(10);
        public ModelProperty<int> MonsterNext { get; set; } = new ModelProperty<int>(50);
    }
}
