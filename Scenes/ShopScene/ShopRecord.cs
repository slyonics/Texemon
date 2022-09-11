using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texemon.Scenes.ShopScene
{
    public class CostRecord
    {
        public int Money { get; set; }
        public string[] Items { get; set; }
    }

    public class VoucherRecord
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
    }

    public class ShopRecord
    {
        public string Name { get; set; }
        public string Intro { get; set; }

        public VoucherRecord[] Vouchers { get; set; }
    }
}
