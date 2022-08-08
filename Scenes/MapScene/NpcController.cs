using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texemon.Scenes.MapScene
{
    public class NpcController : Controller
    {
        public EventController(MapScene iScene, string[] script)
            : base(iScene, script, PriorityLevel.CutsceneLevel)
        {
            mapScene = iScene;
        }

        public override bool ExecuteCommand(string[] tokens)
        {
            switch (tokens[0])
            {
                case "EndGame": EndGame = true; break;
                case "ChangeMap": ChangeMap(tokens); Audio.PlaySound(GameSound.wall_enter); break;
                case "SetWaypoint": SetWaypoint(tokens); break;
                case "Conversation": Conversation(tokens); break;
                case "Encounter": Encounter(tokens); break;
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

        private void ChangeMap(string[] tokens)
        {
            Type sceneType = Type.GetType(tokens[1]);
            if (tokens.Length == 6) CrossPlatformGame.Transition(sceneType, tokens[2], int.Parse(tokens[3]), int.Parse(tokens[4]), (Orientation)Enum.Parse(typeof(Orientation), tokens[5]));
            else if (tokens.Length == 3) CrossPlatformGame.Transition(typeof(MapScene), tokens[1], tokens[2]);
            else if (tokens.Length == 2) CrossPlatformGame.Transition(sceneType);
            else CrossPlatformGame.Transition(sceneType, tokens[2]);
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
