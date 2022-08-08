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

        public override string ParseParameter(string parameter)
        {
            if (parameter.Contains("Flag."))
            {
                return GameProfile.GetSaveData<bool>(parameter.Split('.')[1]).ToString();
            }
            else return base.ParseParameter(parameter);
        }

        private void SetWaypoint(string[] tokens)
        {
            /*
            mapScene.SetWaypoint(int.Parse(tokens[1]), int.Parse(tokens[2]));
            */
        }

        private void Conversation(string[] tokens)
        {
            ConversationScene.ConversationScene conversationScene = new ConversationScene.ConversationScene(tokens[1]);
            var unblock = scriptParser.BlockScript();
            conversationScene.OnTerminated += new TerminationFollowup(unblock);
            CrossPlatformGame.StackScene(conversationScene);
        }

        private void Encounter(string[] tokens)
        {
            /*
            mapScene.MapViewModel.SetActor("Actors_" + tokens[1]);

            MatchScene.MatchScene matchScene;
            if (tokens.Length > 2) matchScene = new MatchScene.MatchScene(tokens[1], tokens[2]);
            else matchScene = new MatchScene.MatchScene(tokens[1]);
            var unblock = scriptParser.BlockScript();
            matchScene.OnTerminated += new TerminationFollowup(unblock);
            CrossPlatformGame.StackScene(matchScene);
            */
        }
    }
}
