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

        private DialogueView dialogueView;

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
                if (dialogueView == null || dialogueView.Terminated) Terminate();
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
                default: return false;
            }

            return true;
        }

        public override string ParseParameter(string parameter)
        {
            switch (parameter)
            {
                case "$targetCenterX": return target.SpriteBounds.Center.X.ToString();
                case "$targetCenterY": return target.SpriteBounds.Center.Y.ToString();
                case "$targetTop": return target.SpriteBounds.Top.ToString();
                case "$targetY": return ((int)target.Position.Y).ToString();
                default: return base.ParseParameter(parameter);
            }
        }

        private void Effect(string[] tokens)
        {
            Vector2 position = new Vector2(int.Parse(tokens[2]), int.Parse(tokens[3]));
            AnimationType animationType = (AnimationType)Enum.Parse(typeof(AnimationType), tokens[1]);
            AnimationParticle animationParticle = new AnimationParticle(battleScene, position, animationType, true);

            if (tokens.Length > 4) animationParticle.AddFrameEvent(int.Parse(tokens[4]), scriptParser.BlockForFrame());

            battleScene.AddParticle(animationParticle);
        }

        private void Attack(string[] tokens)
        {
            List<BattlePlayer> eligibleTargets = battleScene.PlayerList.FindAll(x => !x.Dead);
            target = eligibleTargets[Rng.RandomInt(0, eligibleTargets.Count - 1)];

            scriptParser.RunScript("Animate Attack\nSound Slash\nEffect Slash $targetCenterX $targetCenterY 3\nFlash 255 27 0\nDamage 5");
        }

        private void Dialogue(string[] tokens)
        {
            string dialogue = "";
            for (int i = 3; i < tokens.Length; i++) dialogue += tokens[i] + " ";
            dialogue.TrimEnd();

            if (dialogueView == null)
            {
                dialogueView = new DialogueView(battleScene, BattleScene.COMMAND_WINDOW, dialogue);
                dialogueView.CrawlFactor = int.Parse(tokens[1]);
                dialogueView.Sound = GameSound.None;
                battleScene.AddView(dialogueView);

                DialogueController dialogueController = new DialogueController(PriorityLevel.GameLevel, dialogueView, int.Parse(tokens[2]));
                dialogueController.OnFinishScrolling += scriptParser.BlockForDialogueScrolling();
                battleScene.AddController(dialogueController);
            }
            else dialogueView.Enqueue(dialogue);
        }
    }
}
