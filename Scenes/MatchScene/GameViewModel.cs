using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Threading.Tasks;
using Texemon.Models;
using Texemon.SceneObjects.Widgets;

namespace Texemon.Scenes.MatchScene
{
    public class GameViewModel : ViewModel
    {
        private MatchScene matchScene;
        public MapScene.MapScene mapScene;
        private CrawlText crawlText;

        private ViewModel childViewModel;

        public bool ShowAttackTutorial { get; private set; }

        public string BattleStatus { get; private set; } = "";

        public int SwapActions { get; private set; }

        public bool PlayerTurnCountdown = false;

        int buttonSelection = 0;

        public GameViewModel(Scene iScene, GameView viewName)
            : base(iScene, PriorityLevel.GameLevel, viewName)
        {
            matchScene = iScene as MatchScene;

            mapScene = CrossPlatformGame.GetScene<MapScene.MapScene>();

            ShowAttackTutorial = GameProfile.PlayerProfile.ShowAttackTutorial.Value;

            switch (buttonSelection)
            {
                case 0: GetWidget<Button>("AttackBtn").RadioSelect(); break;
                case 1: GetWidget<Button>("PushBtn").RadioSelect(); break;
                case 2: GetWidget<Button>("FleeBtn").RadioSelect(); break;
            }

            crawlText = GetWidget<CrawlText>("DialogueText");

            BattleStatus = matchScene.Enemy.FullName + " appears! ";
            SetDialogue("", BattleStatus + "Command?");

            BeginPlayerTurn();
        }

        private bool IsValidCommand(int selection)
        {
            switch (selection)
            {
                case 0: return CanAttack.Value;
                case 1: return CanPush.Value;
                case 2: return CanFlee.Value;
            }

            return false;
        }

        public override void Update(GameTime gameTime)
        {
            if (childViewModel != null && childViewModel.Terminated) childViewModel = null;

            if (crawlText.ReadyToProceed && !ReadyToProceed.Value) ReadyToProceed.Value = true;

            if (PlayerCommand.Value && childViewModel == null)
            {
                if (Input.CurrentInput.CommandPressed(Command.Right))
                {
                    int oldbutton = buttonSelection;

                    buttonSelection++;
                    if (buttonSelection > 2) buttonSelection = 0;
                    while (!IsValidCommand(buttonSelection))
                    {
                        buttonSelection++;
                        if (buttonSelection > 2) buttonSelection = 0;
                    }

                    switch (buttonSelection)
                    {
                        case 0: GetWidget<Button>("AttackBtn").RadioSelect(); break;
                        case 1: GetWidget<Button>("PushBtn").RadioSelect(); break;
                        case 2: GetWidget<Button>("FleeBtn").RadioSelect(); break;
                    }

                    if (oldbutton != buttonSelection) Audio.PlaySound(GameSound.menu_cursor_change);
                }
                else if (Input.CurrentInput.CommandPressed(Command.Left))
                {
                    int oldbutton = buttonSelection;

                    buttonSelection--;
                    if (buttonSelection < 0) buttonSelection = 2;
                    while (!IsValidCommand(buttonSelection))
                    {
                        buttonSelection--;
                        if (buttonSelection < 0) buttonSelection = 2;
                    }

                    switch (buttonSelection)
                    {
                        case 0: GetWidget<Button>("AttackBtn").RadioSelect(); break;
                        case 1: GetWidget<Button>("PushBtn").RadioSelect(); break;
                        case 2: GetWidget<Button>("FleeBtn").RadioSelect(); break;
                    }

                    if (oldbutton != buttonSelection) Audio.PlaySound(GameSound.menu_cursor_change);
                }
                else if (Input.CurrentInput.CommandPressed(Command.Confirm))
                {
                    Audio.PlaySound(GameSound.menu_select);

                    switch (buttonSelection)
                    {
                        case 0: Attack(); break;
                        case 1: Item(); break;
                        case 2: Flee(); break;
                    }
                }
            }

            base.Update(gameTime);
        }

        public void BeginPlayerTurn()
        {
            PlayerCommand.Value = true;
            PlayerTurn.Value = false;
            CanPush.Value = true;
            EnemyTurn.Value = false;
            AttackName.Value = "";

            if (matchScene.MatchBoard != null)
            {
                int maxColumnSize = matchScene.MatchBoard.HighestColumnSize();
                if (maxColumnSize >= 12) SetDialogue("", "Danger! Command?");
                else SetDialogue("", BattleStatus + "Command?");

                matchScene.MatchBoard.TotalDamage = 0;
                CanAttack.Value = maxColumnSize > 0;
            }

            FixSelection();
        }

        public void FixSelection()
        {
            if (!IsValidCommand(buttonSelection))
            {
                buttonSelection = 0;
                while (!IsValidCommand(buttonSelection))
                {
                    buttonSelection++;
                    if (buttonSelection > 2) buttonSelection = 0;
                }
            }

            switch (buttonSelection)
            {
                case 0: GetWidget<Button>("AttackBtn").RadioSelect(); break;
                case 1: GetWidget<Button>("PushBtn").RadioSelect(); break;
                case 2: GetWidget<Button>("FleeBtn").RadioSelect(); break;
            }
        }

        public void FinishTurn()
        {
            matchScene.GameViewModel.EnemyTurn.Value = true;
            if (matchScene.Enemy.CurrentHealth > 0) matchScene.GameViewModel.BeginEnemyTurn();
            else matchScene.GameViewModel.Victory();
        }

