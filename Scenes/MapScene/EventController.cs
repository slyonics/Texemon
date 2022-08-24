using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Texemon.Models;
using Texemon.SceneObjects.Controllers;
using Texemon.SceneObjects.Maps;

namespace Texemon.Scenes.MapScene
{
    public class EventController : ScriptController
    {
        private MapScene mapScene;

        public bool EndGame { get; private set; }

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
                case "Conversation": Conversation(tokens, scriptParser); break;
                case "Encounter": Encounter(tokens, scriptParser); break;
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

        public static void ChangeMap(string[] tokens)
        {
            Type sceneType = Type.GetType(tokens[1]);
            if (tokens.Length == 6) CrossPlatformGame.Transition(sceneType, tokens[2], int.Parse(tokens[3]), int.Parse(tokens[4]), (Orientation)Enum.Parse(typeof(Orientation), tokens[5]));
            else if (tokens.Length == 3) CrossPlatformGame.Transition(typeof(MapScene), tokens[1], tokens[2]);
            else if (tokens.Length == 2) CrossPlatformGame.Transition(sceneType);
            else CrossPlatformGame.Transition(sceneType, tokens[2]);
        }

        public static void SetWaypoint(string[] tokens)
        {
            /*
            mapScene.SetWaypoint(int.Parse(tokens[1]), int.Parse(tokens[2]));
            */
        }

        public static void Conversation(string[] tokens, ScriptParser scriptParser)
        {
            ConversationScene.ConversationScene conversationScene = new ConversationScene.ConversationScene(tokens[1]);
            conversationScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
            CrossPlatformGame.StackScene(conversationScene);
        }

        public static void Encounter(string[] tokens, ScriptParser scriptParser)
        {
            BattleScene.BattleScene matchScene = new BattleScene.BattleScene(tokens[1]);
            matchScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
            CrossPlatformGame.StackScene(matchScene);
        }
    }
}
