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
                case "Exercise": Exercise(tokens); break;
                case "Damage": CalculateDamage(tokens); break;
                case "Heal": target.Heal(int.Parse(tokens[1])); break;
                case "Repair": target.Repair(int.Parse(tokens[1])); break;
                case "Flash": target.FlashColor(new Color(byte.Parse(tokens[1]), byte.Parse(tokens[2]), byte.Parse(tokens[3]))); break;
                case "Attack": Attack(tokens); break;
                case "Dialogue": Dialogue(tokens); break;
                case "Flee": Flee(tokens); break;
                case "Defend": StackDialogue("You defend..."); break;
                case "Delay": StackDialogue("You delay..."); break;
                case "OnHit": if (!CalculateHit(tokens)) scriptParser.EndScript(); break;
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
            attacker.ParticleList.Add(animationParticle);
        }

        private bool CalculateHit(string[] tokens)
        {
            int accuracy = int.Parse(tokens[1]);
            if (Rng.RandomInt(1, 100) <= accuracy)
            {
                return true;
            }
            else
            {
                Audio.PlaySound(GameSound.clear);
                target.Miss();

                return false;
            }
        }

        private void CalculateDamage(string[] tokens)
        {
            string statName = tokens[1];

            int damage = 0;
            int stat = 0;
            switch (statName)
            {
                case "Strength": stat = attacker.Stats.Strength.Value; break;
                case "Agility": stat = attacker.Stats.Agility.Value; break;
                case "Mana": stat = attacker.Stats.Mana.Value; break;

                default:
                {
                    if (tokens.Length == 3) damage = int.Parse(tokens[1]);
                    else damage = Rng.RandomInt(int.Parse(tokens[1]), int.Parse(tokens[2]));
                    damage += Rng.RandomInt(0, 9);
                    goto dealDamage;
                }
            }

            int multiplier = int.Parse(tokens[2]);
            string element = tokens[3];
            if (element == "Physical") damage = stat * multiplier + Rng.RandomInt(0, stat) - target.Stats.Defense.Value * 5;
            else damage = (int)(((200 - target.Stats.Mana.Value) * stat * multiplier + Rng.RandomInt(0, stat)) / 20.0f);
            if (damage < 1) damage = 1;

            dealDamage:

            target.Damage(damage);
        }

        private void Exercise(string[] tokens)
        {

        }

        private void Attack(string[] tokens)
        {
            List<BattlePlayer> eligibleTargets = battleScene.PlayerList.FindAll(x => !x.Dead);
            target = eligibleTargets[Rng.RandomInt(0, eligibleTargets.Count - 1)];

            scriptParser.RunScript("Dialogue " + attacker.Stats.Name + " attacks " + target.Stats.Name + "!\nAnimate Attack\nSound Slash\nEffect Bash $targetCenterX $targetCenterY 3\nFlash 255 27 0\nDamage Strength 5 Physical");
        }

        private void Flee(string[] tokens)
        {
            //StackDialogue("You flee...");

            var convoRecord = new ConversationScene.ConversationRecord()
            {
                DialogueRecords = new ConversationScene.DialogueRecord[] {
                            new ConversationScene.DialogueRecord() { Text = "Escaped from battle." }
                        }
            };

            convoScene = new ConversationScene.ConversationScene(convoRecord, new Rectangle(-20, 30, 170, 80), false);
            scriptParser.BlockScript();
            convoScene.OnTerminated += battleScene.BattleViewModel.Close;
            CrossPlatformGame.StackScene(convoScene);

            timeleft = 1000;
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
