using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texemon.Scenes.StatusScene
{
    [Serializable]
    public class AbilityRecord
    {
        public string Name;
        public string Animation;
        public string[] Description;
        public int Icon;
        public int Charges;
        public int MaxCharges;
        public TargetType Targetting;
        public string[] Script;
    }
}
