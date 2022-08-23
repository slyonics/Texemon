﻿using System;
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
        public string[] Enemies { get; set; }
        public string[] Script { get; set; }

        public static List<EncounterRecord> Records { get; set; }
    }
}
