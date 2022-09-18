using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Texemon.Models;
using Texemon.SceneObjects.Controllers;
using Texemon.SceneObjects.Maps;
using Texemon.Scenes.ConversationScene;

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
                case "ChangeMap": ChangeMap(tokens, mapScene); break;
                case "SetWaypoint": SetWaypoint(tokens); break;
                case "Conversation": Conversation(tokens, scriptParser); break;
                case "Encounter": Encounter(tokens, scriptParser); break;
                case "Shop": Shop(tokens); break;
                case "GiveItem": GiveItem(tokens); break;
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

        public static void ChangeMap(string[] tokens, MapScene mapScene)
        {
            if (tokens.Length == 5) CrossPlatformGame.Transition(typeof(MapScene), tokens[1], int.Parse(tokens[2]), int.Parse(tokens[3]), (Orientation)Enum.Parse(typeof(Orientation), tokens[4]));
            else if (tokens.Length == 2) CrossPlatformGame.Transition(typeof(MapScene), tokens[1], mapScene.Tilemap.Name);
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
            BattleScene.BattleScene battleScene = new BattleScene.BattleScene(tokens[1]);
            battleScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
            CrossPlatformGame.StackScene(battleScene);
        }

        public void Shop(string[] tokens)
        {
            ShopScene.ShopScene shopScene = new ShopScene.ShopScene(tokens[1]);
            shopScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
            CrossPlatformGame.StackScene(shopScene);
        }

        public void GiveItem(string[] tokens)
        {
            StatusScene.ItemRecord item = new StatusScene.ItemRecord(StatusScene.StatusScene.ITEMS.First(x => x.Name == string.Join(' ', tokens.Skip(1))));
            GameProfile.Inventory.Add(item);

            ConversationRecord conversationData = new ConversationRecord()
            {
                DialogueRecords = new DialogueRecord[]
                {
                    new DialogueRecord() { Text = "Found @" + item.Icon + " " + item.Name + "!"}
                }
            };

            ConversationScene.ConversationScene conversationScene = new ConversationScene.ConversationScene(conversationData);
            conversationScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
            CrossPlatformGame.StackScene(conversationScene);

            Audio.PlaySound(GameSound.GetItem);
        }
    }
}
