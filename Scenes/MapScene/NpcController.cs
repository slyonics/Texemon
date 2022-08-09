using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Texemon.Models;
using Texemon.SceneObjects.Controllers;

namespace Texemon.Scenes.MapScene
{
    public class NpcController : ScriptController
    {
        private MapScene mapScene;

        public NpcController(MapScene iScene, string[] script)
            : base(iScene, script, PriorityLevel.CutsceneLevel)
        {
            mapScene = iScene;
        }

        public override bool ExecuteCommand(string[] tokens)
        {
            switch (tokens[0])
            {
                case "ChangeMap": EventController.ChangeMap(tokens); Audio.PlaySound(GameSound.wall_enter); break;
                case "SetWaypoint": EventController.SetWaypoint(tokens); break;
                case "Conversation": EventController.Conversation(tokens, scriptParser); break;
                case "Encounter": EventController.Encounter(tokens); break;
                default: return false;
            }

            return true;
        }
    }
}
