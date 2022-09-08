using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Texemon.Models;
using Texemon.Scenes.BattleScene;

namespace Texemon.Scenes.StatusScene
{

    public class HeroModel : BattlerModel
    {
        public HeroModel(HeroType heroType)
            : base(StatusScene.HEROES.First(x => x.Name == heroType).BattlerModel)
        {
            HeroRecord heroRecord = StatusScene.HEROES.First(x => x.Name == heroType);
            ClassRecord classRecord = StatusScene.CLASSES.First(x => x.ClassType == heroRecord.Class);
            Sprite.Value = (GameSprite)Enum.Parse(typeof(GameSprite), "Actors_" + heroRecord.Sprite);
            if (heroRecord.FlightHeight > 0)
            {
                FlightHeight.Value = heroRecord.FlightHeight;
                ShadowSprite.Value = GameSprite.Actors_DroneShadow;
            }

            foreach (string equipment in classRecord.InitialEquipment)
            {
                ItemRecord item = StatusScene.ITEMS.First(x => x.Name == equipment);
                Equipment.Add(new ItemRecord(item));
            }

            foreach (string equipment in heroRecord.InitialEquipment)
            {
                ItemRecord item = StatusScene.ITEMS.First(x => x.Name == equipment);
                Equipment.Add(new ItemRecord(item));
            }

            foreach (string ability in classRecord.InitialAbilities)
            {
                AbilityRecord item = StatusScene.ABILITIES.First(x => x.Name == ability);
                Abilities.Add(new AbilityRecord(item));
            }

            foreach (string ability in heroRecord.InitialAbilities)
            {
                AbilityRecord item = StatusScene.ABILITIES.First(x => x.Name == ability);
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

        public ModelProperty<GameSprite> Sprite { get; set; } = new ModelProperty<GameSprite>(GameSprite.Actors_AirDrone);
        public ModelProperty<GameSprite> ShadowSprite { get; set; } = new ModelProperty<GameSprite>();
        public ModelProperty<int> FlightHeight { get; set; } = new ModelProperty<int>(0);
        public ModelProperty<Color> NameColor { get; private set; } = new ModelProperty<Color>(new Color(252, 252, 252, 255));
        public ModelProperty<Color> HealthColor { get; private set; } = new ModelProperty<Color>(new Color(252, 252, 252, 255));
        public ModelProperty<int> LastCategory { get; private set; } = new ModelProperty<int>(0);
        public ModelProperty<int> LastSlot { get; private set; } = new ModelProperty<int>(0);
        public ModelCollection<CommandRecord> Equipment { get; set; } = new ModelCollection<CommandRecord>();
        public ModelCollection<CommandRecord> Abilities { get; set; } = new ModelCollection<CommandRecord>();
        public ModelCollection<CommandRecord> Actions { get; set; } = new ModelCollection<CommandRecord>();
    }
}
