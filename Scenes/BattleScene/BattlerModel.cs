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
        public BattlerModel()
        {

        }

        public BattlerModel(EnemyRecord enemyRecord)
        {
            Name.Value = enemyRecord.Name;
            MaxHealth.Value = enemyRecord.Health;
            Health.Value = MaxHealth.Value;
        }

        public ModelProperty<string> Name { get; set; } = new ModelProperty<string>("Enemy");
        public ModelProperty<int> Health { get; set; } = new ModelProperty<int>(50);
        public ModelProperty<int> MaxHealth { get; set; } = new ModelProperty<int>(50);
    }
}
