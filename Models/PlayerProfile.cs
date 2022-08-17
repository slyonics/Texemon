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

        public ModelProperty<string> WindowStyle { get; set; } = new ModelProperty<string>("Blank");
        public ModelProperty<string> FrameStyle { get; set; } = new ModelProperty<string>("Blank");
        public ModelProperty<string> SelectedStyle { get; set; } = new ModelProperty<string>("Blank");
        public ModelProperty<string> LabelStyle { get; set; } = new ModelProperty<string>("Blank");
        public ModelProperty<GameFont> Font { get; set; } = new ModelProperty<GameFont>(GameFont.Silver);
    }
}
