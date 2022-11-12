using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Texemon.Models;
using Texemon.Scenes.StatusScene;

namespace Texemon.Scenes.BattleScene
{
    [Serializable]
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
            Strength.Value = clone.Strength.Value;
            Defense.Value = clone.Defense.Value;
            Agility.Value = clone.Agility.Value;
            Mana.Value = clone.Mana.Value;
            Evade.ModelList = clone.Evade.ModelList;
        }

        public BattlerModel(EnemyRecord enemyRecord)
        {
            Name.Value = enemyRecord.Name;
            MaxHealth.Value = enemyRecord.MaxHealth;
            Health.Value = MaxHealth.Value;
            Strength.Value = enemyRecord.Strength;
            Defense.Value = enemyRecord.Defense;
            Agility.Value = enemyRecord.Agility;
            Mana.Value = enemyRecord.Mana;

            Evade.ModelList = new List<ModelProperty<int>>();
            if (enemyRecord.Evade != null) foreach (var evadeEntry in enemyRecord.Evade) Evade.Add(evadeEntry);
        }


        public ModelProperty<string> Name { get; set; } = new ModelProperty<string>("Enemy");
        public ModelProperty<ClassType> Class { get; set; } = new ModelProperty<ClassType>(ClassType.Monster);
        public ModelProperty<int> Health { get; set; } = new ModelProperty<int>(10);
        public ModelProperty<int> MaxHealth { get; set; } = new ModelProperty<int>(10);
        public ModelProperty<int> Strength { get; set; } = new ModelProperty<int>(3);
        public ModelProperty<int> Defense { get; set; } = new ModelProperty<int>(3);
        public ModelProperty<int> Agility { get; set; } = new ModelProperty<int>(3);
        public ModelProperty<int> Mana { get; set; } = new ModelProperty<int>(3);
        public ModelCollection<int> Evade { get; set; } = new ModelCollection<int>();
    }
}