        public void BeginEnemyTurn()
        {
            PlayerCommand.Value = false;
            PlayerTurn.Value = false;
            EnemyTurn.Value = true;

            if (matchScene.MatchBoard.DefeatImminent())
            {
                Audio.PlaySound(GameSound.game_over);

                SetDialogue("", "Defeat...");

                Task.Run(() =>
                {
                    while (!ReadyToProceed.Value) ;
                    matchScene.PromptRetry();
                });

                return;
            }

            AttackModel selectedAttack = Rng.WeightedEntry(matchScene.Enemy.Attacks.ToDictionary(x => x, y => (double)y.Probability));
            if (selectedAttack.Name == "Armor") matchScene.Enemy.Attacks = matchScene.Enemy.Attacks.Where(x => x.Name != "Armor").ToArray();
            AttackName.Value = selectedAttack.FullName;

            AnimatedSprite enemySprite = mapScene.MapViewModel.MapActor.Value;
            enemySprite.PlayAnimation("Talk", Talk1Done);
            Audio.PlaySound(GameSound.enemy_action);

            Task.Delay(500).ContinueWith(t =>
            {
                AttackController attackController = new AttackController(matchScene, selectedAttack.Script);
                attackController.OnTerminated += new TerminationFollowup(() => { matchScene.MatchBoard.ChargeUp(); BeginPlayerTurn(); });
                matchScene.AddController(attackController);
            });
        }

        private void Talk1Done(AnimatedSprite enemySprite)
        {
            enemySprite.PlayAnimation("Idle");
            Task.Delay(50).ContinueWith(t => enemySprite.PlayAnimation("Talk", Talk2Done));
        }

        private void Talk2Done(AnimatedSprite enemySprite)
        {
            enemySprite.PlayAnimation("Idle");
        }

        public void Attack()
        {
            if (!ReadyToProceed.Value) return;

            PlayerCommand.Value = false;
            PlayerTurn.Value = true;
            EnemyTurn.Value = false;

            if (ShowAttackTutorial)
            {
                GameProfile.PlayerProfile.ShowAttackTutorial.Value = false;
            }

            SetDialogue("", "");

            BattleStatus = "The battle continues. ";

            PlayerTurnCountdown = false;

            SetDialogue("", "");

            matchScene.MatchBoard.turnTimer = 15000;
        }

        public void Item()
        {
            if (!ReadyToProceed.Value) return;

            childViewModel = parentScene.AddOverlay(new ItemViewModel(matchScene));
        }

        public void Flee()
        {
            if (!ReadyToProceed.Value) return;

            Audio.PlayMusic(GameMusic.SMP_DUN);

            PlayerCommand.Value = false;
            SetDialogue("", "Got away!");
            mapScene.MapViewModel.SetActor("Actors_Blank");
            mapScene.MapViewModel.ShowHealthBar.Value = false;

            Task.Delay(800).ContinueWith(t => matchScene.EndScene());
        }

        public async void Victory()
        {
            Audio.PlayMusic(GameMusic.SMP_DUN);

            PlayerCommand.Value = false;
            mapScene.MapViewModel.SetActor("Actors_Blank");
            mapScene.MapViewModel.ShowHealthBar.Value = false;

            string victoryText = "Defeated " + matchScene.Enemy.FullName + "!";

            if (matchScene.Enemy.Drops != null && matchScene.Enemy.Drops.Length > 0)
            {
                DropModel itemDrop = Rng.WeightedEntry(matchScene.Enemy.Drops.ToDictionary(x => x, y => (double)y.Probability));
                ItemModel itemModel = ItemModel.Models.FirstOrDefault(x => x.Name == itemDrop.Name);
                if (itemModel != null)
                {
                    ModelProperty<ItemModel> existingItem = GameProfile.PlayerProfile.Items.FirstOrDefault(x => x.Value.Name == itemModel.Name);
                    if (existingItem != null) existingItem.Value.Quantity++;
                    else GameProfile.PlayerProfile.Items.Add(itemModel);

                    victoryText += " Acquired " + itemModel.Name + "!";
                }
            }

            SetDialogue("", victoryText);

            Task.Run(() => { while (!ReadyToProceed.Value) ; }).ContinueWith(t => Task.Delay(800).ContinueWith(t => matchScene.EndScene()));
        }

        public void SwapAction()
        {
            PlayerTurnCountdown = true;
        }

        public void SetDialogue(string speaker, string dialogue)
        {
            Speaker.Value = speaker.Replace('_', ' ');
            Dialogue.Value = dialogue;
            ReadyToProceed.Value = false;
        }

        public ModelProperty<bool> PlayerCommand { get; set; } = new ModelProperty<bool>(true);
        public ModelProperty<bool> PlayerTurn { get; set; } = new ModelProperty<bool>(false);
        public ModelProperty<bool> EnemyTurn { get; set; } = new ModelProperty<bool>(false);

        public ModelProperty<bool> CanAttack { get; set; } = new ModelProperty<bool>(true);
        public ModelProperty<bool> CanPush { get; set; } = new ModelProperty<bool>(true);
        public ModelProperty<bool> CanFlee { get; set; } = new ModelProperty<bool>(true);

        public ModelProperty<string> AttackName { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Speaker { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Dialogue { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> ConversationFont { get; set; } = new ModelProperty<string>(CrossPlatformGame.Scale == 1 ? "Tooltip" : "BigTooltip");
        public ModelProperty<bool> ReadyToProceed { get; set; } = new ModelProperty<bool>(false);

        public ModelProperty<RenderTarget2D> MatchRender { get; set; } = new ModelProperty<RenderTarget2D>(CrossPlatformGame.GameInstance.matchRender);
    }
}
