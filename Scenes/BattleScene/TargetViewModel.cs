using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using Microsoft.Xna.Framework.Graphics;

using Texemon.Models;
using Texemon.SceneObjects.Widgets;
using Texemon.Scenes.StatusScene;

namespace Texemon.Scenes.BattleScene
{
    public class TargetViewModel : ViewModel
    {
        public class TargetButton
        {
            public string Name { get; set; }
            public bool NameVisible { get; set; }
            public Rectangle Bounds { get; set; }

            public Battler target;

            public Button button;
        }

        BattleScene battleScene;

        DataGrid targetList;

        int index = -1;

        int confirmCooldown = 100;


        public TargetViewModel(BattleScene iScene, BattlePlayer iPlayer, CommandRecord iCommandRecord)
            : base(iScene, PriorityLevel.GameLevel)
        {
            battleScene = iScene;

            Player = iPlayer;
            Command = iCommandRecord;

            switch (Command.Targetting)
            {
                case TargetType.SingleEnemy:
                    Targets.ModelList = new List<ModelProperty<TargetButton>>();
                    foreach (BattleEnemy battleEnemy in battleScene.EnemyList) Targets.Add(new TargetButton() { Name = battleEnemy.Stats.Name.Value, NameVisible = true, Bounds=battleEnemy.SpriteBounds, target = battleEnemy });
                    break;

                case TargetType.AllEnemy:
                    {
                        Targets.ModelList = new List<ModelProperty<TargetButton>>();
                        Rectangle bounds = battleScene.EnemyList[0].SpriteBounds;
                        for (int i = 1; i < battleScene.EnemyList.Count; i++) bounds = Rectangle.Union(bounds, battleScene.EnemyList[i].SpriteBounds);
                        Targets.Add(new TargetButton() { Name = "All Enemies", NameVisible = true, Bounds = bounds });

                        break;
                    }

                case TargetType.SingleAlly:
                    Targets.ModelList = new List<ModelProperty<TargetButton>>();
                    foreach (BattlePlayer battlePlayer in battleScene.PlayerList)
                    {
                        if (battlePlayer.Dead) continue;

                        Rectangle bounds = battlePlayer.SpriteBounds;
                        Targets.Add(new TargetButton() { Name = "", NameVisible = false, Bounds = bounds, target = battlePlayer });
                    }
                    break;

                case TargetType.Self:
                    Targets.ModelList = new List<ModelProperty<TargetButton>>();
                    Targets.Add(new TargetButton() { Name = "", NameVisible = false, Bounds = Player.SpriteBounds });
                    break;
            }

            LoadView(GameView.BattleScene_TargetView);

            targetList = GetWidget<DataGrid>("TargetList");

            switch (Command.Targetting)
            {
                case TargetType.Self:
                case TargetType.AllEnemy:
                    index = 0;
                    ((Button)targetList.ChildList.First()).RadioSelect();
                    break;

                case TargetType.SingleEnemy:
                    {
                        int i = 0;
                        foreach (ModelProperty<TargetButton> buttonPropery in Targets)
                        {
                            buttonPropery.Value.button = targetList.ChildList.ElementAt(i) as Button;
                            i++;
                        }

                        if (battleScene.BattleViewModel.LastEnemyTarget != null)
                        {
                            index = Targets.ToList().FindIndex(x => x.Value.target == battleScene.BattleViewModel.LastEnemyTarget);
                            if (index < 0 || index >= Targets.Count()) index = 0;
                            Targets.ElementAt(index).Value.button.RadioSelect();
                        }
                        else
                        {
                            index = 0;
                            battleScene.BattleViewModel.LastEnemyTarget = Targets.First().Value.target as BattleEnemy;
                            ((Button)targetList.ChildList.First()).RadioSelect();
                        }
                    }
                    break;

                case TargetType.SingleAlly:
                    {
                        int i = 0;
                        foreach (ModelProperty<TargetButton> buttonPropery in Targets)
                        {
                            buttonPropery.Value.button = targetList.ChildList.ElementAt(i) as Button;
                            i++;
                        }

                        if (battleScene.BattleViewModel.LastPlayerTarget != null)
                        {
                            index = Targets.ToList().FindIndex(x => x.Value.target == battleScene.BattleViewModel.LastPlayerTarget);
                            if (index < 0 || index >= Targets.Count()) index = 0;
                            Targets.ElementAt(index).Value.button.RadioSelect();
                        }
                        else
                        {
                            index = 0;
                            battleScene.BattleViewModel.LastPlayerTarget = Targets.First().Value.target as BattlePlayer;
                            ((Button)targetList.ChildList.First()).RadioSelect();
                        }
                    }
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var input = Input.CurrentInput;
            if (input.CommandPressed(Main.Command.Up)) PreviousTarget();
            else if (input.CommandPressed(Main.Command.Down)) NextTarget();
            else if (input.CommandPressed(Main.Command.Left)) PreviousTarget();
            else if (input.CommandPressed(Main.Command.Right)) NextTarget();
            else if (input.CommandPressed(Main.Command.Confirm) && confirmCooldown <= 0) SelectCurrentTarget();
            else if (input.CommandPressed(Main.Command.Cancel)) Terminate();

            if (confirmCooldown > 0) confirmCooldown -= gameTime.ElapsedGameTime.Milliseconds;
        }

        private void PreviousTarget()
        {
            switch (Command.Targetting)
            {
                case TargetType.Self:
                case TargetType.AllAlly:
                case TargetType.AllEnemy:
                    if (((Button)targetList.ChildList.First()).Selected) return;
                    break;

            }

            Audio.PlaySound(GameSound.menu_select);

            index--;
            if (index < 0) index = Targets.Count() - 1;

            if (Command.Targetting == TargetType.SingleEnemy || Command.Targetting == TargetType.AllEnemy)
            {
                battleScene.BattleViewModel.LastEnemyTarget = Targets.ElementAt(index).Value.target as BattleEnemy;
                Targets.ElementAt(index).Value.button.RadioSelect();

            }
            else if (Command.Targetting == TargetType.SingleAlly || Command.Targetting == TargetType.AllAlly)
            {
                battleScene.BattleViewModel.LastPlayerTarget = Targets.ElementAt(index).Value.target as BattlePlayer;
                Targets.ElementAt(index).Value.button.RadioSelect();
            }
        }

        private void NextTarget()
        {
            switch (Command.Targetting)
            {
                case TargetType.Self:
                case TargetType.AllAlly:
                case TargetType.AllEnemy:
                    if (((Button)targetList.ChildList.First()).Selected) return;
                    break;

            }

            Audio.PlaySound(GameSound.menu_select);

            index++;
            if (index >= Targets.Count()) index = 0;

            if (Command.Targetting == TargetType.SingleEnemy || Command.Targetting == TargetType.AllEnemy)
            {
                if (Command.Targetting == TargetType.SingleEnemy) battleScene.BattleViewModel.LastEnemyTarget = Targets.ElementAt(index).Value.target as BattleEnemy;
                Targets.ElementAt(index).Value.button.RadioSelect();

            }
            else if (Command.Targetting == TargetType.SingleAlly || Command.Targetting == TargetType.AllAlly)
            {
                if (Command.Targetting == TargetType.SingleAlly) battleScene.BattleViewModel.LastPlayerTarget = Targets.ElementAt(index).Value.target as BattlePlayer;
                Targets.ElementAt(index).Value.button.RadioSelect();
            }
        }

        private void SelectCurrentTarget()
        {
            Audio.PlaySound(GameSound.menu_select);

            SelectTarget(Targets.ElementAt(index));
        }

        public void SelectTarget(object parameter)
        {
            if (Command.Charges > 0) Command.Charges--;

            Terminate();
            Player.EndTurn();

            TargetButton targetButton = (TargetButton)((IModelProperty)parameter).GetValue();

            if (Command.Targetting == TargetType.SingleEnemy)
            {
                battleScene.BattleViewModel.LastEnemyTarget = Targets.ToList().Find(x => x.Value.target == targetButton.target).Value.target as BattleEnemy;

            }
            else if (Command.Targetting == TargetType.SingleAlly)
            {
                battleScene.BattleViewModel.LastPlayerTarget = Targets.ToList().Find(x => x.Value.target == targetButton.target).Value.target as BattlePlayer;
            }

            switch (Command.Targetting)
            {
                case TargetType.Self:
                case TargetType.SingleAlly:
                case TargetType.SingleEnemy:
                    {
                        BattleController battleController = new BattleController(battleScene, Player, targetButton.target, Command.Script);
                        battleScene.AddController(battleController);
                    }
                    break;

                case TargetType.AllEnemy:
                    {
                        int delay = 0;
                        foreach (BattleEnemy battleEnemy in battleScene.EnemyList)
                        {                            
                            string[] script = new string[Command.Script.Count() + 1];
                            script[0] = "Wait " + delay;
                            for (int i = 1; i < script.Count(); i++) script[i] = Command.Script[i - 1];
                            delay += 200;

                            BattleController battleController = new BattleController(battleScene, Player, battleEnemy, script);
                            battleScene.AddController(battleController);
                        }
                    }
                    break;

                case TargetType.AllAlly:
                    {
                        int delay = 0;
                        foreach (BattlePlayer battlePlayer in battleScene.PlayerList)
                        {
                            string[] script = new string[Command.Script.Count() + 1];
                            script[0] = "Wait " + delay;
                            for (int i = 1; i < script.Count(); i++) script[i] = Command.Script[i - 1];                            
                            delay += 200;

                            BattleController battleController = new BattleController(battleScene, Player, battlePlayer, script);
                            battleScene.AddController(battleController);
                        }
                    }
                    break;
            }
        }

        public BattlePlayer Player { get; set; }
        public CommandRecord Command { get; set; }
        public ModelCollection<TargetButton> Targets { get; set; } = new ModelCollection<TargetButton>();
    }
}
