using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Texemon.SceneObjects.Controllers;
using Texemon.SceneObjects.Particles;

namespace Texemon.Scenes.BattleScene
{
    public class BattleController : ScriptController
    {
        private BattleScene battleScene;
        private Battler attacker;
        private Battler target;

        ConversationScene.ConversationScene convoScene;
        double timeleft = 0;

        public BattleController(BattleScene iBattleScene, Battler iAttacker, Battler iTarget, string[] script)
           : base(iBattleScene, script, PriorityLevel.CutsceneLevel)
        {
            battleScene = iBattleScene;
            attacker = iAttacker;
            target = iTarget;
        }

        public override void PreUpdate(GameTime gameTime)
        {
            if (scriptParser.Finished)
            {
                if (convoScene != null)
                {
                    if (convoScene.ConversationViewModel.ReadyToProceed.Value)
                    {
                        timeleft -= gameTime.ElapsedGameTime.TotalMilliseconds;
                        if (timeleft < 0)
                        {
                            convoScene.ConversationViewModel.Terminate();
                            convoScene = null;
                        }
                    }
                }
                else Terminate();
            }
            else scriptParser.Update(gameTime);
        }

        public override bool ExecuteCommand(string[] tokens)
        {
            switch (tokens[0])
            {
                case "Animate": attacker.Animate(tokens[1]); break;
                case "Effect": Effect(tokens); break;
                case "Damage": target.Damage(int.Parse(tokens[1])); break;
                case "Flash": target.FlashColor(new Color(byte.Parse(tokens[1]), byte.Parse(tokens[2]), byte.Parse(tokens[3]))); break;
                case "Attack": Attack(tokens); break;
                case "Dialogue": Dialogue(tokens); break;
                case "Flee": StackDialogue("You flee..."); break;
                case "Defend": StackDialogue("You defend..."); break;
                case "Wait": StackDialogue("You wait..."); break;
                default: return false;
            }

            return true;
        }

        public override string ParseParameter(string parameter)
        {
            switch (parameter)
            {
                case "$targetCenterX": return target.Center.X.ToString();
                case "$targetCenterY": return target.Center.Y.ToString();
                case "$targetTop": return target.Top.Y.ToString();
                case "$targetBottom": return target.Bottom.Y.ToString();
                default: return base.ParseParameter(parameter);
            }
        }

        private void Effect(string[] tokens)
        {
            Vector2 position = new Vector2(int.Parse(tokens[2]), int.Parse(tokens[3]));
            AnimationType animationType = (AnimationType)Enum.Parse(typeof(AnimationType), tokens[1]);
            AnimationParticle animationParticle = new AnimationParticle(battleScene, position, animationType, true);

            if (tokens.Length > 4) animationParticle.AddFrameEvent(int.Parse(tokens[4]), new FrameFollowup(scriptParser.BlockScript()));

            battleScene.AddParticle(animationParticle);
        }

        private void Attack(string[] tokens)
        {
            List<BattlePlayer> eligibleTargets = battleScene.PlayerList.FindAll(x => !x.Dead);
            target = eligibleTargets[Rng.RandomInt(0, eligibleTargets.Count - 1)];

            scriptParser.RunScript("Dialogue prepare yourself\nAnimate Attack\nSound Slash\nEffect Slash $targetCenterX $targetCenterY 3\nFlash 255 27 0\nDamage 25");
        }

        private void Dialogue(string[] tokens)
        {
            if (tokens.Length == 2)
            {
                convoScene = new ConversationScene.ConversationScene(tokens[1], new Rectangle(-20, 30, 170, 80), true);
                var unblock = scriptParser.BlockScript();
                convoScene.ConversationViewModel.OnDialogueScrolled += new Action(unblock);
                CrossPlatformGame.StackScene(convoScene);
            }
            else
            {
                StackDialogue(String.Join(' ', tokens.Skip(1)));
            }

            timeleft = 1000;
        }

        private void StackDialogue(string text)
        {
            var convoRecord = new ConversationScene.ConversationRecord()
            {
                DialogueRecords = new ConversationScene.DialogueRecord[] {
                            new ConversationScene.DialogueRecord() { Text = text }
                        }
            };

            convoScene = new ConversationScene.ConversationScene(convoRecord, new Rectangle(-20, 30, 170, 80), true);
            var unblock = scriptParser.BlockScript();
            convoScene.ConversationViewModel.OnDialogueScrolled += new Action(unblock);
            CrossPlatformGame.StackScene(convoScene);

            timeleft = 1000;
        }
    }
}
