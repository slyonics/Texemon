using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Texemon.Models;

namespace Texemon.Scenes.BattleScene
{
    public class BattlerModel
    {
        public ModelProperty<string> Name { get; set; }
        public ModelProperty<int> Health { get; set; }
        public ModelProperty<int> MaxHealth { get; set; }
    }
}
