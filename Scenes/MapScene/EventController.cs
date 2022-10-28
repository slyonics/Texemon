using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Texemon.Models;
using Texemon.SceneObjects.Controllers;
using Texemon.SceneObjects.Maps;
using Texemon.Scenes.ConversationScene;
using Texemon.Scenes.StatusScene;

namespace Texemon.Scenes.MapScene
{
    public class EventController : ScriptController
    {
        private MapScene mapScene;

        public bool EndGame { get; private set; }
        public Actor ActorSubject { get; set; }

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
                case "Inn": Inn(tokens); break;
                case "RestoreParty": RestoreParty(); break;
                case "Recruit": Recruit(tokens); break;
                default: return false;
            }

            return true;
        }

        public override string ParseParameter(string parameter)
        {
            if (parameter.StartsWith("$SaveData."))
            {
                return GameProfile.GetSaveData<string>(parameter.Split('.')[1]).ToString();
            }
            else if (parameter[0] == '$')
            {
                switch (parameter)
                {
                    
                    default: return null;
                }
            }
            else return null;
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

        public void Inn(string[] tokens)
        {
            ConversationScene.ConversationScene conversationScene = new ConversationScene.ConversationScene("Inn");
            conversationScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());            

            TransitionController transitionOutController = new TransitionController(TransitionDirection.Out, 600);
            SceneObjects.Shaders.ColorFade colorFadeOut = new SceneObjects.Shaders.ColorFade(Color.Black, transitionOutController.TransitionProgress);
            transitionOutController.UpdateTransition += new Action<float>(t => colorFadeOut.Amount = t);
            transitionOutController.FinishTransition += new Action<TransitionDirection>(t =>
            {
                Audio.PauseMusic(true);
                Audio.PlaySound(GameSound.Rest);
                Task.Delay(1500).ContinueWith(t => Audio.PauseMusic(false));

                transitionOutController.Terminate();
                colorFadeOut.Terminate();
                TransitionController transitionInController = new TransitionController(TransitionDirection.In, 600);
                SceneObjects.Shaders.ColorFade colorFadeIn = new SceneObjects.Shaders.ColorFade(Color.Black, transitionInController.TransitionProgress);
                transitionInController.UpdateTransition += new Action<float>(t => colorFadeIn.Amount = t);
                transitionInController.FinishTransition += new Action<TransitionDirection>(t =>
                {
                    colorFadeIn.Terminate();                    
                });
                mapScene.AddController(transitionInController);
                mapScene.SceneShader = colorFadeIn;

                CrossPlatformGame.StackScene(conversationScene);
            });

            mapScene.AddController(transitionOutController);
            mapScene.SceneShader = colorFadeOut;

            RestoreParty();
        }

        public void RestoreParty()
        {
            foreach (var partyMember in GameProfile.PlayerProfile.Party)
            {
                partyMember.Value.Health.Value = partyMember.Value.MaxHealth.Value;
                foreach (var ability in partyMember.Value.Abilities)
                {
                    ability.Value.ChargesLeft = ability.Value.Charges;
                }
            }
        }

        public void Recruit(string[] tokens)
        {
            HeroModel heroModel = new HeroModel((HeroType)Enum.Parse(typeof(HeroType), tokens[1]));
            //heroModel.Name.Value = namingBox.Text;
            // TODO overflow party to backbench
            GameProfile.PlayerProfile.Party.Add(heroModel);
            GameProfile.SetSaveData<bool>(heroModel.Name.Value + "Recruited", true);

            mapScene.AddPartyMember(heroModel, ActorSubject);

            if (tokens.Length >= 2) ActorSubject.Terminate();
        }
    }
}
