using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterLegends.Scenes.StatusScene
{
    [Serializable]
    public class AbilityRecord : CommandRecord
    {
        public AbilityRecord()
        {

        }

        public AbilityRecord(AbilityRecord clone)
            : base(clone)
        {

        }
    }
}
