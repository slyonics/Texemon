using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MonsterTrainer.SceneObjects.Controllers;
using MonsterTrainer.SceneObjects.Particles;

namespace MonsterTrainer.Scenes.BattleScene
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

            if (target == null && attacker is BattleEnemy)
            {
                List<BattlePlayer> eligibleTargets = battleScene.PlayerList.FindAll(x => !x.Dead);
                target = eligibleTargets[Rng.RandomInt(0, eligibleTargets.Count - 1)];
            }
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
                case "Exercise": (attacker as BattlePlayer).Exercise(tokens[1]); break;
                case "Damage": CalculateDamage(tokens); break;
                case "Heal": CalculateHealing(tokens); break;
                case "Repair": target.Repair(int.Parse(tokens[1])); break;
                case "Flash": target.FlashColor(new Color(byte.Parse(tokens[1]), byte.Parse(tokens[2]), byte.Parse(tokens[3]))); break;
                case "Attack": Attack(tokens); break;
                case "Dialogue": Dialogue(tokens); break;
                case "Flee": Flee(tokens); break;
                case "Punch": Punch(tokens); break;
                case "Tackle": Tackle(tokens); break;
                case "Defend": attacker.Defending = true; StackDialogue(attacker.Stats.Name.Value + " is defending against attacks..."); break;
                case "Delay": attacker.Delaying = true; StackDialogue(attacker.Stats.Name.Value + " is waiting for the right moment to act..."); break;
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
                case "$targetName": return target.Stats.Name.Value;
                case "$attackerName": return attacker.Stats.Name.Value;
                case "$attackerNameEx": return attacker.Stats.Name.Value + "!";
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
            int attackerHit = 0;
            if (tokens[2] == "Strength") attackerHit = attacker.Stats.Strength.Value * 2;
            else if (tokens[2] == "Agility") attackerHit = attacker.Stats.Agility.Value * 2;
            int targetEvade = Math.Max(target.Stats.Defense.Value, Math.Max(target.Stats.Strength.Value, target.Stats.Agility.Value)) * 2;

            if (Rng.RandomInt(1, 100) <= accuracy + attackerHit - targetEvade)
            {
                return true;
            }
            else
            {
                Audio.PlaySound(GameSound.Miss);
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
                    damage -= target.Stats.Defense.Value * 5;

                    goto dealDamage;
                }
            }

            int multiplier = int.Parse(tokens[2]);
            string element = tokens[3];
            if (element == "Physical") damage = stat * multiplier + Rng.RandomInt(0, stat) - target.Stats.Defense.Value * 5;
            else damage = (int)((stat * multiplier + Rng.RandomInt(0, stat)) / 5.0f * (200 - target.Stats.Mana.Value) / 40.0f);
            
            dealDamage:

            if (damage < 1) damage = 1;
            target.Damage(damage);
        }

        private void CalculateHealing(string[] tokens)
        {
            if (tokens.Length == 2) target.Heal(int.Parse(tokens[1]));
            else
            {
                int healing = (attacker.Stats.Mana.Value + target.Stats.Mana.Value) * 5;
            }
        }

        private void Attack(string[] tokens)
        {
            List<BattlePlayer> eligibleTargets = battleScene.PlayerList.FindAll(x => !x.Dead);
            target = eligibleTargets[Rng.RandomInt(0, eligibleTargets.Count - 1)];

            scriptParser.RunScript("Dialogue " + attacker.Stats.Name + " attacks " + target.Stats.Name + "!\nAnimate Attack\nSound Slash\nEffect Bash $targetCenterX $targetCenterY 3\nOnHit 100 Strength\nFlash 255 27 0\nDamage Strength 5 Physical");
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

            foreach (BattlePlayer battlePlayer in battleScene.PlayerList)
            {
                foreach (var equipment in battlePlayer.HeroModel.Equipment)
                {
                    if (equipment.Value.ItemType != StatusScene.ItemType.Consumable) equipment.Value.ChargesLeft = equipment.Value.Charges;
                }
            }

            convoScene = new ConversationScene.ConversationScene(convoRecord, new Rectangle(-20, 30, 170, 80));
            scriptParser.BlockScript();
            convoScene.OnTerminated += battleScene.BattleViewModel.Close;
            CrossPlatformGame.StackScene(convoScene);

            // timeleft = 1000;
        }

        private void Punch(string[] tokens)
        {
            scriptParser.RunScript("Dialogue " + attacker.Stats.Name + " punches " + target.Stats.Name + "!\nAnimate Attack\nSound Slash\nEffect Bash $targetCenterX $targetCenterY 3\nOnHit 80 Strength\nFlash 255 27 0\nDamage Strength 5 Physical");
        }

        private void Tackle(string[] tokens)
        {
            scriptParser.RunScript("Dialogue " + attacker.Stats.Name + " tackles " + target.Stats.Name + "!\nAnimate Attack\nSound Slash\nEffect Bash $targetCenterX $targetCenterY 3\nOnHit 80 Strength\nFlash 255 27 0\nDamage Strength 5 Physical");
        }

        private void Dialogue(string[] tokens)
        {
            if (tokens.Length == 2)
            {
                convoScene = new ConversationScene.ConversationScene(tokens[1], new Rectangle(-20, 30, 170, 80), true);
                var unblock = scriptParser.BlockScript();
                convoScene.ConversationViewModel.OnDialogueScrolled += new Action(unblock);
                CrossPlatformGame.StackScene(convoScene);

                // timeleft = 1000;
            }
            else
            {
                StackDialogue(String.Join(' ', tokens.Skip(1)));
            }
        }

        private void StackDialogue(string text)
        {
            var convoRecord = new ConversationScene.ConversationRecord()
            {
                DialogueRecords = new ConversationScene.DialogueRecord[] { new ConversationScene.DialogueRecord() { Text = text} }
            };

            convoScene = new ConversationScene.ConversationScene(convoRecord, new Rectangle(-20, 30, 170, 80), true);
            var unblock = scriptParser.BlockScript();
            convoScene.ConversationViewModel.OnDialogueScrolled += new Action(unblock);
            CrossPlatformGame.StackScene(convoScene);

            timeleft = 1000;
        }

        public static void IncreaseStat(string[] tokens)
        {
            int characterIndex = int.Parse(tokens[1]);
            Scenes.StatusScene.HeroModel heroModel = Models.GameProfile.PlayerProfile.Party[characterIndex];
            string stat = tokens[2];
            int increaseAmount = int.Parse(tokens[3]);

            switch (stat)
            {
                case "Health": heroModel.MaxHealth.Value += increaseAmount; heroModel.NakedHealth.Value += increaseAmount; break;
                case "Strength": heroModel.Strength.Value += increaseAmount; heroModel.NakedStrength.Value += increaseAmount; break;
                case "Defense": heroModel.Defense.Value += increaseAmount; heroModel.NakedDefense.Value += increaseAmount; break;
                case "Agility": heroModel.Agility.Value += increaseAmount; heroModel.NakedAgility.Value += increaseAmount; break;
                case "Mana": heroModel.Mana.Value += increaseAmount; heroModel.NakedMana.Value += increaseAmount; break;
            }
        }
    }
}
