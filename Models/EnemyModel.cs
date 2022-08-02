using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texemon.Models
{
    [Serializable]
    public class EnemyModel
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public string SpriteName { get; set; }
        public int MaxHealth { get; set; }
        public int CurrentHealth { get; set; }

        public AttackModel[] Attacks { get; set; }
        public DropModel[] Drops { get; set; }
        public int Armor { get; internal set; }

        public static List<EnemyModel> Models { get; set; }
    }

    public class AttackModel
    {
        public string Name { get; set; }
        public int Probability { get; set; }
        public string FullName { get; set; }
        public string[] Script { get; set; }
    }

    public class DropModel
    {
        public string Name { get; set; }
        public int Probability { get; set; }
    }
}
