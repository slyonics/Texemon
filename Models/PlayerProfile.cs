using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texemon.Models
{
    [Serializable]
    public class PlayerProfile
    {
        public PlayerProfile()
        {

        }

        public int IdlePoise { get; internal set; }
    }
}
