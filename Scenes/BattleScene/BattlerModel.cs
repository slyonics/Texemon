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
            Health.Value = MaxHealth.Value;
        }

        public BattlerModel(BattlerModel clone)
        {
            Name.Value = clone.Name.Value;
            MaxHealth.Value = clone.MaxHealth.Value;
            Health.Value = MaxHealth.Value;
        }

        public BattlerModel(EnemyRecord enemyRecord)
        {
            Name.Value = enemyRecord.Name;
            MaxHealth.Value = enemyRecord.MaxHealth;
            Health.Value = MaxHealth.Value;
        }

        public ModelProperty<string> Name { get; set; } = new ModelProperty<string>("Enemy");
        public ModelProperty<int> Health { get; set; } = new ModelProperty<int>(50);
        public ModelProperty<int> MaxHealth { get; set; } = new ModelProperty<int>(50);
    }
}
