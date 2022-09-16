using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Texemon.Models;

namespace Texemon.Scenes.BattleScene
{
    public class EncounterRecord
    {
        public string Name { get; set; }
        public string Background { get; set; } = "Trees";
        public int Width { get; set; }
        public string[] Enemies { get; set; }
        public string[] Script { get; set; }
        public string Intro { get; set; } = "Your foes assemble...";

        public static List<EncounterRecord> Records { get; set; }
    }
}
