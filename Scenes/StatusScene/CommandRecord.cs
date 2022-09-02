using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texemon.Scenes.StatusScene
{
    public enum TargetType
    {
        SingleEnemy,
        AllEnemy,
        SingleAlly,
        AllAlly,
        //All,
        Self,
        //Auto
    }

    [Serializable]
    public class CommandRecord
    {
        public CommandRecord()
        {

        }

        public CommandRecord(CommandRecord clone)
        {
            Name = clone.Name;
            Animation = clone.Animation;
            Description = (string[])clone.Description.Clone();
            Icon = clone.Icon;
            Charges = clone.Charges;
            MaxCharges = clone.MaxCharges;
            Targetting = clone.Targetting;
            Script = (string[])clone.Script.Clone();
        }

        public string Name { get; set; }
        public string Animation { get; set; }
        public string[] Description { get; set; }
        public string Icon { get; set; }
        public int Charges { get; set; } = -1;
        public int MaxCharges { get; set; } = -1;
        public bool ShowCharges { get => MaxCharges >= 0; }
        public TargetType Targetting { get; set; }
        public string[] Script { get; set; }
    }
}
