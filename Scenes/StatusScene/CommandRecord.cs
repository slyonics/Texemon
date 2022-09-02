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
