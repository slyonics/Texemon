using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Texemon.Models;
using Texemon.Scenes.BattleScene;

namespace Texemon.Scenes.StatusScene
{
    [Serializable]
    public class HeroModel : BattlerModel
    {
        public HeroModel(HeroType heroType)
            : base(StatusScene.HEROES.First(x => x.Name == heroType).BattlerModel)
        {
            HeroRecord heroRecord = StatusScene.HEROES.First(x => x.Name == heroType);
            ClassRecord classRecord = StatusScene.CLASSES.First(x => x.ClassType == heroRecord.Class);

            Class.Value = heroRecord.Class;

            EquipmentSlots.Value = heroRecord.EquipmentSlots;

            HealthGrowth.Value = heroRecord.HealthGrowth;
            StrengthGrowth.Value = heroRecord.StrengthGrowth;
            DefenseGrowth.Value = heroRecord.DefenseGrowth;
            AgilityGrowth.Value = heroRecord.AgilityGrowth;
            ManaGrowth.Value = heroRecord.ManaGrowth;

            NakedHealth.Value = Health.Value;
            NakedStrength.Value = Strength.Value;
            NakedDefense.Value = Defense.Value;
            NakedAgility.Value = Agility.Value;
            NakedMana.Value = Mana.Value;

            Sprite.Value = (GameSprite)Enum.Parse(typeof(GameSprite), "Actors_" + heroRecord.Sprite);
            if (heroRecord.FlightHeight > 0)
            {
                FlightHeight.Value = heroRecord.FlightHeight;
                ShadowSprite.Value = GameSprite.Actors_DroneShadow;
            }

            foreach (string equipment in classRecord.InitialEquipment)
            {
                ItemRecord item = StatusScene.ITEMS.First(x => x.Name == equipment);
                item.ChargesLeft = item.Charges;
                Equipment.Add(new ItemRecord(item));

                Health.Value += item.BonusHealth;
                Strength.Value += item.BonusStrength;
                Defense.Value += item.BonusDefense;
                Agility.Value += item.BonusAgility;
                Mana.Value += item.BonusMana;
                if (Class.Value == ClassType.Android || Class.Value == ClassType.Drone)
                {
                    Health.Value += item.RobotHealth;
                    Strength.Value += item.RobotStrength;
                    Defense.Value += item.RobotDefense;
                    Agility.Value += item.RobotAgility;
                    Mana.Value += item.RobotMana;
                }
                MaxHealth.Value = Health.Value;
            }

            foreach (string equipment in heroRecord.InitialEquipment)
            {
                ItemRecord item = StatusScene.ITEMS.First(x => x.Name == equipment);
                item.ChargesLeft = item.Charges;
                Equipment.Add(new ItemRecord(item));

                Health.Value += item.BonusHealth;
                Strength.Value += item.BonusStrength;
                Defense.Value += item.BonusDefense;
                Agility.Value += item.BonusAgility;
                Mana.Value += item.BonusMana;
                if (Class.Value == ClassType.Android || Class.Value == ClassType.Drone)
                {
                    Health.Value += item.RobotHealth;
                    Strength.Value += item.RobotStrength;
                    Defense.Value += item.RobotDefense;
                    Agility.Value += item.RobotAgility;
                    Mana.Value += item.RobotMana;
                }
                MaxHealth.Value = Health.Value;
            }

            foreach (string ability in classRecord.InitialAbilities)
            {
                AbilityRecord item = StatusScene.ABILITIES.First(x => x.Name == ability);
                item.ChargesLeft = item.Charges;
                Abilities.Add(new AbilityRecord(item));
            }

            foreach (string ability in heroRecord.InitialAbilities)
            {
                AbilityRecord item = StatusScene.ABILITIES.First(x => x.Name == ability);
                item.ChargesLeft = item.Charges;
                Abilities.Add(new AbilityRecord(item));
            }

            foreach (string action in classRecord.Actions)
            {
                CommandRecord item = new CommandRecord()
                {
                    Name = action,
                    Icon = action,
                    Targetting = TargetType.Self,
                    Script = new string[] { action }
                };
                switch (action)
                {
                    case "Delay": item.Description = new string[] { "Hold turn briefly", "and act later" }; break;
                    case "Defend": item.Description = new string[] { "Spend turn", "evading and", "blocking attacks" }; break;
                    case "Flee": item.Description = new string[] { "Try and escape", "from battle" }; break;
                }
                Actions.Add(item);
            }

            foreach (string action in heroRecord.Actions)
            {

            }
        }

        public ModelProperty<GameSprite> Sprite { get; set; } = new ModelProperty<GameSprite>(GameSprite.Actors_Base);
        public ModelProperty<GameSprite> ShadowSprite { get; set; } = new ModelProperty<GameSprite>();
        public ModelProperty<int> FlightHeight { get; set; } = new ModelProperty<int>(0);

        [field: NonSerialized]
        public ModelProperty<Color> NameColor { get; set; } = new ModelProperty<Color>(new Color(252, 252, 252, 255));
        [field: NonSerialized]
        public ModelProperty<Color> HealthColor { get; set; } = new ModelProperty<Color>(new Color(252, 252, 252, 255));

        public ModelProperty<int> LastCategory { get; private set; } = new ModelProperty<int>(0);
        public ModelProperty<int> LastSlot { get; private set; } = new ModelProperty<int>(0);
        public ModelProperty<int> EquipmentSlots { get; private set; } = new ModelProperty<int>(6);
        public ModelCollection<ItemRecord> Equipment { get; set; } = new ModelCollection<ItemRecord>();
        public ModelCollection<AbilityRecord> Abilities { get; set; } = new ModelCollection<AbilityRecord>();
        public ModelCollection<CommandRecord> Actions { get; set; } = new ModelCollection<CommandRecord>();
        public ModelProperty<double> HealthGrowth { get; set; } = new ModelProperty<double>(0);
        public ModelProperty<double> StrengthGrowth { get; set; } = new ModelProperty<double>(0);
        public ModelProperty<double> DefenseGrowth { get; set; } = new ModelProperty<double>(0);
        public ModelProperty<double> AgilityGrowth { get; set; } = new ModelProperty<double>(0);
        public ModelProperty<double> ManaGrowth { get; set; } = new ModelProperty<double>(0);

        public ModelProperty<int> NakedHealth { get; set; } = new ModelProperty<int>(0);
        public ModelProperty<int> NakedStrength { get; set; } = new ModelProperty<int>(0);
        public ModelProperty<int> NakedDefense { get; set; } = new ModelProperty<int>(0);
        public ModelProperty<int> NakedAgility { get; set; } = new ModelProperty<int>(0);
        public ModelProperty<int> NakedMana { get; set; } = new ModelProperty<int>(0);
    }
}
