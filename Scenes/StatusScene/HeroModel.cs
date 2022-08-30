﻿using System;
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
        public HeroModel(ClassType classType)
            : base(StatusScene.CLASSES.First(x => x.ClassType == classType).BattlerModel)
        {
            ClassRecord classRecord = StatusScene.CLASSES.First(x => x.ClassType == classType);
            Sprite.Value = (GameSprite)Enum.Parse(typeof(GameSprite), "Actors_" + classRecord.Sprite);

            foreach (string equipment in classRecord.InitialEquipment)
            {
                ItemRecord item = StatusScene.ITEMS.First(x => x.Name == equipment);
                Equipment.Add(item);
            }

            foreach (string ability in classRecord.InitialAbilities)
            {
                AbilityRecord item = StatusScene.ABILITIES.First(x => x.Name == ability);
                Abilities.Add(item);
            }

            foreach (string action in classRecord.Actions)
            {
                CommandRecord item = new CommandRecord()
                {
                    Name = action,
                    Icon = action,
                };
                Actions.Add(item);
            }
        }

        public ModelProperty<GameSprite> Sprite { get; set; } = new ModelProperty<GameSprite>(GameSprite.Actors_AirDrone);
        public ModelProperty<Color> NameColor { get; private set; } = new ModelProperty<Color>(Color.White);
        public ModelProperty<int> LastCategory { get; private set; } = new ModelProperty<int>(0);
        public ModelProperty<int> LastSlot { get; private set; } = new ModelProperty<int>(0);
        public ModelCollection<CommandRecord> Equipment { get; set; } = new ModelCollection<CommandRecord>();
        public ModelCollection<CommandRecord> Abilities { get; set; } = new ModelCollection<CommandRecord>();
        public ModelCollection<CommandRecord> Actions { get; set; } = new ModelCollection<CommandRecord>();
    }
}
