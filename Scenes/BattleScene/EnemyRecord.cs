using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Texemon.Models;

namespace Texemon.Scenes.BattleScene
{
    public class AttackData
    {
        public string[] Script;
        public int Weight;
    }

    [Serializable]
    public class EnemyRecord
    {
        public string Name { get; set; }
        public string Sprite { get; set; }
        public int ShadowOffset { get; set; }
        public int Health { get; set; }
        public AttackData[] Attacks { get; set; }
    }
}
