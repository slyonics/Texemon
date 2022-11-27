using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterLegends.Scenes.ShopScene
{
    public class CostRecord
    {
        public int Money { get; set; }
        public string Item { get; set; }
        public string Icon { get; set; }

        public Color CostColor { get; set; }
        public int Have { get; set; }
        public int Need { get; set; }
    }

    public class VoucherRecord
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public CostRecord[] Cost { get; set; }
        public AnimatedSprite Sprite { get; set; }
    }

    public class ShopRecord
    {
        public string Name { get; set; }
        public string Intro { get; set; }

        public VoucherRecord[] Vouchers { get; set; }
    }
}
