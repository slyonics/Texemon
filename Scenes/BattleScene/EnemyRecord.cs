using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Texemon.Models;

namespace Texemon.Scenes.BattleScene
{
    [Serializable]
    public class EnemyRecord
    {
        public string Name { get; set; }
        public string Sprite { get; set; }
    }
}
